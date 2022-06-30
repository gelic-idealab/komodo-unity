using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;
using System;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    /// <summary>
    /// This manager processes user's drawings and contains methods that update the drawings to other users.
    /// </summary>
    public class DrawingInstanceManager : SingletonComponent<DrawingInstanceManager>
    {
        public static DrawingInstanceManager Instance
        {
            get { return (DrawingInstanceManager) _Instance; }

            set { _Instance = value; }
        }

        /// <summary>
        /// lineRenderer prefab; this is assigned through inspector. The prefab locates in Assets -> Packages -> KomodoCore -> Runtime -> Prefabs.
        /// </summary>
        public Transform lineRendererContainerPrefab;

        public EntityManager entityManager;

        /// <summary>
        /// The GameObject parent of user's drawings/strokes.
        /// </summary>
        [HideInInspector] public Transform userStrokeParent;

        /// <summary>
        /// The GameObject parent of other user's drawings/strokes.
        /// </summary>
        [HideInInspector] public Transform externalStrokeParent;

        /// <summary>
        /// A dictionary data structure that stores lineRenders with corresponding IDs.
        /// </summary>
        private Dictionary<int, LineRenderer> lineRendererFromId = new Dictionary<int, LineRenderer>();

        /// <summary>
        /// Create GameObjects: <c>userStrokeParent</c> and <c>externalStrokeParent</c> and initialize them as the parents.
        /// </summary>
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

            GlobalMessageManager.Instance.Subscribe("draw", (str) => ReceiveDrawUpdate(str));
        }

        public void InitializeFinishedLineFromOwnClient(int strokeID, LineRenderer lineRenderer, bool doSendNetworkUpdate)
        {
            //set correct pivot point when scaling object by grabbing
            GameObject pivot = new GameObject("LineRender:" + strokeID, typeof(BoxCollider));

            if (lineRendererContainerPrefab == null)
            {
                throw new System.Exception("Line Renderer Container Prefab is not assigned in DrawingInstanceManager");
            }

            GameObject lineRendCopy = Instantiate(lineRendererContainerPrefab).gameObject;

            lineRendCopy.name = "LineR:" + strokeID;

            //Create a reference to use in network
            NetworkedGameObject nAGO = NetworkedObjectsManager.Instance.CreateNetworkedGameObject(pivot, strokeID, strokeID);

            // Make own client's draw strokes grabbable
            pivot.tag = TagList.interactable;

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

                newBounds.Encapsulate(new Bounds(lineRenderer.GetPosition(i), Vector3.one * 0.01f));
            }

            pivot.transform.position = newBounds.center;

            bColl.center = lineRendCopy.transform.position;

            bColl.size = newBounds.size;

            lineRendCopy.transform.SetParent(pivot.transform, true);

            if (doSendNetworkUpdate)
            {
                SendDrawUpdate(
                    strokeID,
                    Entity_Type.LineEnd,
                    copiedLR.widthMultiplier,
                    lineRenderer.GetPosition(lineRenderer.positionCount - 1),
                    new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b,  lineRenderer.startColor.a)
                );
            }

            pivot.transform.SetParent(userStrokeParent, true);

            if (UndoRedoManager.IsAlive)
            {
                //save undoing process for ourselves and others
                UndoRedoManager.Instance.savedStrokeActions.Push(() =>
                {
                    pivot.SetActive(false);

                    SendDrawUpdate(strokeID, Entity_Type.LineNotRender);
                });
            }
        }

        public void InitializeFinishedLineFromOtherClient(int strokeID, LineRenderer renderer)
        {
            GameObject pivot = new GameObject("LineRender:" + strokeID, typeof(BoxCollider));

            NetworkedGameObject netObject = NetworkedObjectsManager.Instance.CreateNetworkedGameObject(pivot, strokeID, strokeID, true);

            // Make other clients' draw strokes grabbable
            pivot.tag = TagList.interactable;

            //tag created drawing object will be useful in the future for having items with multiple tags
            entityManager.AddComponentData(netObject.Entity, new DrawingTag { });

            var collider = pivot.GetComponent<BoxCollider>();

            Bounds newBounds = new Bounds(renderer.GetPosition(0), Vector3.one * 0.01f);

            for (int i = 0; i < renderer.positionCount; i++)
            {
                newBounds.Encapsulate(new Bounds(renderer.GetPosition(i), Vector3.one * 0.01f));
            }

            pivot.transform.position = newBounds.center;

            collider.center = renderer.transform.position;

            collider.size = newBounds.size;

            renderer.transform.SetParent(pivot.transform, true);

            pivot.transform.SetParent(externalStrokeParent, true);
        }

        public void SendDrawUpdate(int id, Entity_Type entityType, float lineWidth = 1, Vector3 curPos = default, Vector4 color = default)
        {
            var drawUpdate = new Draw
            (
                (int) NetworkUpdateHandler.Instance.client_id,
                id,
                (int) entityType,
                lineWidth,
                curPos,
                color
            );

            var serializedUpdate = JsonUtility.ToJson(drawUpdate);

            KomodoMessage komodoMessage = new KomodoMessage("draw", serializedUpdate);

            komodoMessage.Send();
        }

        protected bool IsLineRendererRegistered (int id)
        {
            return lineRendererFromId.ContainsKey(id);
        }

        protected void RegisterLineRenderer (int id, LineRenderer renderer)
        {
            lineRendererFromId.Add(id, renderer);
        }

        protected LineRenderer GetLineRenderer (int id)
        {
            return lineRendererFromId[id];
        }

        protected void UnregisterLineRenderer (int id)
        {
            lineRendererFromId.Remove(id);
        }

        protected LineRenderer CreateLineRendererContainer (Draw data)
        {
            GameObject lineRendCopy = Instantiate(lineRendererContainerPrefab).gameObject;

            lineRendCopy.name = "LineR:" + data.strokeId;

            lineRendCopy.transform.SetParent(externalStrokeParent, true);

            return lineRendCopy.GetComponent<LineRenderer>();
        }

        protected void ContinueLine (Draw data)
        {
            if (!IsLineRendererRegistered(data.strokeId))
            {
                Debug.LogWarning($"Line renderer {data.strokeId} will not be started or continued, because it was never registered.");

                return;
            }

            LineRenderer renderer = GetLineRenderer(data.strokeId);

            var brushColor = new Vector4(data.curColor.x, data.curColor.y, data.curColor.z, data.curColor.w);

            renderer.startColor = brushColor;

            renderer.endColor = brushColor;

            renderer.widthMultiplier = data.lineWidth;

            ++renderer.positionCount;

            renderer.SetPosition(renderer.positionCount - 1, data.curStrokePos);
        }

        protected void EndLine (Draw data)
        {
            if (!IsLineRendererRegistered(data.strokeId))
            {
                Debug.LogWarning($"Line renderer {data.strokeId} will not be ended, because it was never registered.");

                return;
            }

            LineRenderer renderer = GetLineRenderer(data.strokeId);

            renderer.positionCount += 1;

            renderer.SetPosition(renderer.positionCount - 1, data.curStrokePos);

            InitializeFinishedLineFromOtherClient(data.strokeId, renderer);
        }

        protected void DeleteLine (Draw data)
        {
            if (!IsLineRendererRegistered(data.strokeId))
            {
                Debug.LogWarning($"Line renderer {data.strokeId} will not be deleted, because it was never registered.");

                return;
            }

            bool success = NetworkedObjectsManager.Instance.DestroyAndUnregisterEntity(data.strokeId);

            if (!success)
            {
                Debug.LogWarning($"Could not delete line {data.strokeId}'s networked object.");

                return;
            }

            UnregisterLineRenderer(data.strokeId);
        }

        protected void ShowLine (Draw data)
        {
            bool success = NetworkedObjectsManager.Instance.ShowEntity(data.strokeId);

            if (!success)
            {
                Debug.LogWarning($"Could not show line {data.strokeId}.");
            }
        }

        protected void HideLine (Draw data)
        {
            bool success = NetworkedObjectsManager.Instance.HideEntity(data.strokeId);

            if (!success)
            {
                Debug.LogWarning($"Could not hide line {data.strokeId}.");
            }
        }

        protected void StartLineAndRegisterLineRenderer (Draw data)
        {
            LineRenderer currentLineRenderer = CreateLineRendererContainer(data);

            currentLineRenderer.positionCount = 0;

            RegisterLineRenderer(data.strokeId, currentLineRenderer);
        }

        public void ReceiveDrawUpdate (string stringData)
        {
            Draw data = JsonUtility.FromJson<Draw>(stringData);

            if (!IsLineRendererRegistered(data.strokeId))
            {
                StartLineAndRegisterLineRenderer(data);
            }

            switch (data.strokeType)
            {
                // Continues a Line
                case (int) Entity_Type.Line:
                {
                    ContinueLine(data);

                    break;
                }

                case (int) Entity_Type.LineEnd:
                {
                    EndLine(data);

                    break;
                }

                case (int) Entity_Type.LineDelete:
                {
                    DeleteLine(data);

                    break;
                }

                case (int) Entity_Type.LineRender:
                {
                    ShowLine(data);

                    break;
                }

                case (int) Entity_Type.LineNotRender:
                {
                    HideLine(data);

                    break;
                }
            }
        }
    }
}
