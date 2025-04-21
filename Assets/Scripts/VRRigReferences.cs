using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRRigReferences : MonoBehaviour
{
    public static VRRigReferences Singleton { get; private set; }

    [Header("Required References")]
    [SerializeField] private Transform root;
    [SerializeField] private Transform head;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Debug.LogError($"Multiple instances of {nameof(VRRigReferences)} detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        Singleton = this;
        ValidateReferences();
    }

    private void OnDestroy()
    {
        if (Singleton == this)
        {
            Singleton = null;
        }
    }

    private void ValidateReferences()
    {
        if (root == null)
        {
            Debug.LogError($"{nameof(root)} reference is missing in {nameof(VRRigReferences)}");
            enabled = false;
            return;
        }

        if (head == null)
        {
            Debug.LogError($"{nameof(head)} reference is missing in {nameof(VRRigReferences)}");
            enabled = false;
            return;
        }

        if (leftHand == null)
        {
            Debug.LogError($"{nameof(leftHand)} reference is missing in {nameof(VRRigReferences)}");
            enabled = false;
            return;
        }

        if (rightHand == null)
        {
            Debug.LogError($"{nameof(rightHand)} reference is missing in {nameof(VRRigReferences)}");
            enabled = false;
            return;
        }
    }

    public Transform GetRoot() => root;
    public Transform GetHead() => head;
    public Transform GetLeftHand() => leftHand;
    public Transform GetRightHand() => rightHand;

    // Update is called once per frame
    void Update()
    {
        
    }
}
