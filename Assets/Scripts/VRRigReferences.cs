using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRRigReferences : MonoBehaviour
{
    public static VRRigReferences Singleton;

    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public void Awake()
    {
        Singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
