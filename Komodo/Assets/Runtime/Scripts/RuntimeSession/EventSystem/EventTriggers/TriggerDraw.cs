using UnityEngine;
using Unity.Entities;
using static Komodo.Runtime.NetworkUpdateHandler;

namespace Komodo.Runtime
{

    [RequireComponent(typeof(LineRenderer))]
    public class TriggerDraw : MonoBehaviour
    {
        private Transform thisTransform;
        private LineRenderer lineRenderer;

        public float distanceThreshold = 0.5f;

        public float timeToCheckNewStrokeIndex;
        private float timePass;

        private int curLineIndex = 0;
        private int strokeID = 0;

        private int strokeIndex;

        [Header("IDENTIFY INTERACTION ID, PLACE A UNIQUE NUMBER EXCEPT 0")]
        public int handID;


        //to disable drawing during color picker selection through unity events; // color+picker.cs adjust this
        [HideInInspector] public bool isSelectingColorPicker;

        private EntityManager entityManager;

        //to disable drawing during erassing funcionality
        private bool isEraserOn = false;



        public void Set_DRAW_UPDATE(bool active)
        {
            isEraserOn = active;
        }



        public virtual void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            lineRenderer = GetComponent<LineRenderer>();

            //set initial stroke id
            strokeID = handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;

            thisTransform = transform;
        }


        public void Update()
        {
            if (lineRenderer == null || thisTransform == null || isEraserOn || isSelectingColorPicker)
                return;

            timePass += Time.deltaTime;

            if (timeToCheckNewStrokeIndex < timePass)
            {
                timePass = 0.0f;

                float curDistance = 0;


                if (lineRenderer.positionCount == 0)
                {
                    ++lineRenderer.positionCount;

                    lineRenderer.SetPosition(0, thisTransform.position);
                    curDistance = Vector3.Distance(thisTransform.position, lineRenderer.GetPosition(0));

                }
                else
                    curDistance = Vector3.Distance(thisTransform.position, lineRenderer.GetPosition(curLineIndex));


                if (curDistance > distanceThreshold)
                {
                    //update visuals per stroke 
                    ////offset: 5000 + clientid + child render count 
                    ///

                    DrawingInstanceManager.Instance.SendStrokeNetworkUpdate(strokeID, Entity_Type.Line, lineRenderer.widthMultiplier, lineRenderer.GetPosition(curLineIndex),
                              new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a));


                    ++lineRenderer.positionCount;
                    curLineIndex++;

                    lineRenderer.SetPosition(curLineIndex, thisTransform.position);
                }
            }
        }


        //THIS IS WHERE FUNCTIONS ARE INVOKED (ON RELEASE OF TRIGGER BUTTON WHICH DEACTIVATES PARENT OBJECT
        public virtual void OnDisable()
        {
            //get rid of uncompleted stroke saved up locations 
            if (lineRenderer.positionCount == 1)
                lineRenderer.positionCount = 0;

            if (lineRenderer == null || lineRenderer.positionCount <= 1)
                return;

            //make strokeID identical based on left or right hand add an offset *100 strokeID * 10000
            //ALL STROKE IDS HAVE TO BE UNIQUE TO REFERENCE THROGH THE NETWORK
            if (NetworkUpdateHandler.IsAlive)
                strokeID = handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;
            else
                return;

            //GameObject pivot = 
            DrawingInstanceManager.Instance.CreateUserStrokeInstance(strokeID, lineRenderer, true);

            curLineIndex = 0;

            //pivot.transform.SetParent(lineRendererSharedContainer);

            strokeIndex++;
            //updateID
            if (NetworkUpdateHandler.IsAlive)
                strokeID = handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;

            lineRenderer.positionCount = 0;

        }

    }
}