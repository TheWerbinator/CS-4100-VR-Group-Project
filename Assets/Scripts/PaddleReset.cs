using UnityEngine;
using Unity.Netcode;

public class PaddleReset : NetworkBehaviour
{
    private Vector3 _startPosition;
    private Rigidbody _rigidbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _startPosition = transform.position;
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void ResetPaddle()
    {
        if (!IsOwner) return;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        transform.position = _startPosition;
        transform.rotation = Quaternion.identity;
    }
}
