using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

public class Scoring : NetworkBehaviour
{
    public enum LastPaddleHit { None, HostPaddle, ClientPaddle };
    public enum LastTableSideHit { None, HostTable, ClientTable };
    private LastPaddleHit _lastPaddleHit = LastPaddleHit.None;
    private LastTableSideHit _lastTableSideHit = LastTableSideHit.None;
    [SerializeField] private GameObject _hostSide, _clientSide, _hostPaddle, _clientPaddle;
    [SerializeField] private float hapticIntensity = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;
    private XRBaseController _leftController;
    private XRBaseController _rightController;

    void Start()
    {
        FindControllers();
    }

    private void FindControllers()
    {
        var controllers = FindObjectsOfType<XRBaseController>();
        foreach (var controller in controllers)
        {
            if (controller.controllerNode == UnityEngine.XR.XRNode.LeftHand)
            {
                _leftController = controller;
            }
            else if (controller.controllerNode == UnityEngine.XR.XRNode.RightHand)
            {
                _rightController = controller;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;
        
        if (collision.gameObject == _hostPaddle)
        {
            _lastPaddleHit = LastPaddleHit.HostPaddle;
            SendHapticFeedback(hapticIntensity * 0.3f, hapticDuration * 0.3f);
        }
        if (collision.gameObject == _clientPaddle)
        {
            _lastPaddleHit = LastPaddleHit.ClientPaddle;
            SendHapticFeedback(hapticIntensity * 0.3f, hapticDuration * 0.3f);
        }
        if (collision.gameObject.CompareTag("Floor")) 
        {
            UpdateScore();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        
        if (other == _hostSide)
        {
            if (_lastTableSideHit == LastTableSideHit.HostTable)
            {
                UpdateScore();
            }
            _lastTableSideHit = LastTableSideHit.HostTable;
        }
        if (other == _clientSide)
        {
            if (_lastTableSideHit == LastTableSideHit.ClientTable)
            {
                UpdateScore();
            }
            _lastTableSideHit = LastTableSideHit.ClientTable;
        }
    }

    void UpdateScore()
    {
        if (!IsOwner) return;
        if (_lastTableSideHit == LastTableSideHit.None || _lastPaddleHit == LastPaddleHit.None) return;
        
        SendHapticFeedback(hapticIntensity, hapticDuration);
        
        if (_lastTableSideHit == LastTableSideHit.HostTable)
        {
            GameManager.Instance.UpdateClientScore();
        }
        if (_lastTableSideHit == LastTableSideHit.ClientTable)
        {
            GameManager.Instance.UpdateHostScore();
        }
        
        _lastPaddleHit = LastPaddleHit.None;
        _lastTableSideHit = LastTableSideHit.None;
    }

    private void SendHapticFeedback(float intensity, float duration)
    {
        if (_leftController != null)
        {
            _leftController.SendHapticImpulse(intensity, duration);
        }
        if (_rightController != null)
        {
            _rightController.SendHapticImpulse(intensity, duration);
        }
    }
}
