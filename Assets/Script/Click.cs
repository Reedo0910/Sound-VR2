
using UnityEngine;
using UnityEngine.XR;
using System;

public class Click : MonoBehaviour
{
    public Animator Anim;
    private string AnimName;

    

    // Use this for initialization
    void Start()
    {

    }

    void Update()
    {
        Vector3 rotation;
        Quaternion rotationQ;

        InputDevice device = InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head);
        //Debug.Log (rotation);
        if (device.isValid)
        {
            if (device.TryGetFeatureValue(CommonUsages.centerEyeRotation, out rotationQ))
            {
                rotation = rotationQ.eulerAngles;
            }
        }
        // This is the fail case, where there was no center eye was available.
        rotation = Quaternion.identity.eulerAngles;
    }

    // Update is called once per frame

    public void onClick ()
    {
        AnimName = gameObject.name;
        //Debug.Log(AnimName);
        Anim.Play(AnimName);
        //Debug.Log("clicked");

       
    }

}
