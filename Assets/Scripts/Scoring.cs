using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Scoring : NetworkBehaviour
{
    public enum LastPaddleHit { None, HostPaddle, ClientPaddle };
    public enum LastTableSideHit { None, HostTable, ClientTable };
    private LastPaddleHit _lastPaddleHit = LastPaddleHit.None;
    private LastTableSideHit _lastTableSideHit = LastTableSideHit.None;
    [SerializeField] private GameObject _hostSide, _clientSide, _hostPaddle, _clientPaddle;
    private bool _passedNet;

    void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;
        Debug.Log(collision.gameObject);
        if (collision.gameObject == _hostPaddle) { _lastPaddleHit = LastPaddleHit.HostPaddle; _passedNet = false; }
        if (collision.gameObject == _clientPaddle) { _lastPaddleHit = LastPaddleHit.ClientPaddle; _passedNet = false; }
        if (collision.gameObject.CompareTag("Floor")) 
        {
            // method call to update score
            UpdateScore();
            _lastPaddleHit = LastPaddleHit.None;
            _lastTableSideHit = LastTableSideHit.None;
            _passedNet = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (other == _hostSide) { _lastTableSideHit = LastTableSideHit.HostTable; _passedNet = false; }
        if (other == _clientSide) { _lastTableSideHit = LastTableSideHit.ClientTable; _passedNet = false; }
        if (other.gameObject.CompareTag("Net")) _passedNet = true;
    }

    void UpdateScore()
    {
        if (!IsOwner) return;
        if (_lastTableSideHit == LastTableSideHit.None || _lastPaddleHit = LastPaddleHit.None) return;
        if (_lastTableSideHit == LastTableSideHit.HostTable && _lastPaddleHit == LastPaddleHit.HostPaddle)
        { //update client score
        }
        if (_lastTableSideHit == LastTableSideHit.HostTable && _lastPaddleHit == LastPaddleHit.ClientPaddle)
        { //update client score
        }
        if (_lastTableSideHit == LastTableSideHit.ClientTable && _lastPaddleHit == LastPaddleHit.HostPaddle)
        { //update host score
        }
        if (_lastTableSideHit == LastTableSideHit.ClientTable && _lastPaddleHit == LastPaddleHit.ClientPaddle)
        { //update host score
        }
    }
}
