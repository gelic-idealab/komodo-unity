using Komodo.Runtime;
using Komodo.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class StretchManager : SingletonComponent<StretchManager>, IUpdatable
{
    public static StretchManager Instance
    {
        get { return ((StretchManager)_Instance); }
        set { _Instance = value; }
    }

    [ShowOnly] public Transform firstObjectGrabbed;

    //get parent if we are switching objects between hands we want to keep track of were to place it back, to avoid hierachy parenting displacement
    [ShowOnly] public Transform originalParentOfFirstHandTransform;

    [ShowOnly] public Transform secondObjectGrabbed;

    [ShowOnly] public Transform originalParentOfSecondHandTransform;

    [ShowOnly] public Transform[] hands = new Transform[2];

    /// <summary>
    /// An UnityEvent for the moment when stretch starts.
    /// </summary>
    public UnityEvent onStretchStart;

    /// <summary>
    /// An UnityEvent for the moment when stretch ends.
    /// </summary>
    public UnityEvent onStretchEnd;

    [ShowOnly] public Transform endpoint1;

    private static Transform endpoint0;

    [ShowOnly] public Transform midpoint;

    [ShowOnly] public Transform stretchParent;

    [ShowOnly] public bool didStartStretching;

    public bool debug;

    public GameObject axesPrefab;

    private float initialDistance;

    private Vector3 initialScale;

    private GameObject objectPoseDisplay;

    public void Awake()
    {
        //set our manager's alive state to true to detect if it exists
        var initManager = Instance;

        onStretchStart.AddListener(() => StretchStart());

        onStretchEnd.AddListener(() => StretchRelease());

        var stretchEndpoint0Object = CreatePivotPoint("StretchEndpoint0", debug, axesPrefab);

        endpoint0 = stretchEndpoint0Object.transform;

        endpoint0.SetParent(transform.parent, true);

        endpoint0.localPosition = Vector3.zero;

        var stretchEndpoint1Object = CreatePivotPoint("StretchEndpoint1", debug, axesPrefab);

        endpoint1 = stretchEndpoint1Object.transform;

        endpoint1.parent = transform.parent;

        var stretchMidpointObject = CreatePivotPoint("StretchMidpoint", debug, axesPrefab);

        midpoint = stretchMidpointObject.transform;

        midpoint.SetParent(transform.parent, true);

        midpoint.localPosition = Vector3.zero;

        objectPoseDisplay = CreatePivotPoint("ObjectPose", debug, axesPrefab);
    }
    /// <summary>
    /// Creates a GameObject and gives it a name. 
    /// </summary>
    /// <param name="name">name of the GameObject</param>
    /// <param name="doDisplay">if true, instantiate from a prefab; otherwise, return a default GameObject.</param>
    /// <param name="prefab">the prefab to create and name</param>
    /// <returns></returns>
    public GameObject CreatePivotPoint (string name, bool doDisplay, GameObject prefab) {
        if (doDisplay)
        {
            var result = Instantiate(prefab);

            result.name = name;

            return result;
        }

        return new GameObject(name);
    }


    /// <summary>
    /// Get player's reference and then initialize player's hands.
    /// </summary>
    public void Start()
    {
        var player = GameObject.FindWithTag(TagList.player);

        if (player)
        {
           if(player.TryGetComponent(out PlayerReferences pR))
            {
                hands[0] = pR.handL;
                hands[1] = pR.handR;
            }
        }

        //set references for parent
        stretchParent = endpoint1.parent;
    }

    /// <summary>
    /// Start the stretch and register StretchManager from <c>GameStateManager</c>.
    /// </summary>
    private void StretchStart()
    {
        if (GameStateManager.IsAlive)
        {
            GameStateManager.Instance.RegisterUpdatableObject(this);
        }
    }

    /// <summary>
    /// Stop the stretch and de-register StretchManager from <c>GameStateManager</c>.  
    /// </summary>
    private void StretchRelease()
    {
        if (GameStateManager.IsAlive)
        {
            GameStateManager.Instance.DeRegisterUpdatableObject(this);
        }
    }

    /// <summary>
    /// Update the stretch results in real time.
    /// </summary>
    /// <param name="realltime">the time since the Komodo starts</param>
    public void OnUpdate(float realltime)
    {
        if (didStartStretching == false)
        {
            firstObjectGrabbed.SetParent(stretchParent, true);

            UpdateGrabPoints();

            InitializeScale();

            objectPoseDisplay.transform.SetParent(firstObjectGrabbed);

            objectPoseDisplay.transform.localPosition = Vector3.zero;

            firstObjectGrabbed.SetParent(midpoint, true);

            didStartStretching = true;

            return;
        }

        UpdateGrabPoints();

        UpdateScale();

        UpdateRotationAndPosition();
    }

    private void UpdateRotationAndPosition ()
    {
        endpoint1.rotation = hands[1].transform.rotation;

        endpoint0.rotation = hands[0].transform.rotation;

        Vector3 averageUp = (endpoint0.up + endpoint1.up) / 2;

        midpoint.LookAt(endpoint0.position, averageUp);
    }

    /// <summary>
    /// Update the grab point of with positions and rotations of both hands. Then calculate the midpoint's positions and rotations for the object. Rotate <c>midpoint</c> based on these calculations.
    /// </summary>
    private void UpdateGrabPoints () {
        endpoint1.position = hands[1].transform.position;

        endpoint0.position = hands[0].transform.position;

        midpoint.position = (endpoint0.position + endpoint1.position) / 2;

        endpoint1.rotation = hands[1].transform.rotation;

        endpoint0.rotation = hands[0].transform.rotation;

        Vector3 averageUp = (endpoint0.up + endpoint1.up) / 2;

        //rotate the midpoint so the forward vector of midpoint points at endpoint0.position. Then rotates its updirection in the direction hinted by averageUp.
        midpoint.LookAt(endpoint0.position, averageUp);
    }

    private void InitializeScale () {
        initialDistance = Vector3.Distance(endpoint1.position, endpoint0.position);

        initialScale = Vector3.one;

        midpoint.localScale = Vector3.one;
    }

    /// <summary>
    /// Update the scale of the object.
    /// </summary>
    private void UpdateScale () {
        var currentScaleRatio = GetCurrentScaleRatio();

        if (float.IsNaN(firstObjectGrabbed.localScale.y)) {
            Debug.LogError("First Object Grabbed's' local scale was NaN");
        }

        midpoint.localScale = initialScale * currentScaleRatio;
    }

    /// <summary>
    /// Get the current scale ratio of the object that is held in hands.
    /// </summary>
    /// <returns>return the scale ratio of the object.</returns>
    private float GetCurrentScaleRatio () {
        return Vector3.Distance(hands[0].transform.position, hands[1].transform.position) / initialDistance;
    }
}
