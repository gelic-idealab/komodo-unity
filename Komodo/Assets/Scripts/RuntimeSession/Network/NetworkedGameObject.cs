using System.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Used to establish gameobject as a reference to be used in networking
/// </summary>
//add interfaces to invoke eventsystem interactions (look start, look end) for our net objects
public class NetworkedGameObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //Register object to reference lists on clientspawnmanager to be refered to for synchronization
    [Tooltip("Entity_Data is created on Instantiate")]
    //public Entity_Data entity_data;
    public bool usePhysics;
    
    [ShowOnly] [SerializeField] private bool isRegistered = false;

    //used to keep track of what UI element  does this object belongs to (for using with rendering and locking UI buttons)
    [ShowOnly] public int buttonID = -1;
    [ShowOnly] public int thisEntityID;

    private Rigidbody thisRigidBody;

    //this is used to keep tabs on a unique identifier for our decomposed objeccts that are instantiated
    private static int uniqueDefaultID;

    //entity used to access our data through entityManager
    public Entity Entity;
    private EntityManager entityManager;

    public IEnumerator Start()
    {
        // Unity.Entities.GameObjectEntity.AddToEntity(World.DefaultGameObjectInjectionWorld.EntityManager, gameObject, entityData);
        //if we consider it a physics element we either get its rigidbody component, if it does not have one we add a new one
        if (usePhysics)
        {
            thisRigidBody = GetComponent<Rigidbody>();
            if (!thisRigidBody) gameObject.AddComponent<Rigidbody>();
        }

        yield return new WaitUntil(() => GameStateManager.Instance.isAssetImportFinished);

        //if this object was not instantiated early we make sure to instantiate it whenever it is active in scene
        if (isRegistered == false)
            Instantiate(-1);
    }

    /// <summary>
    /// Instantiate this object to be referenced through the network
    /// </summary>
    /// <param name="assetImportIndex"> used to assoicate our entity with a UI index to reference it with buttons</param>
    /// <param name="uniqueEntityID">if we give this paramater, we set it as the entitty ID instead of giving it a default id</param>
    public void Instantiate(int assetImportIndex, int uniqueEntityID = -1)
    {
        //get our entitymanager to get access to the entity world
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
      
        //set custom id if we are not given a specified id when instantiating this network associated object
        int EntityID = (uniqueEntityID == -1) ? (999 * 1000) + ((int)Entity_Type.objects * 100) + (uniqueDefaultID++) : uniqueEntityID;

        //create our entity reference
        if (Entity == Entity.Null)
            Entity = entityManager.CreateEntity();

        //to see in the editor for debugging purposes
        this.buttonID = assetImportIndex;
        thisEntityID = EntityID;
#if UNITY_EDITOR
        entityManager.SetName(Entity, gameObject.name);
#endif
        //set the data that our entity will be storing
        if (assetImportIndex != -1){
            entityManager.AddSharedComponentData(Entity, new ButtonIDSharedComponentData { buttonID = assetImportIndex });
        }
        
        entityManager.AddComponentData(Entity, new NetworkEntityIdentificationComponentData
        {
            entityID = EntityID,
            clientID = NetworkUpdateHandler.Instance.client_id,
            sessionID = NetworkUpdateHandler.Instance.session_id,
            current_Entity_Type = !usePhysics ? Entity_Type.objects : Entity_Type.physicsObject,

        });

        ClientSpawnManager.Instance.RegisterNetworkedGameObject(EntityID, this);

        isRegistered = true;
    }

    #region Physics Interaction Events (Add to network on collision)
    //if this object is a physics object detect when it collides to mark it to send its position information
    public void OnCollisionEnter(Collision collision)
    {
        //check if other object interacting has a rigidbody
        if (!collision.rigidbody)
            return;

        if (usePhysics && collision.rigidbody.CompareTag("Interactable"))
        {
            if (!MainClientUpdater.Instance.physics_entityContainers_InNetwork_OutputList.Contains(this))
                MainClientUpdater.Instance.physics_entityContainers_InNetwork_OutputList.Add(this);

            if (!entityManager.HasComponent<SendNetworkUpdateTag>(Entity))
                entityManager.AddComponent<SendNetworkUpdateTag>(Entity);
        }
    }
    #endregion

    #region EventSystem Interaction Events (Look and LookEnd Network Calls)
    //call to send information to the server about our look at behavio
    public void OnPointerEnter(PointerEventData eventData)
    {
        NetworkUpdateHandler.Instance.InteractionUpdate(
           new Interaction
           {
               interactionType = (int)INTERACTIONS.LOOK,
               sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
               targetEntity_id = thisEntityID,
           });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        NetworkUpdateHandler.Instance.InteractionUpdate(
           new Interaction
           {
               interactionType = (int)INTERACTIONS.LOOK_END,
               sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
               targetEntity_id = thisEntityID,
           });
    }
    #endregion
}
