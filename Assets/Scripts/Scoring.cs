using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Scoring : NetworkBehaviour
{
    public enum LastTableSideHit { None, HostTable, ClientTable };
    private LastTableSideHit _lastTableSideHit = LastTableSideHit.None;

    void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;
        if (collision.gameObject.CompareTag("Floor")) 
        {
            Debug.Log("Floor hit");
            UpdateScore();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (other.gameObject.CompareTag("Home")) _lastTableSideHit = LastTableSideHit.HostTable;
        if (other.gameObject.CompareTag("Guest")) _lastTableSideHit = LastTableSideHit.ClientTable;
    }

    void UpdateScore()
    {
        if (!IsOwner) return;
        if (_lastTableSideHit == LastTableSideHit.None) 
        {
            GameManager.Instance.NoScore();
        }
        if (_lastTableSideHit == LastTableSideHit.HostTable)
        {
            _lastTableSideHit = LastTableSideHit.None;
            GameManager.Instance.UpdateClientScore();
        }
        if (_lastTableSideHit == LastTableSideHit.ClientTable)
        {
            _lastTableSideHit = LastTableSideHit.None;
            GameManager.Instance.UpdateHostScore();
        }
    }
}
