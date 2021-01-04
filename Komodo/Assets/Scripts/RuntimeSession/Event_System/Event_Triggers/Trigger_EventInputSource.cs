using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This script is meant to be an inputsource for interacting with UI elements with GraphicsRaycaster and world objects with PhysicsRaycaster 
/// </summary>
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PhysicsRaycaster))]
public class Trigger_EventInputSource : MonoBehaviour
{
    [HideInInspector] public Camera eventCamera;
    [HideInInspector] public Transform thisTransform;
    [HideInInspector] public LineRenderer thisLineRenderer;
    [HideInInspector] public PhysicsRaycaster physicsRaycaster;
    public bool usePhysicsRaycast;
    public bool useLineRenderer;

    //obtain References
    public void Awake() => (thisTransform, thisLineRenderer, eventCamera, physicsRaycaster) = (transform, GetComponent<LineRenderer>(), GetComponent<Camera>(), GetComponent<PhysicsRaycaster>());

    //determine if we should use physicsRayster
    public void Start() {
        if (usePhysicsRaycast) physicsRaycaster.enabled = true; else physicsRaycaster.enabled = false;
        if (useLineRenderer) thisLineRenderer.enabled = true; else thisLineRenderer.enabled = false;
    }

    public void OnEnable() 
    {
        if (EventSystemManager.IsAlive)
            EventSystemManager.Instance.AddInputSource(this);
    }
    // THIS IS WHERE FUNCTIONS ARE INVOKED(ON RELEASE OF TRIGGER BUTTON WHICH DEACTIVATES PARENT OBJECT
    public void OnDisable()
    {
        if (EventSystemManager.IsAlive)
            EventSystemManager.Instance.RemoveInputSourveAndSendClickAndDownEvent(this);
    }

    public void UpdateLineRenerer(Vector3 startPosition, Vector3 endPosition)
    {
        if (useLineRenderer)
        {
            thisLineRenderer.SetPosition(0, startPosition);
            thisLineRenderer.SetPosition(1, endPosition);
        }
    }

}
