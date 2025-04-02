using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RigManager : NetworkBehaviour
{
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (OwnerClientId == 0)
        {
            transform.position = new Vector3(1.35f, 0, 0);
        }
        if (OwnerClientId == 1)
        {
            transform.position = new Vector3(-1.73f, 0, 0);
            transform.Rotate(0, 90, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
