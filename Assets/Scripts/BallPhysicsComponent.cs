using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysicsComponent : NetworkBehaviour
{
  public enum BallState { Held, Serving, Waiting, Active }
  public BallState ballState = BallState.Held;
  [SerializeField] private float _serveForce = 5f;
  private Rigidbody _rigidbody;

  public override void OnNetworkSpawn()
  {
    _rigidbody = GetComponent<Rigidbody>();
    _rigidbody.useGravity = false;
  }

  void Update()
  {
    if (!IsOwner) return;
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
    if (!IsOwner) return;

    Debug.Log(collision.gameObject);
    if (collision.gameObject.CompareTag("Paddle")) ballState = BallState.Active;
  }

  public void ServeBall()
  {
    if (!IsOwner) return;
    _rigidbody.useGravity = true;
    _rigidbody.velocity = Vector3.up * _serveForce;
    ballState = BallState.Waiting;
  }

  public void ResetBall(Vector3 position)
  {
    if (!IsOwner) return;
    transform.position = position;
    _rigidbody.velocity = Vector3.zero;
    _rigidbody.angularVelocity = Vector3.zero;
    _rigidbody.useGravity = false;
    ballState = BallState.Held;
  }
}
