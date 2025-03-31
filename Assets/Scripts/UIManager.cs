using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void JoinAsClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
