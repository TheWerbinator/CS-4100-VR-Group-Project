using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysicsComponent : MonoBehaviour
{
  [SerializeField] private float _serveForce = 5f;
  private enum _BallState { Held, Serving, Waiting, Active }
  private _BallState _ballState = _BallState.Held;
  private Rigidbody _rb;

  void Start()
  {
    _rb = GetComponent<Rigidbody>();
    _rb.useGravity = false;
  }

  void Update()
  {
    switch (_ballState)
    {
      case _BallState.Held:
        _rb.velocity = Vector3.zero;
        _rb.useGravity = false;

        // if (controller button is pushed)
        // {
        //   ballState = BallState.Serving;
        // }
        break;

      case _BallState.Serving:
        _rb.useGravity = true;
        _rb.velocity = Vector3.up * _serveForce;
        _ballState = _BallState.Waiting;
        break;

      case _BallState.Waiting:
        // insert check for user interaction here
        break;

      case _BallState.Active:
        _rb.useGravity = true;
        break;
    }
  }

  void OnCollisionEnter(Collision collision)
  {
    Debug.Log(collision.gameObject);
    if (collision.gameObject.CompareTag("Paddle"))
    {
      _ballState = _BallState.Active;
    }
  }

  public void ResetBall(Vector3 position)
  {
    transform.position = position;
    _rb.velocity = Vector3.zero;
    _rb.angularVelocity = Vector3.zero;
    _rb.useGravity = false;
    _ballState = _BallState.Held;
  }
}
