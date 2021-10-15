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

        //list of decomposed for entire set locking
        public Dictionary<int, List<NetworkedGameObject>> networkedSubObjectListFromIndex = new Dictionary<int, List<NetworkedGameObject>>();

        public List<Entity> topLevelEntityList = new List<Entity>();

        //this is used to keep tabs on a unique identifier for our decomposed objeccts that are instantiated
        private static int uniqueDefaultID;

        public void Awake ()
        {
            var forceAlive = Instance;

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        public void Register (int entityID, NetworkedGameObject netObject)
        {
            networkedObjectFromEntityId.Add(entityID, netObject);
        }

        public Entity GetEntity(int index)
        {
            if (index < 0 || index >= topLevelEntityList.Count)
            {
                throw new System.Exception("Entity index is out-of-bounds for the client's entity list.");
            }

            return topLevelEntityList[index];
        }

        public NetworkedGameObject GetNetworkedGameObject(int buttonId)
        {
            if (buttonId < 0 || buttonId >= ModelImportInitializer.Instance.networkedGameObjects.Count)
            {
                throw new System.Exception("Index is out-of-bounds for the client's networked game objects list.");
            }

            return ModelImportInitializer.Instance.networkedGameObjects[buttonId];
        }

        public List<NetworkedGameObject> GetNetworkedSubObjectList(int index)
        {
            List<NetworkedGameObject> result;

            bool success = networkedSubObjectListFromIndex.TryGetValue(index, out result);

            if (!success)
            {
                throw new System.Exception($"Value was not found in client's networked game objects dictionary for key {index}.");
            }

            return result;
        }


        public int GenerateEntityIDBase ()
        {
            return (999 * 1000) + ((int) Entity_Type.objects * 100);
        }

        public int GenerateUniqueEntityID ()
        {
            int id = GenerateEntityIDBase() + uniqueDefaultID;

            uniqueDefaultID += 1;

            return id;
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
        //     if (Instance.networkedObjectFromEntityId.ContainsKey(entityID))
        //     {
        //         entityManager.DestroyEntity(Instance.networkedObjectFromEntityId[entityID].Entity);
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
                case (int)INTERACTIONS.SHOW:

                    if (UIManager.IsAlive)
                    {
                        UIManager.Instance.ProcessNetworkToggleVisibility(interactionData.targetEntity_id, true);
                    }

                    break;

                case (int)INTERACTIONS.HIDE:

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
            entityManager.AddComponentData(Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity, new TransformLockTag());
        }

        public void ApplyDropInteraction (Interaction interactionData)
        {
            if (entityManager.HasComponent<TransformLockTag>(Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity))
            {
                entityManager.RemoveComponent<TransformLockTag>(Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity);
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
                Debug.LogError($"ApplyLockInteraction: couldn't find netObject for targetEntityID {interactionData.targetEntity_id}");

                return;
            }

            var targetEntity = Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity;

            if (!entityManager.HasComponent<ButtonIDSharedComponentData>(targetEntity))
            {
                Debug.LogError($"ApplyLockInteraction: couldn't find button ID component for entity with targetEntityID {interactionData.targetEntity_id}");

                return;
            }

            var buttonIndex = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(targetEntity).buttonID;

            //disable button interaction for others
            if (!UIManager.IsAlive)
            {
                Debug.LogError($"ApplyLockInteraction: entity with targetEntityID {interactionData.targetEntity_id}: UIManager.IsAlive was false");

                return;
            }

            UIManager.Instance.ProcessNetworkToggleLock(buttonIndex, true);
        }

        public void ApplyUnlockInteraction (Interaction interactionData)
        {
            if (!Instance.networkedObjectFromEntityId.ContainsKey(interactionData.targetEntity_id))
            {
                Debug.LogError($"ApplyLockInteraction: couldn't find netObject for targetEntityID {interactionData.targetEntity_id}");

                return;
            }

            var targetEntity = Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity;

            if (!entityManager.HasComponent<ButtonIDSharedComponentData>(targetEntity))
            {
                Debug.LogError($"ApplyLockInteraction: couldn't find button ID component for entity with targetEntityID {interactionData.targetEntity_id}");

                return;
            }

            var buttonIndex = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(targetEntity).buttonID;

            //disable button interaction for others
            if (!UIManager.IsAlive)
            {
                Debug.LogError($"ApplyLockInteraction: entity with targetEntityID {interactionData.targetEntity_id}: UIManager.IsAlive was false");

                return;
            }

            UIManager.Instance.ProcessNetworkToggleLock(buttonIndex, false);
        }
                /// <summary>
        /// Allows ClientSpawnManager have reference to the network reference gameobject to update with calls
        /// </summary>
        /// <param name="gObject"></param>
        /// <param name="modelListIndex"> This is the model index in list</param>
        /// <param name="customEntityID"></param>
        public NetworkedGameObject CreateNetworkedGameObject(GameObject gObject, int modelListIndex = -1, int customEntityID = 0, bool doNotLinkWithButtonID = false)
        {
            //add a Net component to the object
            NetworkedGameObject netObject = gObject.AddComponent<NetworkedGameObject>();

            //to look a decomposed set of objects we need to keep track of what Index we are iterating over regarding or importing models to create sets
            //we keep a list reference for each index and keep on adding to it if we find a model with the same id
            //make sure we are using it as a button reference
            if (doNotLinkWithButtonID)
            {
                return InstantiateNetworkedGameObject(netObject, customEntityID, modelListIndex);
            }

            if (modelListIndex == -1)
            {
                return InstantiateNetworkedGameObject(netObject, customEntityID, modelListIndex);
            }

            List<NetworkedGameObject> subObjects;

            Dictionary<int, List<NetworkedGameObject>> netSubObjectLists = Instance.networkedSubObjectListFromIndex;

            if (!netSubObjectLists.ContainsKey(modelListIndex))
            {
                subObjects = new List<NetworkedGameObject>();

                subObjects.Add(netObject);

                netSubObjectLists.Add(modelListIndex, subObjects);

                return InstantiateNetworkedGameObject(netObject, customEntityID, modelListIndex);
            }

            subObjects = Instance.GetNetworkedSubObjectList(modelListIndex);

            subObjects.Add(netObject);

            netSubObjectLists[modelListIndex] = subObjects;

            return InstantiateNetworkedGameObject(netObject, customEntityID, modelListIndex);
        }

        protected NetworkedGameObject InstantiateNetworkedGameObject(NetworkedGameObject netObject, int entityId, int modelListIndex)
        {
            //to enable only imported objects to be grabbed
            //TODO: change for drawings
            netObject.tag = TagList.interactable;

            //We then set up the data to be used through networking
            if (entityId == 0)
            {
                netObject.Instantiate(modelListIndex);

                return netObject;
            }

            netObject.Instantiate(modelListIndex, entityId);

            return netObject;
        }

        public void LinkNetObjectToButton(int entityID, NetworkedGameObject netObject)
        {
            if (entityManager.HasComponent<ButtonIDSharedComponentData>(netObject.Entity))
            {
                var buttonID = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(netObject.Entity).buttonID;

                if (buttonID < 0 || buttonID >= ModelImportInitializer.Instance.networkedGameObjects.Count)
                {
                    throw new System.Exception("Button ID value is out-of-bounds for networked objects list.");
                }

                ModelImportInitializer.Instance.networkedGameObjects[buttonID] = netObject;
            }
        }
    }
}
