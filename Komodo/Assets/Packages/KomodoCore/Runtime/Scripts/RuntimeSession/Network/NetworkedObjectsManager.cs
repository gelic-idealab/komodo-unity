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

        // Returns true iff the entity type matched and  
        public bool TryToApplyPosition (Position positionData)
        {
            if (positionData.entityType != (int) Entity_Type.objects)
            {
                return false;
            }

            int entityId = positionData.entityId;

            if (!networkedObjectFromEntityId.ContainsKey(entityId))
            {
                Debug.LogWarning("Entity ID : " + positionData.entityId + "not found in Dictionary dropping object movement packet");

                return false;
            }

            Transform netObjTransform = networkedObjectFromEntityId[entityId].transform;

            if (!netObjTransform)
            {
                Debug.LogError($"TryToApplyPosition: NetObj with entityID {entityId} had no Transform component");

                return false;
            }

            netObjTransform.position = positionData.pos;

            netObjTransform.rotation = positionData.rot;

            UnityExtensionMethods.SetGlobalScale(netObjTransform, Vector3.one * positionData.scaleFactor);

            return true;
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
                case (int) INTERACTIONS.SHOW:

                    if (UIManager.IsAlive)
                    {
                        UIManager.Instance.ProcessNetworkToggleVisibility(interactionData.targetEntity_id, true);
                    }

                    break;

                case (int) INTERACTIONS.HIDE:

                    if (UIManager.IsAlive)
                    {
                        UIManager.Instance.ProcessNetworkToggleVisibility(interactionData.targetEntity_id, false);
                    }

                    break;

                case (int) INTERACTIONS.GRAB:

                    Instance.ApplyGrabStartInteraction(interactionData);

                    break;

                case (int) INTERACTIONS.DROP:

                    Instance.ApplyGrabEndInteraction(interactionData);

                    break;

                case (int) INTERACTIONS.CHANGE_SCENE:

                    if (SceneManagerExtensions.IsAlive)
                    {
                        //check the loading wait for changing into a new scene - to avoid loading multiple scenes
                        SceneManagerExtensions.Instance.SelectScene(interactionData.targetEntity_id);
                    }

                    break;

                case (int) INTERACTIONS.LOCK:

                    Instance.ApplyLockInteraction(interactionData);

                    break;

                case (int) INTERACTIONS.UNLOCK:

                    Instance.ApplyUnlockInteraction(interactionData);

                    break;

                case (int) INTERACTIONS.LOOK:

                    // Do nothing

                    break;

                case (int) INTERACTIONS.LOOK_END:

                    // Do nothing

                    break;

                default:

                    Debug.LogWarning ($"Tried to ApplyInteraction, but interaction type {interactionData.interactionType} was unknown. Skipping");

                    break;
            }
        }

        // TODO -- add error checking.
        public void ApplyGrabStartInteraction (Interaction interactionData)
        {
            entityManager.AddComponentData(Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity, new TransformLockTag());
        }

        // TODO -- add error checking.
        public void ApplyGrabEndInteraction (Interaction interactionData)
        {
            if (!entityManager.HasComponent<TransformLockTag>(Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity))
            {
                Debug.LogWarning("Client Entity does not exist for Drop interaction--- EntityID" + interactionData.targetEntity_id);

                return;
            }

            entityManager.RemoveComponent<TransformLockTag>(Instance.networkedObjectFromEntityId[interactionData.targetEntity_id].Entity);
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

        // Returns true for success and false for failure
        public bool DestroyAndUnregisterEntity (int id)
        {
            if (!networkedObjectFromEntityId.ContainsKey(id))
            {
                Debug.LogWarning($"Networked Object with key {id} will not be destroyed or unregistered, because it was never registered.");

                return false;
            }

            Destroy(networkedObjectFromEntityId[id].gameObject);

            networkedObjectFromEntityId.Remove(id);

            return true;
        }

        // Returns true for success and false for failure
        public bool ShowEntity (int id)
        {
            if (!NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(id))
            {
                Debug.LogWarning($"Networked Object {id} will not be shown, because it was never registered.");

                return false;
            }

            NetworkedObjectsManager.Instance.networkedObjectFromEntityId[id].gameObject.SetActive(true);

            return true;
        }

        // Returns true for success and false for failure
        public bool HideEntity (int id)
        {
            if (!networkedObjectFromEntityId.ContainsKey(id))
            {
                Debug.LogWarning($"Networked Object {id} will not be hidden, because it was never registered.");

                return false;
            }

            networkedObjectFromEntityId[id].gameObject.SetActive(false);

            return true;
        }
    }
}
