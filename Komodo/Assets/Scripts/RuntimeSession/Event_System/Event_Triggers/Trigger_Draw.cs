using UnityEngine;
using Unity.Entities;

[RequireComponent(typeof(LineRenderer))]
public class Trigger_Draw : MonoBehaviour
{

    private LineRenderer lineRenderer;

    public Transform lineRendererContainerPrefab;
    public static Transform lineRendererSharedContainer;


    public float distanceThreshold = 0.5f;
    private Material materialToChangeLRTo;

    private Transform thisTransform;

    private Vector3 originalRotationForAABB;
    public float timeToCheckNewStrokeIndex;
    private float timePass;

    private int curLineIndex = 0;
    private int strokeID = 0;

    private int strokeIndex;

        [Header("IDENTIFY INTERACTION ID, PLACE A UNIQUE NUMBER EXCEPT 0")]
    public int handID;


    //to disable drawing during color picker selection through unity events; // color+picker.cs adjust this
    [HideInInspector]public bool isSelectingColorPicker;

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

        //first one on adds instantiates our container and provides reference to our sharedcontainer
        if (!lineRendererSharedContainer)
        {
            lineRendererSharedContainer = Instantiate(lineRendererContainerPrefab);
            lineRendererSharedContainer.name = "Main Client Drawing Container";
            lineRendererSharedContainer.transform.SetParent(ClientSpawnManager.Instance.transform); 
        }

        lineRenderer = GetComponent<LineRenderer>();

        //set initial stroke id
        strokeID = handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;

        thisTransform = transform;
        materialToChangeLRTo = lineRendererSharedContainer.GetComponent<LineRenderer>().sharedMaterial;
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

            //if (false)
             if (lineRenderer.positionCount == 0)
            {
                ++lineRenderer.positionCount;

               lineRenderer.SetPosition(0, thisTransform.position);
               curDistance = Vector3.Distance(thisTransform.position, lineRenderer.GetPosition(0));

                originalRotationForAABB = thisTransform.TransformDirection(Vector3.forward);
            }
            else
               curDistance = Vector3.Distance(thisTransform.position, lineRenderer.GetPosition(curLineIndex));


            if (curDistance > distanceThreshold)
            {
                //update visuals per stroke 
                ////offset: 5000 + clientid + child render count 
              NetworkUpdateHandler.Instance.DrawUpdate(
     //    ClientSpawnManager.Instance.Draw_Refresh(
                    new Draw(NetworkUpdateHandler.Instance.client_id, strokeID, (int)Entity_Type.Line, lineRenderer.widthMultiplier, lineRenderer.GetPosition(curLineIndex), 
                        new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a)));


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
        if(lineRenderer.positionCount == 1)
            lineRenderer.positionCount = 0;

        if (lineRenderer == null || lineRenderer.positionCount <= 1)
            return;
       
          //make strokeID identical based on left or right hand add an offset *100 strokeID * 10000
        //ALL STROKE IDS HAVE TO BE UNIQUE TO REFERENCE THROGH THE NETWORK
        if (ClientSpawnManager.IsAlive)
            strokeID = handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;
        else
            return;

        //used to set correct pivot point when scalling object by grabbing
        GameObject pivot = new GameObject("LineRender:" + strokeID, typeof(BoxCollider));
        GameObject lineRendCopy = Instantiate(lineRendererContainerPrefab).gameObject;//new GameObject("LineR:" + strokeID);
        lineRendCopy.name =  "LineR:" + strokeID;

        //Create a reference to use in network
        var nAGO = ClientSpawnManager.Instance.CreateNetworkAssociatedGameObject(pivot, strokeID, strokeID);
       
        //tag it as a drawing for ECS
        pivot.tag = "Drawing";
        entityManager.AddComponentData(nAGO.Entity, new DrawingTag { });
        //entityManager.AddComponentData(nAGO.Entity, new NetworkEntityIdentificationComponentData { clientID = NetworkUpdateHandler.Instance.client_id, entityID = strokeID, sessionID = NetworkUpdateHandler.Instance.session_id, current_Entity_Type = Entity_Type.none });

        var bColl = pivot.GetComponent<BoxCollider>();
        LineRenderer copiedLR = lineRendCopy.GetComponent<LineRenderer>();// lineRendGO.AddComponent<LineRenderer>();
     
        copiedLR.sharedMaterial = materialToChangeLRTo;
        var color = lineRenderer.startColor;
        copiedLR.startColor = color;
        copiedLR.endColor = color;

        copiedLR.widthMultiplier = lineRenderer.widthMultiplier;

        Bounds newBounds = new Bounds(lineRenderer.GetPosition(0), Vector3.one * 0.01f);
        copiedLR.positionCount = 0;

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            copiedLR.positionCount++;
            copiedLR.SetPosition(i, lineRenderer.GetPosition(i));

            newBounds.Encapsulate(new Bounds(lineRenderer.GetPosition(i), Vector3.one * 0.01f));//lineRenderer.GetPosition(i));
        }

        pivot.transform.position = newBounds.center;
        bColl.center = lineRendCopy.transform.position;  //newBounds.center;//averageLoc / lr.positionCount;//lr.GetPosition(0)/2;
        bColl.size = newBounds.size;

        lineRendCopy.transform.SetParent(pivot.transform, true);

        curLineIndex = 0;

        pivot.transform.SetParent(lineRendererSharedContainer);


        //send signal to close off current linerender object
        NetworkUpdateHandler.Instance.DrawUpdate(
            new Draw(NetworkUpdateHandler.Instance.client_id, strokeID,
            (int)Entity_Type.LineEnd, copiedLR.widthMultiplier, lineRenderer.GetPosition(lineRenderer.positionCount - 1),
            new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a)
            ));

        strokeIndex++;
        //updateID
        if (ClientSpawnManager.IsAlive)
            strokeID = handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;

        lineRenderer.positionCount = 0;
    
    }

}
