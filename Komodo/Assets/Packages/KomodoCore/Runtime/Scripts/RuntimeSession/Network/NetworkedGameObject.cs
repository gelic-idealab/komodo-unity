//#define TESTING_BEFORE_BUILDING

using System.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    /// <summary>
    /// Used to establish gameobject as a reference to be used in networking
    /// </summary>
    //add interfaces to invoke eventsystem interactions (look start, look end) for our net objects
    public class NetworkedGameObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        //Register object to reference lists on clientspawnmanager to be refered to for synchronization
        [Tooltip("Entity_Data is created on Instantiate")]

        public bool usePhysics;

        [ShowOnly] [SerializeField] private bool isRegistered = false;

        //used to keep track of what UI element  does this object belongs to (for using with rendering and locking UI buttons)
        [ShowOnly] public int buttonIndex = -1;

        [ShowOnly] public int thisEntityID;

        private Rigidbody thisRigidBody;

        //entity used to access our data through entityManager
        public Entity Entity;

        private EntityManager entityManager;

        public IEnumerator Start()
        {
            //if we consider it a physics element we either get its rigidbody component, if it does not have one we add a new one
            InitializePhysicsComponentsIfNeeded();

            yield return new WaitUntil(() => GameStateManager.Instance.isAssetImportFinished);

            InstantiateIfNeeded();
        }

        private void InstantiateIfNeeded()
        {
            //if this object was not instantiated early we make sure to instantiate it whenever it is active in scene
            if (isRegistered)
            {
                return;
            }

            Instantiate();
        }

        private void InitializePhysicsComponentsIfNeeded()
        {
            if (usePhysics)
            {
                thisRigidBody = GetComponent<Rigidbody>();

                if (!thisRigidBody)
                {
                    gameObject.AddComponent<Rigidbody>();
                }
            }
        }

        /// <summary>
        /// Instantiate this object to be referenced through the network
        /// </summary>
        /// <param name="importIndex"> used to assoicate our entity with a UI index to reference it with buttons</param>
        /// <param name="uniqueEntityID">if we give this paramater, we set it as the entity ID instead of giving it a default id</param>
        public void Instantiate(int importIndex = -1, int uniqueEntityID = -1)
        {
            //get our entitymanager to get access to the entity world
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            //set custom id if we are not given a specified id when instantiating this network associated object
            int EntityID = (uniqueEntityID == -1) ? NetworkedObjectsManager.Instance.GenerateUniqueEntityID() : uniqueEntityID;

            //create our entity reference
            if (Entity == Entity.Null)
            {
                Entity = entityManager.CreateEntity();
            }

            thisEntityID = EntityID;

#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
//do nothing
#else
            entityManager.SetName(Entity, gameObject.name);
#endif

            buttonIndex = importIndex;

            //set the data that our entity will be storing
            if (buttonIndex != -1)
            {
                entityManager.AddSharedComponentData(Entity, new ButtonIDSharedComponentData { buttonID = buttonIndex });
            }

            entityManager.AddComponentData(Entity, new NetworkEntityIdentificationComponentData
            {
                entityID = EntityID,

                clientID = NetworkUpdateHandler.Instance.client_id,

                sessionID = NetworkUpdateHandler.Instance.session_id,

                current_Entity_Type = !usePhysics ? Entity_Type.objects : Entity_Type.physicsObject,
            });

            NetworkedObjectsManager.Instance.Register(EntityID, this);

                //TODO: evaluate how good this solution is.
            //check to see if the gameObject is the main object or a subobject. If it's a main object, link it to the button.
            if (this.name == buttonIndex.ToString())
            {
                NetworkedObjectsManager.Instance.LinkNetObjectToButton(EntityID, this);
            }

            isRegistered = true;
        }

        #region Physics Interaction Events (Add to network on collision)
        //if this object is a physics object detect when it collides to mark it to send its position information
        public void OnCollisionEnter(Collision collision)
        {
            //check if other object interacting has a rigidbody
            if (!collision.rigidbody)
            {
                return;
            }

            if (!usePhysics || !collision.rigidbody.CompareTag(TagList.interactable))
            {
                return;
            }

            if (!NetworkedPhysicsManager.Instance.physics_networkedEntities.Contains(this))
            {
                NetworkedPhysicsManager.Instance.physics_networkedEntities.Add(this);
            }

            if (entityManager.HasComponent<SendNetworkUpdateTag>(Entity))
            {
                return;
            }

            entityManager.AddComponent<SendNetworkUpdateTag>(Entity);
        }
        #endregion

        private void SendLookStartInteraction()
        {
            NetworkUpdateHandler.Instance.SendSyncInteractionMessage(
               new Interaction
               {
                   interactionType = (int)INTERACTIONS.LOOK,

                   sourceEntity_id = NetworkUpdateHandler.Instance.client_id,

                   targetEntity_id = thisEntityID,
               }
            );
        }

        private void SendLookEndInteraction()
        {
            NetworkUpdateHandler.Instance.SendSyncInteractionMessage(
               new Interaction
               {
                   interactionType = (int)INTERACTIONS.LOOK_END,

                   sourceEntity_id = NetworkUpdateHandler.Instance.client_id,

                   targetEntity_id = thisEntityID,
               }
            );
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SendLookStartInteraction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SendLookEndInteraction();
        }
    }
}