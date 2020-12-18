using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Used to establish gameobject as a reference to be used in networking
/// </summary>
//add interfaces to invoke eventsystem interactions (look start, look end) for our net objects
public class Net_Register_GameObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //Register object to reference lists on clientspawnmanager to be refered to for synchronization
    [Tooltip("Entity_Data is created on Instantiate")]
    public Entity_Data entity_data;
    public bool usePhysics;

    //used to keep track of what UI element  does this object belongs to (for using with rendering and locking UI buttons)
    [HideInInspector]public int assetImportIndex = -1;
    [ShowOnly] [SerializeField]private bool isRegistered = false;
    private Rigidbody thisRigidBody;
    
    //this is used to keep tabs on a unique identifier for our decomposed objeccts that are instantiated
    private static int uniqueDefaultID;

    public IEnumerator Start()
    {
        //if we consider it a physics element we either get its rigidbody component, if it does not have one we add a new one
        if (usePhysics)
        {
            thisRigidBody = GetComponent<Rigidbody>();
            if (!thisRigidBody) gameObject.AddComponent<Rigidbody>();
        }

            yield return new WaitUntil(() => GameStateManager.Instance.isAssetLoading_Finished);

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
        //associate this entity with the UI index
        this.assetImportIndex = assetImportIndex;

        //create our entity data and set its field
        entity_data = ScriptableObject.CreateInstance<Entity_Data>();

        //set the type of entity it is 
        entity_data.current_Entity_Type = !usePhysics ? entity_data.current_Entity_Type = Entity_Type.objects : entity_data.current_Entity_Type = Entity_Type.physicsObject;

        //associate this entity with our client to be able to identify who is interacting with set object
        entity_data.clientID = NetworkUpdateHandler.Instance.client_id;

        ////if we do not get a uniqueEntityID we give it a default following this convention: ENTITYID DERIVED EXAMPLE =  CLIENTID - 65, ENTITY TYPE - 3, Count - 1 = ENTITYID 6531
        if (uniqueEntityID == -1)
            entity_data.entityID = (999 * 1000) + ((int)Entity_Type.objects * 100) + (uniqueDefaultID++);
        else
            entity_data.entityID = uniqueEntityID;

        //Setup References in ClientSpawnManager
        
        ClientSpawnManager.Instance.RegisterNetWorkObject(entity_data.entityID, this);
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
               sourceEntity_id = ClientSpawnManager.Instance.mainPlayer_RootTransformData.entityID,
               targetEntity_id = entity_data.entityID,
           });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        NetworkUpdateHandler.Instance.InteractionUpdate(
           new Interaction
           {
               interactionType = (int)INTERACTIONS.LOOK_END,
               sourceEntity_id = ClientSpawnManager.Instance.mainPlayer_RootTransformData.entityID,
               targetEntity_id = entity_data.entityID,
           });
    }
    #endregion
}
