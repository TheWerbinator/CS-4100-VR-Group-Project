using System;
using UnityEngine;

public class BallPhysicsComponent : MonoBehaviour
{
  public enum BallState { Held, Serving, Waiting, Active }
  private float _gravity = 9.89f;
  private Vector3 _velocity = new Vector3(0, 0, 0);
  private Rigidbody _body;
  private BallState _ballState;
  // Start is called before the first frame update
  void Start()
  {
    _velocity = Vector3.down * _gravity;
    _body = GetComponent<Rigidbody>();
    // BallState = this.BallState;
    _ballState = BallState.Held;
  }

  // Update is called once per frame
  void Update()
  {
    if (_ballState == BallState.Held || BallState.Waiting) return;
    if (_ballState == BallState.Serve)
    {
      _velocity += _gravity * Time.deltaTime;
      _body.position += _velocity * Time.deltaTime;
      //if The position of the ball is far enough up in the air
      _ballState = BallState.Waiting;
    }

  }
}
