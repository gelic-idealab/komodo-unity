using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;

namespace Komodo.Runtime
{
    public class DrawingInstanceManager : SingletonComponent<DrawingInstanceManager>
    {
        public static DrawingInstanceManager Instance
        {
            get { return ((DrawingInstanceManager)_Instance); }
            set { _Instance = value; }
        }

        public Transform lineRendererContainerPrefab;

        public EntityManager entityManager;

        [HideInInspector]public Transform userStrokeParent;
        [HideInInspector]public Transform externalStrokeParent;

        //used for redo funcionality
        private List<Transform> savedStrokesList;

        public void Awake()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            //parent for our stored lines
            userStrokeParent = new GameObject("UserStrokeParent").transform;
            externalStrokeParent = new GameObject("ExternalClientStrokeParent").transform;


            userStrokeParent.SetParent(transform);
            externalStrokeParent.SetParent(transform);

        }

        public void CreateUserStrokeInstance(int strokeID, LineRenderer lineRenderer, bool sendNetworkCall)
        {
            //used to set correct pivot point when scalling object by grabbing
            GameObject pivot = new GameObject("LineRender:" + strokeID, typeof(BoxCollider));
            GameObject lineRendCopy = Instantiate(lineRendererContainerPrefab).gameObject;//new GameObject("LineR:" + strokeID);
            lineRendCopy.name = "LineR:" + strokeID;

            //Create a reference to use in network
            var nAGO = ClientSpawnManager.Instance.CreateNetworkAssociatedGameObject(pivot, strokeID, strokeID);

            //tag it as a drawing for ECS
            pivot.tag = "Drawing";
            entityManager.AddComponentData(nAGO.Entity, new DrawingTag { });

            var bColl = pivot.GetComponent<BoxCollider>();
            LineRenderer copiedLR = lineRendCopy.GetComponent<LineRenderer>();// lineRendGO.AddComponent<LineRenderer>();

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

            if (sendNetworkCall)
            {
                //send signal to close off current linerender object
                NetworkUpdateHandler.Instance.DrawUpdate(
                    new Draw(NetworkUpdateHandler.Instance.client_id, strokeID,
                    (int)Entity_Type.LineEnd, copiedLR.widthMultiplier, lineRenderer.GetPosition(lineRenderer.positionCount - 1),
                    new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a)
                    ));
            }

            pivot.transform.SetParent(userStrokeParent, true);
        }


        public void CreateExternalClientStrokeInstance(int strokeID, LineRenderer currentLineRenderer)
        {
            GameObject pivot = new GameObject("LineRender:" + strokeID, typeof(BoxCollider));
            pivot.tag = "Drawing";

            NetworkAssociatedGameObject nAGO = ClientSpawnManager.Instance.CreateNetworkAssociatedGameObject(pivot, strokeID, strokeID, true);

           

            //tag created drawing object
            entityManager.AddComponentData(nAGO.Entity, new DrawingTag { });

            var bColl = pivot.GetComponent<BoxCollider>();

            Bounds newBounds = new Bounds(currentLineRenderer.GetPosition(0), Vector3.one * 0.01f);

            for (int i = 0; i < currentLineRenderer.positionCount; i++)
                newBounds.Encapsulate(new Bounds(currentLineRenderer.GetPosition(i), Vector3.one * 0.01f));

            pivot.transform.position = newBounds.center;
            bColl.center = currentLineRenderer.transform.position;
            bColl.size = newBounds.size;

            currentLineRenderer.transform.SetParent(pivot.transform, true);

            pivot.transform.SetParent(externalStrokeParent, true);
        }


      
    }
}
