using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Required References")]
    [SerializeField] private Transform root;
    [SerializeField] private Transform head;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    [Header("Visual Settings")]
    [SerializeField] private Renderer[] meshToDisable;

    private const float NETWORK_UPDATE_RATE = 0.05f; // 20 updates per second
    private float lastNetworkUpdateTime;
    private Vector3 lastRootPosition;
    private Quaternion lastRootRotation;
    private Vector3 lastHeadPosition;
    private Quaternion lastHeadRotation;
    private Vector3 lastLeftHandPosition;
    private Quaternion lastLeftHandRotation;
    private Vector3 lastRightHandPosition;
    private Quaternion lastRightHandRotation;

    private void Awake()
    {
        ValidateReferences();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            if (meshToDisable != null)
            {
                foreach (var renderer in meshToDisable)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = false;
                    }
                }
            }
        }
    }

    private void ValidateReferences()
    {
        if (root == null)
        {
            Debug.LogError($"{nameof(root)} reference is missing in {nameof(NetworkPlayer)}");
            enabled = false;
            return;
        }

        if (head == null)
        {
            Debug.LogError($"{nameof(head)} reference is missing in {nameof(NetworkPlayer)}");
            enabled = false;
            return;
        }

        if (leftHand == null)
        {
            Debug.LogError($"{nameof(leftHand)} reference is missing in {nameof(NetworkPlayer)}");
            enabled = false;
            return;
        }

        if (rightHand == null)
        {
            Debug.LogError($"{nameof(rightHand)} reference is missing in {nameof(NetworkPlayer)}");
            enabled = false;
            return;
        }

        if (meshToDisable == null || meshToDisable.Length == 0)
        {
            Debug.LogWarning($"{nameof(meshToDisable)} array is empty in {nameof(NetworkPlayer)}");
        }
    }

    private void Update()
    {
        if (!IsOwner || VRRigReferences.Singleton == null) return;

        // Update local transforms
        UpdateLocalTransforms();

        // Network synchronization
        if (Time.time - lastNetworkUpdateTime >= NETWORK_UPDATE_RATE)
        {
            SynchronizeNetworkTransforms();
            lastNetworkUpdateTime = Time.time;
        }
    }

    private void UpdateLocalTransforms()
    {
        try
        {
            root.position = VRRigReferences.Singleton.GetRoot().position;
            root.rotation = VRRigReferences.Singleton.GetRoot().rotation;

            head.position = VRRigReferences.Singleton.GetHead().position;
            head.rotation = VRRigReferences.Singleton.GetHead().rotation;

            leftHand.position = VRRigReferences.Singleton.GetLeftHand().position;
            leftHand.rotation = VRRigReferences.Singleton.GetLeftHand().rotation;

            rightHand.position = VRRigReferences.Singleton.GetRightHand().position;
            rightHand.rotation = VRRigReferences.Singleton.GetRightHand().rotation;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update local transforms: {e.Message}");
        }
    }

    private void SynchronizeNetworkTransforms()
    {
        if (IsServer)
        {
            // Server-side: Update all clients
            UpdateClientTransformsClientRpc(
                root.position, root.rotation,
                head.position, head.rotation,
                leftHand.position, leftHand.rotation,
                rightHand.position, rightHand.rotation
            );
        }
        else
        {
            // Client-side: Send updates to server
            UpdateServerTransformsServerRpc(
                root.position, root.rotation,
                head.position, head.rotation,
                leftHand.position, leftHand.rotation,
                rightHand.position, rightHand.rotation
            );
        }
    }

    [ServerRpc]
    private void UpdateServerTransformsServerRpc(
        Vector3 rootPos, Quaternion rootRot,
        Vector3 headPos, Quaternion headRot,
        Vector3 leftHandPos, Quaternion leftHandRot,
        Vector3 rightHandPos, Quaternion rightHandRot)
    {
        // Server validates and broadcasts updates
        UpdateClientTransformsClientRpc(
            rootPos, rootRot,
            headPos, headRot,
            leftHandPos, leftHandRot,
            rightHandPos, rightHandRot
        );
    }

    [ClientRpc]
    private void UpdateClientTransformsClientRpc(
        Vector3 rootPos, Quaternion rootRot,
        Vector3 headPos, Quaternion headRot,
        Vector3 leftHandPos, Quaternion leftHandRot,
        Vector3 rightHandPos, Quaternion rightHandRot)
    {
        if (IsOwner) return; // Don't update owner's transforms

        // Store last known values for interpolation
        lastRootPosition = root.position;
        lastRootRotation = root.rotation;
        lastHeadPosition = head.position;
        lastHeadRotation = head.rotation;
        lastLeftHandPosition = leftHand.position;
        lastLeftHandRotation = leftHand.rotation;
        lastRightHandPosition = rightHand.position;
        lastRightHandRotation = rightHand.rotation;

        // Update transforms
        root.position = rootPos;
        root.rotation = rootRot;
        head.position = headPos;
        head.rotation = headRot;
        leftHand.position = leftHandPos;
        leftHand.rotation = leftHandRot;
        rightHand.position = rightHandPos;
        rightHand.rotation = rightHandRot;
    }
}
