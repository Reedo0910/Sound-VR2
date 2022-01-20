using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedToPlayer : MonoBehaviour
{
    private GameObject playerObject;

    public bool isPositionFixed = true;
    public bool isRotationFixed = true;
    public bool isRevertY = false;
    // Start is called before the first frame update
    void Start()
    {
        playerObject = GameObject.Find("OVRPlayerController");
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotationFixed)
        {
            var rotationVector = transform.rotation.eulerAngles;
            if (isRevertY)
            {
                rotationVector.y = -playerObject.transform.rotation.eulerAngles.y;
            }
            else
            {
                rotationVector.y = playerObject.transform.rotation.eulerAngles.y;
            }
           
            transform.rotation = Quaternion.Euler(rotationVector);
        }

        if (isPositionFixed)
        {
            var positionVector = transform.position;
            transform.position = new Vector3(playerObject.transform.position.x, positionVector.y, playerObject.transform.position.z);
        }
    }
}
