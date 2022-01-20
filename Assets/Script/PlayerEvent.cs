using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Source: https://www.youtube.com/watch?v=ZlKsRlHSxek

public class PlayerEvent : MonoBehaviour
{
    #region Events
    public static UnityAction OnTriggerUp = null;
    public static UnityAction OnTriggerDown = null;
    public static UnityAction<OVRInput.Controller, GameObject> OnControllerSource = null;
    #endregion

    #region Anchors
    public GameObject m_LeftAnchor;
    public GameObject m_RightAnchor;
    public GameObject m_HeadAnchor;
    #endregion

    #region Input
    private Dictionary<OVRInput.Controller, GameObject> m_ControllerSets = null;
    private OVRInput.Controller m_InputSource = OVRInput.Controller.None;
    private OVRInput.Controller m_Controller = OVRInput.Controller.None;
    private bool m_InputActive = true;
    #endregion

    private void Awake()
    {
        OVRManager.HMDMounted += PlayerFound;
        OVRManager.HMDUnmounted += PlayerLost;

        m_ControllerSets = CreateControllerSets();
    }

    private void OnDestroy()
    {
        OVRManager.HMDMounted -= PlayerFound;
        OVRManager.HMDUnmounted -= PlayerLost;
    }

    private void Update()
    {
        // Check for active input
        if (!m_InputActive)
            return;

        // Check if a controller exists
        CheckForController();

        // Check for input source
        CheckInputSource();

        // Check for actual input
        Input();
    }

    private void CheckForController()
    {
        OVRInput.Controller controllerCheck = m_Controller;

        // Right touch
        if (OVRInput.IsControllerConnected(OVRInput.Controller.RTouch))
        {
            controllerCheck = OVRInput.Controller.RTouch;
        }

        // Left touch
        else if (OVRInput.IsControllerConnected(OVRInput.Controller.LTouch))
        {
            controllerCheck = OVRInput.Controller.LTouch;
        }

        else if (OVRInput.IsControllerConnected(OVRInput.Controller.Touch))
        {
            if (AppManager.instance.isRightHanded)
            {
                controllerCheck = OVRInput.Controller.RTouch;
            }
            else
            {
                controllerCheck = OVRInput.Controller.LTouch;
            }
        }

        // If no controllers, headset
        else
        {
            controllerCheck = OVRInput.Controller.None;
        }

        // Update
        m_Controller = UpdateSource(controllerCheck, m_Controller);
    }

    private void CheckInputSource()
    {
        // Update
        m_InputSource = UpdateSource(OVRInput.GetActiveController(), m_InputSource);
    }

    private void Input()
    {
        // Dual Controllers
        if (AppManager.instance.activeController == OVRInput.Controller.Touch)
        {
            if (!AppManager.instance.isRightHanded)
            {
                // Detect trigger press input on left controller
                // Trigger down
                if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
                {
                    if (OnTriggerDown != null)
                    {
                        OnTriggerDown();
                    }
                }

                // Trigger up
                if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
                {
                    if (OnTriggerUp != null)
                    {
                        OnTriggerUp();
                    }
                }
            }
            else
            {
                // Detect trigger press input on right controller
                // Trigger down
                if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
                {
                    if (OnTriggerDown != null)
                    {
                        OnTriggerDown();
                    }
                }

                // Trigger up
                if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
                {
                    if (OnTriggerUp != null)
                    {
                        OnTriggerUp();
                    }
                }
            }
        }
        // Individual Controller
        else
        {
            // Trigger down
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                if (OnTriggerDown != null)
                {
                    OnTriggerDown();
                }
            }

            // Trigger up
            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
            {
                if (OnTriggerUp != null)
                {
                    OnTriggerUp();
                }
            }
        }
       
    }

    private OVRInput.Controller UpdateSource(OVRInput.Controller check, OVRInput.Controller previous)
    {
        // If values are the same, return
        if (check == previous)
            return previous;


        // (DIY) if both controller are in use, switch to default one
        if (check == OVRInput.Controller.Touch)
        {
            if (AppManager.instance.isRightHanded)
            {
                check = OVRInput.Controller.RTouch;
            }
            else
            {
                check = OVRInput.Controller.LTouch;
            }
        }

        // Get controller object
        GameObject controllerObject = null;
        m_ControllerSets.TryGetValue(check, out controllerObject);

        // If no controller, set to the head
        if (controllerObject == null)
            controllerObject = m_HeadAnchor;

        // Send out event
        if (OnControllerSource != null)
            OnControllerSource(check, controllerObject);

        // Return new value
        return check;
    }

    private void PlayerFound()
    {
        m_InputActive = true;
    }

    private void PlayerLost()
    {
        m_InputActive = false;
    }

    private Dictionary<OVRInput.Controller, GameObject> CreateControllerSets()
    {
        Dictionary<OVRInput.Controller, GameObject> newSets;

        newSets = new Dictionary<OVRInput.Controller, GameObject>()
            {
                { OVRInput.Controller.LTouch, m_LeftAnchor },
                { OVRInput.Controller.RTouch, m_RightAnchor },
                { OVRInput.Controller.None, m_HeadAnchor }
            };

        return newSets;
    }
}
