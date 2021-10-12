using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    public class NetworkedObjectsManager : SingletonComponent<NetworkedObjectsManager>
    {
        public static NetworkedObjectsManager Instance
        {
            get { return (NetworkedObjectsManager) _Instance; }

            set { _Instance = value; }
        }

        private EntityManager entityManager;

        public Dictionary<int, NetworkedGameObject> networkedObjectFromEntityId = new Dictionary<int, NetworkedGameObject>();

        public void Awake ()
        {
            var forceAlive = Instance;

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        public void Register (int entityID, NetworkedGameObject netObject)
        {
            networkedObjectFromEntityId.Add(entityID, netObject);
        }

        public void ApplyPosition (Position positionData)
        {
            if (networkedObjectFromEntityId.ContainsKey(positionData.entityId))
            {
                networkedObjectFromEntityId[positionData.entityId].transform.position = positionData.pos;

                networkedObjectFromEntityId[positionData.entityId].transform.rotation = positionData.rot;

                UnityExtensionMethods.SetGlobalScale(networkedObjectFromEntityId[positionData.entityId].transform, Vector3.one * positionData.scaleFactor);
            }
            else
            {
                Debug.LogWarning("Entity ID : " + positionData.entityId + "not found in Dictionary dropping object movement packet");
            }
        }

        //TODO(Brandon): is this even used anymore?
        // public void DeleteAndUnregisterNetworkedGameObject(int entityID)
        // {
        //     if (NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(entityID))
        //     {
        //         entityManager.DestroyEntity(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[entityID].Entity);
        //         Destroy(Instance.networkedObjectFromEntityId[entityID].gameObject);
        //         Instance.networkedObjectFromEntityId.Remove(entityID);
        //     }
        // }

        public void ApplyInteraction (Interaction interactionData)
        {
            if (GameStateManager.IsAlive && UIManager.IsAlive && !UIManager.Instance.IsReady())
            {
                return;
            }

            switch (interactionData.interactionType)
            {
                case (int)INTERACTIONS.RENDERING:

                    if (UIManager.IsAlive)
                    {
                        UIManager.Instance.ProcessNetworkToggleVisibility(interactionData.targetEntity_id, true);
                    }

                    break;

                case (int)INTERACTIONS.NOT_RENDERING:

                    if (UIManager.IsAlive)
                    {
                        UIManager.Instance.ProcessNetworkToggleVisibility(interactionData.targetEntity_id, false);
                    }

                    break;

                case (int)INTERACTIONS.GRAB:

                    Instance.ApplyGrabInteraction(interactionData);

                    break;

                case (int)INTERACTIONS.DROP:

                    Instance.ApplyDropInteraction(interactionData);

                    break;

                case (int)INTERACTIONS.CHANGE_SCENE:

                    if (SceneManagerExtensions.IsAlive)
                    {
                        //check the loading wait for changing into a new scene - to avoid loading multiple scenes
                        SceneManagerExtensions.Instance.SelectScene(interactionData.targetEntity_id);
                    }

                    break;

                case (int)INTERACTIONS.LOCK:

                    Instance.ApplyLockInteraction(interactionData);

                    break;

                case (int)INTERACTIONS.UNLOCK:

                    Instance.ApplyUnlockInteraction(interactionData);

                    break;
            }
        }

        public void ApplyGrabInteraction (Interaction interactionData)
        {
            entityManager.AddComponentData(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity, new TransformLockTag());
        }

        public void ApplyDropInteraction (Interaction interactionData)
        {
            if (entityManager.HasComponent<TransformLockTag>(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity))
            {
                entityManager.RemoveComponent<TransformLockTag>(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity);
            }
            else
            {
                Debug.LogWarning("Client Entity does not exist for Drop interaction--- EntityID" + interactionData.targetEntity_id);
            }
        }

        public void ApplyLockInteraction (Interaction interactionData)
        {
            if (!Instance.networkedObjectFromEntityId.ContainsKey(interactionData.targetEntity_id))
            {
                return;
            }

            var buttID = -1;

            if (entityManager.HasComponent<ButtonIDSharedComponentData>(Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity))
            {
                buttID = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity).buttonID;//entityID_To_NetObject_Dict[newData.targetEntity_id].buttonID;
            }

            if (buttID == -1)
            {
                return;
            }

            //disable button interaction for others
            if (UIManager.IsAlive)
            {
                UIManager.Instance.ProcessNetworkToggleLock(buttID, true);
            }
        }

        public void ApplyUnlockInteraction (Interaction interactionData)
        {
            if (!NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(interactionData.targetEntity_id))
            {
                return;
            }

            var buttID = -1;

            if (entityManager.HasComponent<ButtonIDSharedComponentData>(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity))
            {
                buttID = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity).buttonID;
            }

            if (buttID == -1)
            {
                return;
            }

            if (UIManager.IsAlive)
            {
                UIManager.Instance.ProcessNetworkToggleLock(buttID, false);
            }
        }
    }
}
