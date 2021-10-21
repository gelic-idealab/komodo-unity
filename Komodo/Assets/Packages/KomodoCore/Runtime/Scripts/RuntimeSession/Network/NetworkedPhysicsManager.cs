using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    public class NetworkedPhysicsManager : SingletonComponent<NetworkedPhysicsManager>
    {
        public static NetworkedPhysicsManager Instance
        {
            get { return (NetworkedPhysicsManager)_Instance; }

            set { _Instance = value; }
        }

        private EntityManager entityManager;

        public Dictionary<int, Rigidbody> rigidbodyFromEntityId = new Dictionary<int, Rigidbody>();

        List<NetworkedGameObject> physicsnRGOToRemove = new List<NetworkedGameObject>();
        [HideInInspector] public List<NetworkedGameObject> physics_networkedEntities = new List<NetworkedGameObject>();

        private NetworkUpdateHandler netUpdateHandler;

        void Start ()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            netUpdateHandler = NetworkUpdateHandler.Instance;

            if (netUpdateHandler == null)
            {
                throw new System.Exception("NetworkUpdateHandler instance not found.");
            }
        }

        public void OnUpdate()
        {
            foreach (var entityContainers in physics_networkedEntities)
            {
                SendPhysicsGameObjectUpdatesToNetwork(entityContainers);
            }

            //remove physics objects that should not send calls anymore if RigidBody is changed to isKinematic or IsSleeping()
            foreach (var item in physicsnRGOToRemove)
            {
                physics_networkedEntities.Remove(item);
            }

            //clear the list of physics objects to remove from sending updates
            physicsnRGOToRemove.Clear();
        }

        public void ApplyPositionToStart(Position positionData)
        {
            //alternate kinematic to allow for sending non physics transform updates;
            if (NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(positionData.entityId))
            {
                if (!rigidbodyFromEntityId.ContainsKey(positionData.entityId))
                {
                    rigidbodyFromEntityId.Add(positionData.entityId, NetworkedObjectsManager.Instance.networkedObjectFromEntityId[positionData.entityId].GetComponent<Rigidbody>());
                }

                var rb = rigidbodyFromEntityId[positionData.entityId];

                if (!rb)
                {
                    Debug.LogError("There is no rigidbody in netobject entity id: " + positionData.entityId);

                    return;
                }

                rb.isKinematic = true;

                NetworkedObjectsManager.Instance.networkedObjectFromEntityId[positionData.entityId].transform.position = positionData.pos;

                NetworkedObjectsManager.Instance.networkedObjectFromEntityId[positionData.entityId].transform.rotation = positionData.rot;

                UnityExtensionMethods.SetGlobalScale(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[positionData.entityId].transform, Vector3.one * positionData.scaleFactor);
            }
            else
            {
                Debug.LogWarning("Entity ID : " + positionData.entityId + "not found in Dictionary dropping physics object movement packet");
            }
        }

        public void ApplyPositionToEnd(Position positionData)
        {
            //alternate kinematic to allow for sending non physics transform updates;
            if (NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(positionData.entityId))
            {
                //skip opperation if current object is grabbed to avoid turning physics back on

                if (entityManager.HasComponent<TransformLockTag>(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[positionData.entityId].Entity))
                    return;

                if (!rigidbodyFromEntityId.ContainsKey(positionData.entityId))
                {
                    rigidbodyFromEntityId.Add(positionData.entityId, NetworkedObjectsManager.Instance.networkedObjectFromEntityId[positionData.entityId].GetComponent<Rigidbody>());
                }

                var rb = rigidbodyFromEntityId[positionData.entityId];

                if (!rb)
                {
                    Debug.LogError("There is no rigidbody in netobject entity id: " + positionData.entityId);

                    return;
                }

                rb = NetworkedObjectsManager.Instance.networkedObjectFromEntityId[positionData.entityId].GetComponent<Rigidbody>();

                rb.isKinematic = false;
            }
            else
            {
                Debug.LogWarning("Entity ID : " + positionData.entityId + "not found in Dictionary dropping physics object movement packet");
            }
        }

        /// <summary>
        /// Meant to convert our Physics GameObject data send  data to follow our POSITION struct to be sent each update
        /// </summary>
        /// <param name="Net_Register_GameObject container of data"></param>
        public void SendPhysicsGameObjectUpdatesToNetwork(NetworkedGameObject eContainer)
        {
            int entityID = default;
            NetworkEntityIdentificationComponentData entityIDContainer = default;

            entityIDContainer = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(eContainer.Entity);
            entityID = entityIDContainer.entityID;

            //make sure that we setup the reference to our rigidBody of our physics object that we are using to send data from
            if (!rigidbodyFromEntityId.ContainsKey(entityID))
            {
                rigidbodyFromEntityId.Add(entityID, eContainer.GetComponent<Rigidbody>());
            }
            var rb = rigidbodyFromEntityId[entityID];

            if (!rb)
            {
                Debug.LogError("There is no rigidbody in netobject entity id DICTIONARY: " + entityID);
                return;
            }

            Position coords = default;

            if (!rb.isKinematic && rb.IsSleeping() || entityManager.HasComponent<TransformLockTag>(eContainer.Entity))
            {
                physicsnRGOToRemove.Add(eContainer);

                //Send a last update for our network objects to be remove their physics funcionality to sync with others. 
                StopPhysicsUpdates(eContainer);
            }

            coords = new Position
            {
                clientId = entityIDContainer.clientID,
                entityId = entityIDContainer.entityID,
                entityType = (int)entityIDContainer.current_Entity_Type,
                rot = eContainer.transform.rotation,
                pos = eContainer.transform.position,
                scaleFactor = eContainer.transform.lossyScale.x,
            };

            netUpdateHandler.SendSyncPoseMessage(coords);
        }

    /// <summary>
    /// A call to remove Physics funcionality from specified netObject 
    /// </summary>
    /// <param name="eContainer"></param>
    public void StopPhysicsUpdates(NetworkedGameObject eContainer)
    {
        Position coords = default;

        NetworkEntityIdentificationComponentData entityIDContainer = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(eContainer.Entity);

        coords = new Position
        {
            clientId = entityIDContainer.clientID,
            entityId = entityIDContainer.entityID,
            entityType = (int)Entity_Type.physicsEnd,

        };

        netUpdateHandler.SendSyncPoseMessage(coords);
    }

        public Rigidbody GetRigidbody(int entityId)
        {
            Rigidbody result;

            bool success = rigidbodyFromEntityId.TryGetValue(entityId, out result);

            if (!success)
            {
                throw new System.Exception($"Value was not found in client's rigidbodies dictionary for key {entityId}.");
            }

            return result;
        }
    }
}
