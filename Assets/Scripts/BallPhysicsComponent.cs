using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysicsComponent : MonoBehaviour
{
  public enum BallState { Held, Serving, Waiting, Active }

  public BallState ballState = BallState.Held;
  public float serveForce = 5f;

  private Rigidbody rb;

  void Start()
  {
    rb = GetComponent<Rigidbody>();
    rb.useGravity = false; // We'll enable it when serving
  }

  void Update()
  {
    switch (ballState)
    {
      case BallState.Held:
        rb.velocity = Vector3.zero;
        rb.useGravity = false;

        // if (controller button is pushed)
        // {
        //   ballState = BallState.Serving;
        // }
        break;

      case BallState.Serving:
        ServeBall();
        break;

      case BallState.Waiting:
        // maybe check for user interaction here
        break;

      case BallState.Active:
        // physics engine handles this
        break;
    }
  }

  void OnCollisionEnter(Collision collision)
  {
    Debug.Log(collision.gameObject);
    if (collision.gameObject.CompareTag("Paddle"))
    {
      ballState = BallState.Active;
    }
  }

  public void ServeBall()
  {
    rb.useGravity = true;
    rb.velocity = Vector3.up * serveForce;
    ballState = BallState.Waiting;
  }

  public void ResetBall(Vector3 position)
  {
    transform.position = position;
    rb.velocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;
    rb.useGravity = false;
    ballState = BallState.Held;
  }
}
