using UnityEngine;
using UnityEngine.EventSystems;

namespace Komodo.Runtime
{
    /// <summary>
    /// This script is meant to be an inputsource for interacting with UI elements with GraphicsRaycaster and world objects with PhysicsRaycaster 
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(PhysicsRaycaster))]
    public class TriggerEventInputSource : MonoBehaviour
    {
        [HideInInspector] public Camera eventCamera;
        [HideInInspector] public Transform thisTransform;
        [HideInInspector] public LineRenderer thisLineRenderer;
        [HideInInspector] public PhysicsRaycaster physicsRaycaster;
        public bool usePhysicsRaycast;
        public bool useLineRenderer;

        //obtain References
        public void Awake() =>
            (thisTransform, thisLineRenderer, eventCamera, physicsRaycaster) = (transform, GetComponent<LineRenderer>(), GetComponent<Camera>(), GetComponent<PhysicsRaycaster>());

        public void Start()
        {
            //determine if we should use physicsRaycaster
            if (usePhysicsRaycast)
            {
                physicsRaycaster.enabled = true;
            }
            else
            {
                physicsRaycaster.enabled = false;
            }

            if (useLineRenderer)
            {
                thisLineRenderer.enabled = true;
            }
            else
            {
                thisLineRenderer.enabled = false;
            }

           ///to get ghost funcionality we need to add input source, we leave this gameobject active and then set it off to 
            transform.parent.gameObject.SetActive(false);
        }

        public void OnEnable()
        {
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.xrStandaloneInput.RegisterInputSource(this, true);
            }
        }

        // THIS IS WHERE FUNCTIONS ARE INVOKED (ON RELEASE OF TRIGGER BUTTON WHICH DEACTIVATES PARENT OBJECT
        public void OnDisable()
        {
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.RemoveInputSourceAndSendClickAndDownEvent(this);
            }
        }

        public void UpdateLineRenderer(Vector3 startPosition, Vector3 endPosition)
        {
            if (useLineRenderer)
            {
                thisLineRenderer.SetPosition(0, startPosition);
                thisLineRenderer.SetPosition(1, endPosition);
            }
        }

    }
}