using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Source: https://www.youtube.com/watch?v=ZlKsRlHSxek

public class Pointer : MonoBehaviour
{
    public float m_Distance = 10.0f;
    public LineRenderer m_LineRenderer = null;
    public LayerMask m_EverythingMask = 0;
    public LayerMask m_InteractableMask = 0;
    public UnityAction<Vector3, GameObject> OnPointerUpdate = null;

    private Transform m_CurrentOrigin = null;
    private GameObject m_CurrentObject = null;

    private void Awake()
    {
        PlayerEvent.OnControllerSource += UpdateOrigin;
        PlayerEvent.OnTriggerDown += ProcessTriggerDown;
    }

    private void Start()
    {
        SetLineColor();

        m_LineRenderer.enabled = true;

    }

    private void OnDestroy()
    {
        PlayerEvent.OnControllerSource -= UpdateOrigin;
        PlayerEvent.OnTriggerDown -= ProcessTriggerDown;
    }

    private void Update()
    {
        if (!m_CurrentOrigin)
        {
            return;
        }

        Vector3 hitPoint = UpdateLine();

        m_CurrentObject = UpdatePointerStatus();

        if (OnPointerUpdate != null)
        {
            OnPointerUpdate(hitPoint, m_CurrentObject);
        }
    }

    private Vector3 UpdateLine()
    {
        // Create ray
        RaycastHit hit = CreateRaycast(m_EverythingMask);

        // Default end
        Vector3 endPosition = m_CurrentOrigin.position + (m_CurrentOrigin.forward * m_Distance);

        // Check hit
        if (hit.collider != null)
        {
            //Debug.Log("myVr:" + hit.point);
            endPosition = hit.point;
        }

        // Set position
        m_LineRenderer.SetPosition(0, m_CurrentOrigin.position);
        m_LineRenderer.SetPosition(1, endPosition);

        return endPosition;
    }

    private RaycastHit CreateRaycast(int layer)
    {
        RaycastHit hit;
        Ray ray = new Ray(m_CurrentOrigin.position, m_CurrentOrigin.forward);
        Physics.Raycast(ray, out hit, m_Distance, layer);

        return hit;
    }

    private void SetLineColor()
    {
        if (!m_LineRenderer)
        {
            return;
        }

        Color endColor = Color.white;
        endColor.a = 0.0f;

        m_LineRenderer.endColor = endColor;
    }

    private void UpdateOrigin(OVRInput.Controller controller, GameObject controllerObject)
    {
        // Set origin of pointer
        m_CurrentOrigin = controllerObject.transform;

        // Is the laser visible
        if (controller == OVRInput.Controller.None)
        {
            m_LineRenderer.enabled = false;
        }
        else
        {
            m_LineRenderer.enabled = true;
        }
    }

    private GameObject UpdatePointerStatus()
    {
        // create ray
        RaycastHit hit = CreateRaycast(m_InteractableMask);

        // check hit
        if (hit.collider)
        {
            //Debug.Log("myVr:" + hit.collider.gameObject);

            return hit.collider.gameObject;
        }

        // return
        return null;
    }

    private void ProcessTriggerDown()
    {
        if (!m_CurrentObject)
        {
            return;
        }

        Interactable interactable = m_CurrentObject.GetComponent<Interactable>();
        interactable.Pressed();
    }
}
