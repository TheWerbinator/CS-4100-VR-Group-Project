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
    [SerializeField] private GameObject _hostSide, _clientSide, _hostPaddle, _clientPaddle, _netAirspace;

    void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;
        Debug.Log(collision.gameObject);
        if (collision.gameObject == _hostPaddle) _lastPaddleHit = LastPaddleHit.HostPaddle;
        if (collision.gameObject == _clientPaddle) _lastPaddleHit = LastPaddleHit.ClientPaddle;
        if (collision.gameObject.CompareTag("Floor")) 
        {
            UpdateScore();
            _lastPaddleHit = LastPaddleHit.None;
            _lastTableSideHit = LastTableSideHit.None;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (other == _hostSide) _lastTableSideHit = LastTableSideHit.HostTable;
        if (other == _clientSide) _lastTableSideHit = LastTableSideHit.ClientTable;
    }

    void UpdateScore()
    {
        if (!IsOwner) return;
        if (_lastTableSideHit == LastTableSideHit.None || _lastPaddleHit == LastPaddleHit.None) return;
        if (_lastTableSideHit == LastTableSideHit.HostTable)
        {
            GameManager.Instance.UpdateClientScore();
        }
        if (_lastTableSideHit == LastTableSideHit.ClientTable)
        {
            GameManager.Instance.UpdateHostScore();
        }
    }
}
