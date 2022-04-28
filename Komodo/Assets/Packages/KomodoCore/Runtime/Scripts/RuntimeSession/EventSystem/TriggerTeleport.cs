using Komodo.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Komodo.Runtime
{
    /**
     * @brief This script raycast to interact with objects by sending a collider that triggers functions ontrigger.
     * This script is mainly attached to the TeleportLeft and TeleportRight gameobjects.
     * 
     * To find these gameobjects, go to
     * Heirarchy -> PlayerSet -> WebXRCameraSet -> PlayspaceAnchor -> Hands -> handL(or handR) -> Armature -> Palm -> 
     * F1cTeleportLeft(or F1cTeleportRight) -> TeleportLeft(or TeleportRight).
     */
    public class TriggerTeleport : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        public Color originalLineColor;
        public Color selectionLineColor;

        public Vector3? newPlayerPos;

        [Header("PARABOLIC LINE SETTINGS")]
        public bool isParabolic;

        /** 
         * @brief Horizontal distance of end point from controller. 
        */
        [Tooltip("Horizontal distance of end point from controller")]
        public float distance = 15.0f;

        /** 
         * @brief Vertical of end point from controller.
        */
        [Tooltip("Vertical of end point from controller")]
        public float dropHeight = 5.0f;

        /** 
         * @brief Height of bezier control (0 is at mid point).
        */
        [Tooltip("Height of bezier control (0 is at mid point)")]
        public float controlHeight = 5.0f;

        /** 
         * @brief How many segments to use for curve, must be at least 3. More segments = better quality.
        */
        [Tooltip("How many segments to use for curve, must be at least 3. More segments = better quality")]
        public int segments = 10;

        public bool MakingContact { get; protected set; }
        // If it did, what was the normal
        public Vector3 Normal { get; protected set; }
           
        /** 
         * @brief HitPoint returns a location of the ray when it hits something. 
        */
        public Vector3 HitPoint { get; protected set; }

        /** 
         * @brief How manu angles from world up the surface can point and still be valid. Avoids casting onto walls.
        */
        [Tooltip("How manu angles from world up the surface can point and still be valid. Avoids casting onto walls.")]
        public float surfaceAngle = 5;


        /** 
         * @brief The variable End returns stores the location of the ray's endpoint. 
        */
        public Vector3 End { get; protected set; }

        public Vector3 Control
        {
            get
            {
                Vector3 midPoint = transform.position + (End - transform.position) * 0.5f;
                return midPoint + Vector3.up * controlHeight;
            }
        }
        [Space]

        /** 
         * @brief transfportationIndicator is assigned to a gameobject named Floor Transport Indicator. Floor Transport Indicator 
         * can be found in F1cTeleportLeft -> Floor Transport Indicator. 
         * 
         * This gameobject shows the teleportation indicator (a purple cylinder) when a player is trying to teleport. 
        */
        public GameObject transportationIndicator;
        
        /** 
         * @brief layerMask is assigned with the Unity Layer: Walkable. And from this name you can easily tell it stores information about
         * which area in the scene can be teleported to and which cannot.
        */
        public LayerMask layerMask;

        Vector3 locationToKeep;

        [ShowOnly] public bool isOverObject;

        /** 
         * @brief As you may know, Awake() runs when this script's instance is called. There are a few things that will be set up 
         * when this script is running:
         * \n 1) initialize and assign lineRenderer with the component that the current gameobject holds.
         * \n 2) set transportationIndicator as not-active. This is because we want to prevent the transportationIndicator 
         *       from randomly being on when the player is in the scene and have no intention of teleporting. 
         * \n 3) set up parabola for the teleportation line. 
        */
        public void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            //    colliderInput = transform.GetComponentInChildren<Collider>(true);

            if (transportationIndicator)
                transportationIndicator.SetActive(false);

            // setup the curve for the teleportation line to make it a parabola. 
            if (isParabolic)
                lineRenderer.positionCount = segments;
        }


        [System.Serializable]
        public class Output_NewVector3_UnityEvent : UnityEvent<Vector3> { }
        //   public void OnEnable() => colliderInput.enabled = true;
        public Coord_UnityEvent onNewTransformUpdate;

        /** 
         * @brief OnDisable() is another Unity specific function. This function is called when the script becomes disable or
         * when the gameobject that this script is attached to is out of scope. 
        */
        public void OnDisable()
        {
            if (transportationIndicator)
                transportationIndicator.SetActive(false);

            if (newPlayerPos != null)
            // if (inputSelection.newPlayerPos != null)
            {
                onNewTransformUpdate.Invoke(new Position
                {
                    pos = (Vector3)newPlayerPos,

                });
            }
            // colliderInput.enabled = false;
        }

        public void Update()
        {
            //if (colliderInput.enabled == false)
            //    return;

            //This is setting up the distance for the line renderer.
            Vector3 pos = transform.position + (transform.forward * 100);
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, pos);

            //raycast are either meant to be for selection or teleportation which takes into account the differences in implementation.
            // For this if-part, it is implemented for selection.
            if (!isParabolic)
            {
                //PLACING THIS HERE AFFECTS WORLD COLLIDER INTERACTIONS WITH LINE RENDERER?
                if (Physics.Linecast(transform.position, pos, out RaycastHit hit, layerMask))
                {
                    if (!isOverObject)
                        lineRenderer.material.color = selectionLineColor;

                    pos = hit.point;
                    isOverObject = true;
                    //  colliderInput.transform.position = hit.point;

                }
                else
                {
                    if (isOverObject)
                        lineRenderer.material.color = originalLineColor;

                    isOverObject = false;
                }

                lineRenderer.SetPosition(1, pos);
            }
            else
            {
                MakingContact = false;
                //local
                End = HitPoint = transform.position + transform.forward * distance + (transform.up * -1.0f) * dropHeight;
                //world
                //End = HitPoint = transform.position + Vector3.forward * distance + (transform.TransformDirection(Vector3.up) * -1.0f) * dropHeight;

                RaycastHit hit;
                Vector3 last = transform.position;
                float recip = 1.0f / (float)(segments - 1);

                for (int i = 1; i < segments; ++i)
                {
                    float t = (float)i * recip;
                    Vector3 sample = SampleCurve(transform.position, End, Control, Mathf.Clamp01(t));
                    lineRenderer.SetPosition(i, sample);

                    if (Physics.Linecast(last, sample, out hit, layerMask))//~excludeLayers))
                    {
                        pos = hit.point;

                        if (!transportationIndicator.activeInHierarchy)
                        {
                            transportationIndicator.SetActive(true);
                            lineRenderer.material.color = selectionLineColor;
                        }
                        //rotate appropriate to the surface normal
                        transportationIndicator.transform.rotation = (Quaternion.FromToRotation(transportationIndicator.transform.up, hit.normal)) * transportationIndicator.transform.rotation;
                        transportationIndicator.transform.position = pos;

                        newPlayerPos = pos;

                        // colliderInput.transform.position = pos;

                        locationToKeep = pos;

                        lineRenderer.SetPosition(i, sample);

                        //setBaseLine by setting last points to be the same
                        for (int e = i; e < segments; ++e)
                            lineRenderer.SetPosition(e, hit.point);

                        break;

                    }
                    else
                    {

                        pos = transform.position + (transform.forward * 100);
                        // colliderInput.transform.position = pos;
                        newPlayerPos = null;
                        if (transportationIndicator.activeInHierarchy)
                        {
                            transportationIndicator.SetActive(false);
                            lineRenderer.material.color = originalLineColor;
                        }
                        else
                            lineRenderer.SetPosition(i, sample);

                    }
                    last = sample;
                }

            }
        }

        Vector3 SampleCurve(Vector3 start, Vector3 end, Vector3 control, float time)
        {
            return Vector3.Lerp(Vector3.Lerp(start, control, time), Vector3.Lerp(control, end, time), time);
        }
    }
}