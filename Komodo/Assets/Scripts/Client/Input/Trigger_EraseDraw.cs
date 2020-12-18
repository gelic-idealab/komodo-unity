using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Trigger_EraseDraw : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        //if line? tag, line rend? 
        if (!other.CompareTag("Drawing"))
            return;

        var netReg = other.GetComponent<Net_Register_GameObject>();

        if(netReg)
        {
            
            if (ClientSpawnManager.Instance.entityID_To_NetObject_Dict.ContainsKey(netReg.entity_data.entityID))
            {
                Destroy(ClientSpawnManager.Instance.entityID_To_NetObject_Dict[netReg.entity_data.entityID].gameObject);
                ClientSpawnManager.Instance.entityID_To_NetObject_Dict.Remove(netReg.entity_data.entityID);
            }

            NetworkUpdateHandler.Instance.DrawUpdate(
                   //    ClientSpawnManager.Instance.Draw_Refresh(
                   new Draw((int)ClientSpawnManager.Instance.mainPlayer_RootTransformData.clientID, netReg.entity_data.entityID 
                   , (int)Entity_Type.LineDelete, 1, Vector3.zero,
                       Vector4.zero));

        }
    }
    // private LineRenderer lineRenderer;
    // private int curLineIndex = 0;

    // public Transform lineRendererContainer;
    // public float distanceThreshold = 0.5f;
    // private Material materialToChangeLRTo;

    // private Transform thisTransform;

    // private Vector3 originalRotationForAABB;
    // public float timeToCheckNewStrokeIndex;
    // private float timePass;


    // private int strokeID = 0;

    // private int strokeIndex;

    //     [Header("IDENTIFY INTERACTION ID, PLACE A UNIQUE NUMBER EXCEPT 0")]
    // public int handID;

    // public virtual void Start()
    // {
    //     lineRenderer = GetComponent<LineRenderer>();

    //     //set initial stroke id
    //     strokeID = handID * 1000000 + 100000 + (int)ClientSpawnManager.Instance._mainClient_entityData.clientID * 10000 + strokeIndex;

    //     thisTransform = transform;
    //     materialToChangeLRTo = lineRendererContainer.GetComponent<LineRenderer>().sharedMaterial;

    // }


    // public void Update()
    // {
    //     if (lineRenderer == null || thisTransform == null)
    //         return;

    //     timePass += Time.deltaTime;

    //     if (timeToCheckNewStrokeIndex < timePass)
    //     {
    //         timePass = 0.0f;

    //         float curDistance = 0;

    //         //if (false)
    //          if (lineRenderer.positionCount == 0)
    //         {
    //             ++lineRenderer.positionCount;

    //            lineRenderer.SetPosition(0, thisTransform.position);
    //            curDistance = Vector3.Distance(thisTransform.position, lineRenderer.GetPosition(0));

    //             originalRotationForAABB = thisTransform.TransformDirection(Vector3.forward);
    //         }
    //         else
    //            curDistance = Vector3.Distance(thisTransform.position, lineRenderer.GetPosition(curLineIndex));



    //         if (curDistance > distanceThreshold)
    //         {
    //             //update visuals per stroke 
    //             ////offset: 5000 + clientid + child render count 
    //           NetworkUpdateHandler.Instance.DrawUpdate(
    //  //    ClientSpawnManager.Instance.Draw_Refresh(
    //                 new Draw((int)ClientSpawnManager.Instance._mainClient_entityData.clientID, strokeID, (int)Entity_Type.Line, lineRenderer.startWidth, lineRenderer.GetPosition(curLineIndex), 
    //                     new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a)));

    //             ++lineRenderer.positionCount;
    //             curLineIndex++;

    //             lineRenderer.SetPosition(curLineIndex, thisTransform.position);

    //         }
    //     }
    // }


    // //THIS IS WHERE FUNCTIONS ARE INVOKED (ON RELEASE OF TRIGGER BUTTON WHICH DEACTIVATES PARENT OBJECT
    // public virtual void OnDisable()
    // {

    //     if (lineRenderer == null || lineRenderer.positionCount <= 1)
    //         return;

    //     strokeIndex++;
    //     //used to set correct pivot point when scalling object by grabbing

    //     GameObject pivot = new GameObject("LineRender:" + strokeID, typeof(BoxCollider));
    //     GameObject lineRendGO = new GameObject("LineR:" + strokeID);

    //     //pivot.tag = "Interactable";
    //     ////offset
    //     ///     //make strokeID identical based on left or right hand add an offset *100 strokeID * 10000
    //     //ALL STROKE IDS HAVE TO BE CONSISTENT
    //     if (ClientSpawnManager.IsAlive)
    //     {
    //         strokeID = handID * 1000000 + 100000 + (int)ClientSpawnManager.Instance._mainClient_entityData.clientID * 10000 + strokeIndex;
    //     }
    //     else
    //         return;
    //  ////offset: 5000 + clientid + child render count 
    ////     ClientSpawnManager.Instance.LinkNewNetworkObject(pivot, strokeID, strokeID);

    //     //var rb = pivot.GetComponent<Rigidbody>();
    //     //rb.isKinematic = true;
    //     //rb.useGravity = false;

    //     var bColl = pivot.GetComponent<BoxCollider>();
    //     LineRenderer copiedLR = lineRendGO.AddComponent<LineRenderer>();

    //     //dont instantiate materials instead just add color to line renderers
    //     //if(materialToChangeLRTo == null)
    //     //    materialToChangeLRTo = lineRendererContainer.GetComponent<LineRenderer>().lineRenderer.startColor;

    //     copiedLR.sharedMaterial = materialToChangeLRTo;
    //     var color = lineRenderer.startColor;
    //     copiedLR.startColor = color;
    //     copiedLR.endColor = color;

    //     copiedLR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    //     copiedLR.allowOcclusionWhenDynamic = false;
    //     copiedLR.useWorldSpace = false;
    //     copiedLR.startWidth = lineRenderer.startWidth;
    //     copiedLR.receiveShadows = false;

    //     Bounds newBounds = new Bounds(lineRenderer.GetPosition(0), Vector3.one * 0.01f);
    //     copiedLR.positionCount = 0;

    //     for (int i = 0; i < lineRenderer.positionCount; i++)
    //     {
    //         copiedLR.positionCount++;
    //         copiedLR.SetPosition(i, lineRenderer.GetPosition(i));

    //         newBounds.Encapsulate(new Bounds(lineRenderer.GetPosition(i), Vector3.one * 0.01f));//lineRenderer.GetPosition(i));
    //     }

    //     pivot.transform.position = newBounds.center;
    //     bColl.center = lineRendGO.transform.position;  //newBounds.center;//averageLoc / lr.positionCount;//lr.GetPosition(0)/2;
    //     bColl.size = newBounds.size;

    //     lineRendGO.transform.SetParent(pivot.transform, true);

    //     curLineIndex = 0;

    //     pivot.transform.SetParent(lineRendererContainer);



    //     //send signal to close off current linerender object
    //     NetworkUpdateHandler.Instance.DrawUpdate(

    //  //   ClientSpawnManager.Instance.Draw_Refresh(

    //         new Draw((int)ClientSpawnManager.Instance._mainClient_entityData.clientID, strokeID, 
    //         (int)Entity_Type.LineEnd, copiedLR.startWidth, lineRenderer.GetPosition(lineRenderer.positionCount-1), 
    //         new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a)
    //         ));

    //     lineRenderer.positionCount = 0;

    // }

}
