using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

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

    //to disable drawing during erassing funcionality
    private bool isEraserOn = false;



    public void Set_DRAW_UPDATE(bool active)
    {
        isEraserOn = active;
    }

  

    public virtual void Start()
    {
        //first one on adds instantiates our container and provides reference to our sharedcontainer
        if (!lineRendererSharedContainer)
        {
            lineRendererSharedContainer = Instantiate(lineRendererContainerPrefab);
            lineRendererSharedContainer.name = "Main Client Drawing Container";
        }



        lineRenderer = GetComponent<LineRenderer>();

        //set initial stroke id
        strokeID = handID * 1000000 + 100000 + (int)ClientSpawnManager.Instance.mainPlayer_RootTransformData.clientID * 10000 + strokeIndex;

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
                    new Draw((int)ClientSpawnManager.Instance.mainPlayer_RootTransformData.clientID, strokeID, (int)Entity_Type.Line, lineRenderer.startWidth, lineRenderer.GetPosition(curLineIndex), 
                        new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a)));

                //ClientSpawnManager.Instance.Draw_Refresh(new Draw
                //{
                //    clientId = (int)ClientSpawnManager.Instance._mainClient_entityData.clientID,
                //    strokeId = strokeID + 100000000,
                //    curStrokePos = lineRenderer.GetPosition(curLineIndex),
                //    strokeType = (int)Entity_Type.Line,
                //    curColor = new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a)
                //});





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
        {
            lineRenderer.positionCount = 0;
        }

        if (lineRenderer == null || lineRenderer.positionCount <= 1)
            return;

        
        //used to set correct pivot point when scalling object by grabbing

       
        //pivot.tag = "Interactable";
        ////offset
        ///     //make strokeID identical based on left or right hand add an offset *100 strokeID * 10000
        //ALL STROKE IDS HAVE TO BE CONSISTENT
        if (ClientSpawnManager.IsAlive)
        {
            strokeID = handID * 1000000 + 100000 + (int)ClientSpawnManager.Instance.mainPlayer_RootTransformData.clientID * 10000 + strokeIndex;
        }
        else
            return;



        GameObject pivot = new GameObject("LineRender:" + strokeID, typeof(BoxCollider));
        GameObject lineRendCopy = Instantiate(lineRendererContainerPrefab).gameObject;//new GameObject("LineR:" + strokeID);
        lineRendCopy.name =  "LineR:" + strokeID;

        ClientSpawnManager.Instance.LinkNewNetworkObject(pivot, strokeID, strokeID);
        pivot.tag = "Drawing";

        ////offset: 5000 + clientid + child render count 

   
        var bColl = pivot.GetComponent<BoxCollider>();
        LineRenderer copiedLR = lineRendCopy.GetComponent<LineRenderer>();// lineRendGO.AddComponent<LineRenderer>();
     



        copiedLR.sharedMaterial = materialToChangeLRTo;
        var color = lineRenderer.startColor;
        copiedLR.startColor = color;
        copiedLR.endColor = color;

        //copiedLR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //copiedLR.allowOcclusionWhenDynamic = false;
        //copiedLR.useWorldSpace = false;
        //copiedLR.startWidth = lineRenderer.startWidth;
        //copiedLR.receiveShadows = false;
        //copiedLR.numCapVertices = 3;

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


            new Draw((int)ClientSpawnManager.Instance.mainPlayer_RootTransformData.clientID, strokeID,
            (int)Entity_Type.LineEnd, copiedLR.startWidth, lineRenderer.GetPosition(lineRenderer.positionCount - 1),
            new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a)
            ));



        strokeIndex++;
        if (ClientSpawnManager.IsAlive)
        {
            strokeID = handID * 1000000 + 100000 + (int)ClientSpawnManager.Instance.mainPlayer_RootTransformData.clientID * 10000 + strokeIndex;
        }

        lineRenderer.positionCount = 0;
    
    }

}
