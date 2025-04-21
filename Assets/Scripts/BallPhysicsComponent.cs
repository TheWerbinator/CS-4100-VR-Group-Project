using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysicsComponent : NetworkBehaviour
{
  public enum BallState { Held, Serving, Waiting, Active }
  public NetworkVariable<BallState> NetworkBallState = new NetworkVariable<BallState>(BallState.Held);
  public BallState ballState = BallState.Held;
  [SerializeField] private float _serveForce = 5f;
  [SerializeField] private float assistStrength = 0.1f;
  [SerializeField] private float maxVelocity = 20f;
  [SerializeField] private float velocitySmoothing = 0.1f;
  [SerializeField] private float minVelocity = 2f;
  [SerializeField] private float hapticIntensity = 0.5f;
  [SerializeField] private float hapticDuration = 0.1f;
  private NetworkVariable<string> _networkLastSide = new NetworkVariable<string>("Home");
  private string _lastSide { get => _networkLastSide.Value; set => _networkLastSide.Value = value; }
  private Rigidbody _rigidbody;
  private Vector3 _opponentDirection = Vector3.forward;
  private Vector3 _initialPosition;
  private bool _isServing = false;
  private Vector3 _targetVelocity;
  private float _lastCollisionTime;
  private const float COLLISION_COOLDOWN = 0.1f;
  private bool _isInitialized = false;
  private XRBaseController _leftController;
  private XRBaseController _rightController;
  private bool _isGrabbed = false;

  void Start()
  {
    InitializeComponent();
    FindControllers();
  }

  private void FindControllers()
  {
    var controllers = FindObjectsOfType<XRBaseController>();
    foreach (var controller in controllers)
    {
      if (controller.controllerNode == UnityEngine.XR.XRNode.LeftHand)
      {
        _leftController = controller;
      }
      else if (controller.controllerNode == UnityEngine.XR.XRNode.RightHand)
      {
        _rightController = controller;
      }
    }
  }

  private void InitializeComponent()
  {
    if (_isInitialized) return;

    _initialPosition = transform.position;
    _rigidbody = GetComponent<Rigidbody>();
    if (_rigidbody == null)
    {
      Debug.LogError("BallPhysicsComponent requires a Rigidbody component");
      enabled = false;
      return;
    }
    _rigidbody.useGravity = false;
    
    // Subscribe to network state changes
    NetworkBallState.OnValueChanged += OnBallStateChanged;
    _isInitialized = true;
  }

  void OnDestroy()
  {
    if (_isInitialized)
    {
      NetworkBallState.OnValueChanged -= OnBallStateChanged;
    }
  }

  private void OnBallStateChanged(BallState previous, BallState current)
  {
    if (!_isInitialized) return;
    
    ballState = current;
    UpdateBallState();
  }

  void Update()
  {
    if (!_isInitialized) return;
    if (!IsOwner) return;
    UpdateBall();
  }

  void UpdateBall()
  {
    if (!_isInitialized) return;

    switch (ballState)
    {
      case BallState.Held: HoldBall(); break;
      case BallState.Serving: ServeBall(); break;
      case BallState.Waiting:
        // Check for trigger press on either controller
        if ((_leftController != null && _leftController.selectInteractionState.active) ||
            (_rightController != null && _rightController.selectInteractionState.active))
        {
          SetBallState(BallState.Serving);
        }
        break;
      case BallState.Active: 
        _rigidbody.useGravity = true;
        break;
    }
  }

  void FixedUpdate()
  {
    if (!_isInitialized) return;
    if (ballState == BallState.Active)
    {
      // Smooth velocity changes
      _targetVelocity = _rigidbody.velocity;
      
      // Ensure minimum velocity
      if (_targetVelocity.magnitude < minVelocity && _targetVelocity.magnitude > 0)
      {
        _targetVelocity = _targetVelocity.normalized * minVelocity;
      }
      
      // Limit maximum velocity
      if (_targetVelocity.magnitude > maxVelocity)
      {
        _targetVelocity = _targetVelocity.normalized * maxVelocity;
      }
      
      _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, _targetVelocity, velocitySmoothing);

      // Apply corrective force when moving towards opponent
      if (Vector3.Dot(_rigidbody.velocity, _opponentDirection) > 0)
      {
        float correctiveForceZ = -transform.position.z * assistStrength;
        Vector3 correctiveForce = new Vector3(0, 0, correctiveForceZ);
        _rigidbody.AddForce(correctiveForce, ForceMode.Acceleration);
      }
    }
  }

  void HoldBall()
  {
    if (!_isInitialized) return;
    
    _rigidbody.velocity = Vector3.zero;
    _rigidbody.useGravity = false;
    _rigidbody.angularVelocity = Vector3.zero;
  }

  void OnCollisionEnter(Collision collision)
  {
    if (!_isInitialized) return;
    if (!IsOwner) return;
    if (Time.time - _lastCollisionTime < COLLISION_COOLDOWN) return;
    _lastCollisionTime = Time.time;

    // Send haptic feedback on collision
    SendHapticFeedback();

    if (collision.gameObject.CompareTag("Home")) _lastSide = "Home";
    if (collision.gameObject.CompareTag("Guest")) _lastSide = "Guest";
    if (collision.gameObject.CompareTag("BackBoard")) _lastSide = "BackBoard";
    
    if (collision.gameObject.CompareTag("Floor"))
    {
      if (_lastSide == "BackBoard")
      {
        ScoreManager.AddScoreHome();
      }
      else if (_lastSide == "Guest")
      {
        ScoreManager.AddScoreGuest();
      }
      ResetBall();
    }
    
    if (collision.gameObject.CompareTag("Paddle"))
    {
      SetBallState(BallState.Active);
      
      // Calculate reflection direction
      Vector3 reflection = Vector3.Reflect(_rigidbody.velocity.normalized, collision.contacts[0].normal);
      
      // Add a small random force to make the game more interesting
      Vector3 randomForce = new Vector3(
        Random.Range(-0.5f, 0.5f),
        Random.Range(0.1f, 0.3f),
        Random.Range(-0.5f, 0.5f)
      );
      
      // Combine reflection and random force
      Vector3 finalForce = (reflection + randomForce).normalized * Mathf.Max(_rigidbody.velocity.magnitude, minVelocity);
      _rigidbody.velocity = finalForce;
    }
  }

  private void SendHapticFeedback()
  {
    if (_leftController != null)
    {
      _leftController.SendHapticImpulse(hapticIntensity, hapticDuration);
    }
    if (_rightController != null)
    {
      _rightController.SendHapticImpulse(hapticIntensity, hapticDuration);
    }
  }

  public void ServeBall()
  {
    if (!_isInitialized) return;
    if (!IsOwner) return;
    if (_isServing) return;

    _isServing = true;
    _rigidbody.useGravity = true;
    _rigidbody.velocity = Vector3.up * _serveForce;
    SetBallState(BallState.Waiting);
  }

  public void ResetBall()
  {
    if (!_isInitialized) return;
    if (!IsOwner) return;
    
    transform.position = _initialPosition;
    _rigidbody.velocity = Vector3.zero;
    _rigidbody.angularVelocity = Vector3.zero;
    _rigidbody.useGravity = false;
    _isServing = false;
    SetBallState(BallState.Held);
  }

  private void SetBallState(BallState newState)
  {
    if (!_isInitialized) return;
    
    if (IsServer)
    {
      NetworkBallState.Value = newState;
    }
    else
    {
      SetBallStateServerRpc(newState);
    }
  }

  [ServerRpc]
  private void SetBallStateServerRpc(BallState newState)
  {
    if (!_isInitialized) return;
    NetworkBallState.Value = newState;
  }

  private void UpdateBallState()
  {
    if (!_isInitialized) return;
    
    switch (ballState)
    {
      case BallState.Held:
        HoldBall();
        break;
      case BallState.Active:
        _rigidbody.useGravity = true;
        break;
    }
  }

  // Called by XR Grab Interactable when the ball is grabbed
  public void OnGrabbed()
  {
    _isGrabbed = true;
    SetBallState(BallState.Held);
  }

  // Called by XR Grab Interactable when the ball is released
  public void OnReleased()
  {
    _isGrabbed = false;
    if (ballState == BallState.Held)
    {
      SetBallState(BallState.Waiting);
    }
  }
}
