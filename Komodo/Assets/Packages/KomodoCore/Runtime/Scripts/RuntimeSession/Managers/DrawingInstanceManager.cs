using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;
using System;
using Komodo.Utilities;
//using static Komodo.Runtime.NetworkUpdateHandler;

namespace Komodo.Runtime
{
    //may need to rename this if we are including primitive shapes to our list
    public class DrawingInstanceManager : SingletonComponent<DrawingInstanceManager>
    {
        public static DrawingInstanceManager Instance
        {
            get { return ((DrawingInstanceManager)_Instance); }
            set { _Instance = value; }
        }

        public Transform lineRendererContainerPrefab;

        public EntityManager entityManager;

        [HideInInspector] public Transform userStrokeParent;
        [HideInInspector] public Transform externalStrokeParent;


        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;

            //TODO -- warn if we are not attached to a GameObject

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

            if (lineRendererContainerPrefab == null)
            {
                throw new System.Exception("Line Renderer Container Prefab is not assigned in DrawingInstanceManager");
            }

            GameObject lineRendCopy = Instantiate(lineRendererContainerPrefab).gameObject;
            lineRendCopy.name = "LineR:" + strokeID;

            //Create a reference to use in network
            var nAGO = ClientSpawnManager.Instance.CreateNetworkedGameObject(pivot, strokeID, strokeID);

            //tag it as a drawing for ECS
            pivot.tag = TagList.drawing;
            entityManager.AddComponentData(nAGO.Entity, new DrawingTag { });

            var bColl = pivot.GetComponent<BoxCollider>();
            LineRenderer copiedLR = lineRendCopy.GetComponent<LineRenderer>();

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
                SendStrokeNetworkUpdate(strokeID, Entity_Type.LineEnd, copiedLR.widthMultiplier, lineRenderer.GetPosition(lineRenderer.positionCount - 1), new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a));
            }

            pivot.transform.SetParent(userStrokeParent, true);

            if (UndoRedoManager.IsAlive)
                //save undoing process for ourselves and others
                UndoRedoManager.Instance.savedStrokeActions.Push(() =>
                {
                    pivot.SetActive(false);

                    //send network update call for everyone else
                    SendStrokeNetworkUpdate(strokeID, Entity_Type.LineNotRender);
                });

        }


        public void CreateExternalClientStrokeInstance(int strokeID, LineRenderer currentLineRenderer)
        {
            GameObject pivot = new GameObject("LineRender:" + strokeID, typeof(BoxCollider));

            NetworkedGameObject nAGO = ClientSpawnManager.Instance.CreateNetworkedGameObject(pivot, strokeID, strokeID, true);

            //overide interactable tag when creatingNetworkGameObject since we are not moving drawings only deleting them
            pivot.tag = TagList.drawing;
            //tag created drawing object will be useful in the future for having items with multiple tags
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


        public void SendStrokeNetworkUpdate(int sID, Entity_Type entityType, float lineWidth = 1, Vector3 curPos = default, Vector4 color = default)
        {
            var drawUpdate = new Draw((int)NetworkUpdateHandler.Instance.client_id, sID
               , (int)entityType, lineWidth, curPos,
               color);

            var drawSer = JsonUtility.ToJson(drawUpdate);

            NetworkUpdateHandler.KomodoMessage komodoMessage = new NetworkUpdateHandler.KomodoMessage("draw", drawSer);
            komodoMessage.Send();
        }

    }


}
