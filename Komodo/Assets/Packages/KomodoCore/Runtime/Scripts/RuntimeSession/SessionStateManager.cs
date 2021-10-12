using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    public class SessionStateManager : SingletonComponent<SessionStateManager>
    {
        public static SessionStateManager Instance
        {
            get { return (SessionStateManager) _Instance; }

            set { _Instance = value; }
        }

        private EntityManager entityManager;

        private SessionState _state;

        public void Awake ()
        {
            var forceAlive = Instance;

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        void Start ()
        {
        
        }

        public void SetSessionState (SessionState state)
        {
            _state = state;
        }

        public SessionState GetSessionState ()
        {
            return _state;
        }

        public bool IsReady ()
        {
            return (_state == null);
        }

        private EntityState GetEntityStateFromNetObject (NetworkedGameObject netObject)
        {
            int desiredEntityId = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netObject.Entity).entityID;

            foreach (var candidateEntityState in _state.entities)
            {
                if (candidateEntityState.id == desiredEntityId)
                {
                    return candidateEntityState;
                }
            }

            Debug.LogError($"SessionStateManager: Could not find EntityState that matched netObject with entity ID {desiredEntityId}");

            return new EntityState();
        }

        public void ApplyCatchup ()
        {
            if (!UIManager.IsAlive)
            {
                Debug.LogWarning("Tried to process session state for lock and visibility, but there was no UIManager.");
            }

            if (!SceneManagerExtensions.IsAlive)
            {
                Debug.LogWarning("Tried to process session state for scene, but there was no SceneManagerExtensions.");
            }

            if (!ClientSpawnManager.IsAlive)
            {
                Debug.LogWarning("Tried to process session state for clients, but there was no ClientSpawnManager.");
            }

            if (_state == null)
            {
                Debug.LogWarning("SessionStateManager: state was null. State catch-up will not be applied.");

                return;
            }

            SceneManagerExtensions.Instance.SelectScene(_state.scene);

            ClientSpawnManager.Instance.AddNewClients(_state.clients);

            foreach (var netObject in ModelImportInitializer.Instance.networkedGameObjects)
            {
                var entityState = GetEntityStateFromNetObject(netObject);

                UIManager.Instance.ProcessNetworkToggleVisibility(entityState.id, entityState.render);

                int interactionType = entityState.locked ? (int) INTERACTIONS.LOCK : (int) INTERACTIONS.UNLOCK;

                ApplyInteraction(new Interaction(
                    sourceEntity_id: -1,
                    targetEntity_id: entityState.id,
                    interactionType: interactionType
                ));
            }

            foreach (EntityState entity in _state.entities)
            {
                if (entity.latest != null  && entity.latest.Length > 0)
                {
                    ApplyPosition(NetworkUpdateHandler.Instance.DeSerializeCoordsStruct(entity.latest)); //TODO(Brandon): this is sus. Probably why reconnect button resets all objects.
                }
            }
        }

        public void ApplyPosition(Position positionData)
        {
            if (GameStateManager.IsAlive && !GameStateManager.Instance.isAssetImportFinished)
            {
                return;
            }

            if (positionData.entityType != (int)Entity_Type.objects && positionData.entityType != (int)Entity_Type.physicsObject)
            {
                ClientSpawnManager.Instance.AddClientIfNeeded(positionData.clientId);
            }

            switch (positionData.entityType)
            {
                case (int) Entity_Type.users_head:

                    ClientSpawnManager.Instance.ApplyPositionToHead(positionData);

                    break;

                case (int) Entity_Type.users_Lhand:

                    ClientSpawnManager.Instance.ApplyPositionToLeftHand(positionData);

                    break;

                case (int) Entity_Type.users_Rhand:

                    ClientSpawnManager.Instance.ApplyPositionToRightHand(positionData);

                    break;

                case (int) Entity_Type.objects:

                    NetworkedObjectsManager.Instance.ApplyPosition(positionData);

                    break;

                case (int) Entity_Type.physicsObject:

                    NetworkedPhysicsManager.Instance.ApplyPositionToStart(positionData);

                    break;

                case (int) Entity_Type.physicsEnd:

                    NetworkedPhysicsManager.Instance.ApplyPositionToEnd(positionData);

                    break;
            }
        }

        public void ApplyInteraction(Interaction interactionData)
        {
            NetworkedObjectsManager.Instance.ApplyInteraction(interactionData);
        }
    }

    [System.Serializable]
    public struct EntityState
    {
        public int id;
        public float[] latest; //position struct 
        public bool render;
        public bool locked;
    }

    [System.Serializable]
    public class SessionState
    {
        public int[] clients;
        public EntityState[] entities;
        public int scene;
        public bool isRecording;
    }
}
