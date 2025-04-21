using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private Transform head;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    public Renderer[] meshToDisable;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
            foreach (var item in meshToDisable)
                item.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            root.position = VRRigReferences.Singleton.root.position;
            root.rotation = VRRigReferences.Singleton.root.rotation;

            head.position = VRRigReferences.Singleton.head.position;
            head.rotation = VRRigReferences.Singleton.head.rotation;

            leftHand.position = VRRigReferences.Singleton.leftHand.position;
            leftHand.rotation = VRRigReferences.Singleton.leftHand.rotation;

            rightHand.position = VRRigReferences.Singleton.rightHand.position;
            rightHand.rotation = VRRigReferences.Singleton.rightHand.rotation;
        }
    }

    public void SetPlayer2Position()
    {
        if (IsOwner)
        {
            root.position = new Vector3(-VRRigReferences.Singleton.root.position.x, VRRigReferences.Singleton.root.position.y, VRRigReferences.Singleton.root.position.z);
            root.rotation = Quaternion.Euler(0, 180, 0) * VRRigReferences.Singleton.root.rotation;
            head.position = new Vector3(-VRRigReferences.Singleton.head.position.x, VRRigReferences.Singleton.head.position.y, VRRigReferences.Singleton.head.position.z);
            head.rotation = Quaternion.Euler(0, 180, 0) * VRRigReferences.Singleton.head.rotation;
            leftHand.position = new Vector3(-VRRigReferences.Singleton.leftHand.position.x, VRRigReferences.Singleton.leftHand.position.y, VRRigReferences.Singleton.leftHand.position.z);
            leftHand.rotation = Quaternion.Euler(0, 180, 0) * VRRigReferences.Singleton.leftHand.rotation;
            rightHand.position = new Vector3(-VRRigReferences.Singleton.rightHand.position.x, VRRigReferences.Singleton.rightHand.position.y, VRRigReferences.Singleton.rightHand.position.z);
            rightHand.rotation = Quaternion.Euler(0, 180, 0) * VRRigReferences.Singleton.rightHand.rotation;
        }
    }
}
