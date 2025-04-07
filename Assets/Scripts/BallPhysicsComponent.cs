using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysicsComponent : NetworkBehaviour 
{
  public enum BallState { Held, Serving, Waiting, Active }
  public BallState ballState = BallState.Held;
  [SerializeField] private float _serveForce = 5f;
  private Rigidbody _rb;


  public override void OnNetworkSpawn()
  {
    _rb = GetComponent<Rigidbody>();
    _rb.useGravity = false;
  }

  void Update()
  {
    if (!IsOwner)
    {
        return;
    }

        switch (ballState)
    {
      case BallState.Held:
        _rb.velocity = Vector3.zero;
        _rb.useGravity = false;

        // if (controller button is pushed)
        // {
        //   ballState = BallState.Serving;
        // }
        break;

      case BallState.Serving:
        ServeBall();
        break;

      case BallState.Waiting:
        // check for user interaction here
        break;

      case BallState.Active:
        _rb.useGravity = true;
        break;
    }
  }

  void OnCollisionEnter(Collision collision)
  {
    if (!IsOwner)
    {
        return;
    }
        Debug.Log(collision.gameObject);
    if (collision.gameObject.CompareTag("Paddle"))
    {
      ballState = BallState.Active;
    }
  }

  public void ServeBall()
  {
    if (!IsOwner)
    {
        return;
    }
    _rb.useGravity = true;
    _rb.velocity = Vector3.up * _serveForce;
    ballState = BallState.Waiting;
  }

  public void ResetBall(Vector3 position)
  {
    if (!IsOwner)
    {
        return;
    }
    transform.position = position;
    _rb.velocity = Vector3.zero;
    _rb.angularVelocity = Vector3.zero;
    _rb.useGravity = false;
    ballState = BallState.Held;
  }
}
