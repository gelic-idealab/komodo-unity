using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class TriggerCreatePrimitive : MonoBehaviour
{
    private Transform thisTransform;
    private int primitiveTypeCreating;
    private PrimitiveType currentPrimitiveType;

    public GameObject primitiveCreationParent;

    //reference to pick up what toggle is currently on;
    public ToggleGroup primitiveToggleGroup;
    public Toggle sphereToggle;
    public Toggle capsuleToggle;
    public Toggle CylinderToggle;
    public Toggle CubeToggle;
    public Toggle PlaneToggle;
    // public Toggle QuadToggle;
    public Toggle currentToggle;

    public Transform parentOfPrimitiveObjectsToDisplay;

    public EntityManager entityManager;
    public void Awake() { thisTransform = transform; }

    public bool isInitialized = false;
    public void Initialize()
    {
        //only initialize once
        if (isInitialized == true)
            return;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        isInitialized = true;

   //     parentOfPrimitiveObjectsToDisplay.GetChild(0).gameObject.SetActive(true);

        //Set current toggle when we are changing it
        sphereToggle.onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); parentOfPrimitiveObjectsToDisplay.GetChild(0).gameObject.SetActive(true);   });
        capsuleToggle.onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); parentOfPrimitiveObjectsToDisplay.GetChild(1).gameObject.SetActive(true); });
        CylinderToggle.onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); parentOfPrimitiveObjectsToDisplay.GetChild(2).gameObject.SetActive(true); });
        CubeToggle.onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); parentOfPrimitiveObjectsToDisplay.GetChild(3).gameObject.SetActive(true); });
        PlaneToggle.onValueChanged.AddListener((bool state) => { UpdateCurrentToggleOn(state); DeactivateAllChildren(); parentOfPrimitiveObjectsToDisplay.GetChild(4).gameObject.SetActive(true); });
    }

    public void UpdateCurrentToggleOn(bool state)
    {
        currentToggle = primitiveToggleGroup.GetFirstActiveToggle();
    }


    // Start is called before the first frame update
    public void OnEnable()
    {
        //only create when our cursor is Off
        if (UIManager.Instance.GetCursorActiveState())
            return;

        //get active toggle and look for corresponding primitive type toggles
        if (primitiveToggleGroup.GetFirstActiveToggle() != null)// if(UIManager.Instance.mainUIDashboard)//primitiveToggleGroup.gameObject.activeInHierarchy) //&& primitiveToggleGroup.IsActive())
            currentToggle = primitiveToggleGroup.GetFirstActiveToggle();

        if (!currentToggle)
            return;

        GameObject primitive = default;
        var rot = Quaternion.identity;
        var scale =  Vector3.one * 0.2f;

        if (currentToggle.GetInstanceID() == sphereToggle.GetInstanceID())
        {
            currentPrimitiveType = PrimitiveType.Sphere;
            rot = parentOfPrimitiveObjectsToDisplay.GetChild(0).rotation;
            scale = parentOfPrimitiveObjectsToDisplay.GetChild(0).lossyScale;
        }
        else if (currentToggle.GetInstanceID() == capsuleToggle.GetInstanceID())
        {
            currentPrimitiveType = PrimitiveType.Capsule;
            rot = parentOfPrimitiveObjectsToDisplay.GetChild(1).rotation;
            scale = parentOfPrimitiveObjectsToDisplay.GetChild(1).lossyScale;
        }
        else if (currentToggle.GetInstanceID() == CylinderToggle.GetInstanceID())
        {
            currentPrimitiveType = PrimitiveType.Cylinder;
            rot = parentOfPrimitiveObjectsToDisplay.GetChild(2).rotation;
            scale = parentOfPrimitiveObjectsToDisplay.GetChild(2).lossyScale;
        }
        else if (currentToggle.GetInstanceID() == CubeToggle.GetInstanceID())
        {
            currentPrimitiveType = PrimitiveType.Cube;
            rot = parentOfPrimitiveObjectsToDisplay.GetChild(3).rotation;
            scale = parentOfPrimitiveObjectsToDisplay.GetChild(3).lossyScale;
        }
        else if (currentToggle.GetInstanceID() == PlaneToggle.GetInstanceID())
        {
            currentPrimitiveType = PrimitiveType.Plane;
            rot = parentOfPrimitiveObjectsToDisplay.GetChild(4).rotation;
            scale = parentOfPrimitiveObjectsToDisplay.GetChild(4).lossyScale;
        }

        primitive = GameObject.CreatePrimitive(currentPrimitiveType);
        NetworkedGameObject nAGO =  ClientSpawnManager.Instance.CreateNetworkedGameObject(primitive);

        //tag it to be used with ECS system
        entityManager.AddComponentData(nAGO.Entity, new PrimitiveTag { });
       // primitive.tag = "Interactable";
     //   entityManager.AddComponentData(nAGO.Entity, new NetworkEntityIdentificationComponentData { clientID = NetworkUpdateHandler.Instance.client_id, entityID = 0, sessionID = NetworkUpdateHandler.Instance.session_id, current_Entity_Type = Entity_Type.none });


        primitive.transform.position = thisTransform.position;

        primitive.transform.SetGlobalScale(scale); 

        primitive.transform.rotation = rot;
        primitive.transform.SetParent(primitiveCreationParent.transform, true);
    //    primitive.transform.localPosition = Vector3.zero;
    }

    public void OnDisable()
    {
        //avoid our currentToggle from being set to null when we deactivate our UI
        if(primitiveToggleGroup.GetFirstActiveToggle() != null)
        currentToggle = primitiveToggleGroup.GetFirstActiveToggle();// primitiveToggleGroup.ActiveToggles;

    }

    public void DeactivateAllChildren()
    {
        foreach (Transform item in parentOfPrimitiveObjectsToDisplay)
            item.gameObject.SetActive(false);

    }
}
