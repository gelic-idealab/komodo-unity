using UnityEngine;
//using Unity.Entities;

namespace Komodo.Runtime
{
    /** 
     * @brief This is the script that controls the drawing fucntionality. Also, this script contains functions that capture
     * stroke IDs and data from users. 
    */

    [RequireComponent(typeof(LineRenderer))]
    public class TriggerDraw : MonoBehaviour
    {   
        /** 
         * @brief the transform of the object that this script is attached to. In this case,it will be F1cSelectLeft (or F1cSelectRight) ->
         * DrawLeft (or DrawRight).
        */
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

        //to disable drawing during color picker selection through unity events;
        [HideInInspector] public bool isSelectingColorPicker;

        //private EntityManager entityManager;

        //to disable drawing during erasing funcionality
        private bool isEraserOn = false;

        public void Set_DRAW_UPDATE(bool active)
        {
            isEraserOn = active;
        }

        public virtual void Start()
        {
            //entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            lineRenderer = GetComponent<LineRenderer>();

            //set initial stroke id
            strokeID = handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;

            thisTransform = transform;
        }


        /** 
         * @brief Update() as you may know is a Unity function. However, this Update() contains most of the code that serves to the 
         * drawing functionality. All the drawing strokes will be stored in the DrawingInstanceManager.cs
         * 
         * Here are a few things that happen in the Update():
         * 
         * 1) Before anything happens, we check to see if lineRenderer or thisTransform are null. At the same time, we check if user is 
         * using eraser or selecting color. If one of these happens, we return the Update(); in other words we do nothing. 
         * 
         * 2) The actual drawing process involve two variables: ```timeToCheckNewStrokeIndex``` and ```timePass```.       
         * ```timeToCheckNewStrokeIndex``` is always 0, and timePass will always bigger than timeToCheckNewStrokeIndex. In this sense, this script will always check if there is 
         * a new stroke index being created.
         * 
         * 3) The line ```if (lineRenderer.positionCount == 0)``` means a new line is being created. If this is the case, it increments
         * ```lineRenderer.positionCount```, sets a point to lineRenderer with user's hand's position. Then, it stores a distance between
         * the position of the hand and the first index of the line.
         * 
         * 4) If ```lineRenderer.positionCount``` is not 0, then it will get a distance bwetween position of user's hand and the position
         * of current line index. 
         * 
         * 5) if the ```curDistance``` is greater than ```distanceThreshold``` (which is 0.5f), then it will draw the line.
         * 
         * 
        */
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
                {
                    curDistance = Vector3.Distance(thisTransform.position, lineRenderer.GetPosition(curLineIndex));
                }

                if (curDistance > distanceThreshold)
                {
                    //update visuals per stroke 
                    //offset: 5000 + clientid + child render count 

                    DrawingInstanceManager.Instance.SendDrawUpdate(
                        strokeID,
                        Entity_Type.Line,
                        lineRenderer.widthMultiplier,
                        lineRenderer.GetPosition(curLineIndex),
                        new Vector4
                        (
                            lineRenderer.startColor.r,
                            lineRenderer.startColor.g,
                            lineRenderer.startColor.b,
                            lineRenderer.startColor.a
                        )
                    );

                    ++lineRenderer.positionCount;

                    curLineIndex++;

                    lineRenderer.SetPosition(curLineIndex, thisTransform.position);
                }
            }
        }

        /** 
         * @brief This is where functions are invoked on release of trigger button which deactivates parent object. In other words,
         * this happens when users are not in drawing mode. However, this function will get rid of uncompleted stroke and save up 
         * locations.
        */
        public virtual void OnDisable()
        {
            //get rid of uncompleted stroke saved up locations 
            if (lineRenderer.positionCount == 1)
            {
                lineRenderer.positionCount = 0;
            }

            if (lineRenderer == null || lineRenderer.positionCount <= 1)
            {
                return;
            }

            //make strokeID identical based on left or right hand add an offset *100 strokeID * 10000
            //ALL STROKE IDS HAVE TO BE UNIQUE TO REFERENCE THROGH THE NETWORK
            if (NetworkUpdateHandler.IsAlive)
            {
                strokeID = handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;

                // TODO: refactor the multiple instances of this 
                // calculation into one method.

                // TODO: evaluate reduced human readability if there
                // are 100 or more registered clients.
                // Consider client 111. Then the equation would be 
                // 1000000 + 100000 + 1110000 + 1234
                // = 2210000 + 891234
                // = 3101234
                // The apparent hand is 3 and apparent client ID is 10, 
                // but the actual hand is 1 and the actual client ID is 
                // 111.
            }
            else
            {
                return;
            }

            DrawingInstanceManager.Instance.InitializeFinishedLineFromOwnClient(strokeID, lineRenderer, true);

            curLineIndex = 0;

            strokeIndex++;

            if (NetworkUpdateHandler.IsAlive)
            {
                strokeID = handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;
            }

            lineRenderer.positionCount = 0;
        }
    }
}