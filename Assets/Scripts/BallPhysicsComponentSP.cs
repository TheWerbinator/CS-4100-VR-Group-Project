using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysicsComponentSP : NetworkBehaviour
{
  public enum BallState { Held, Serving, Waiting, Active }
  public BallState ballState = BallState.Held;
  [SerializeField] private float _serveForce = 5f;
  [SerializeField] private float assistStrength = 0.1f;
  private string _lastSide = "Home";
  private Rigidbody _rigidbody;
  private Vector3 _opponentDirection = Vector3.forward;
  private Vector3 _initialPosition;

  void Start()
  {
    _initialPosition = transform.position;
    _rigidbody = GetComponent<Rigidbody>();
    _rigidbody.useGravity = false;
  }

  void Update()
  {
    UpdateBall();
  }
  void UpdateBall()
  {
    switch (ballState)
    {
      case BallState.Held: HoldBall(); break;
      case BallState.Serving: ServeBall(); break;
      case BallState.Waiting:
        // insert check for user interaction here
        break;
      case BallState.Active: _rigidbody.useGravity = true; break;
    }
  }

  void FixedUpdate()
  {
    if (ballState == BallState.Active && Vector3.Dot(_rigidbody.velocity, _opponentDirection) > 0)
    {
      float correctiveForceZ = -transform.position.z * assistStrength;
      Vector3 correctiveForce = new Vector3(correctiveForceZ, 0, 0);
      _rigidbody.AddForce(correctiveForce, ForceMode.Acceleration);
    }
  }
  void HoldBall()
  {
    _rigidbody.velocity = Vector3.zero;
    _rigidbody.useGravity = false;

    // if (controller button is pushed)
    // {
    //   ballState = BallState.Serving;
    // }
  }

  void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.CompareTag("Home")) _lastSide = "Home";
    if (collision.gameObject.CompareTag("Guest")) _lastSide = "Guest";
    if (collision.gameObject.CompareTag("BackBoard")) _lastSide = "BackBoard";
    if (collision.gameObject.CompareTag("Floor"))
    {
      ResetBall();
      if (_lastSide == "BackBoard")
      {
        ScoreManager.AddScoreHome();
      }
      else ScoreManager.AddScoreGuest();
    }
    if (collision.gameObject.CompareTag("Paddle")) ballState = BallState.Active;
  }

  public void ServeBall()
  {
    _rigidbody.useGravity = true;
    _rigidbody.velocity = Vector3.up * _serveForce;
    ballState = BallState.Waiting;
  }

  public void ResetBall()
  {
    transform.position = _initialPosition;
    _rigidbody.velocity = Vector3.zero;
    _rigidbody.angularVelocity = Vector3.zero;
    _rigidbody.useGravity = false;
    ballState = BallState.Held;
  }
}
