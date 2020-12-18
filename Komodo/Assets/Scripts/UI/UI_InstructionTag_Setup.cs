using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UnityEvent_Register : UnityEvent<UnityAction> { }

public class UI_InstructionTag_Setup : MonoBehaviour
{

    [SerializeField] private Transform _thisTransform;


    private bool isInVR;
    [Header("VR Settings")]
    [SerializeField] private Transform _NonVRCamera;
    [SerializeField] private Transform _VRCamera;

    [Space]

    [Header("LineRenderer Settings")]
    [SerializeField] private LineRenderer _thisLineRenderer;
    [SerializeField] private Transform _LineRendOffsetTransform;
    [SerializeField] private float _RenderLineTag_Y_Offset = 0.3f;
    [SerializeField] private Transform _targetForLineIndication;

    [Header("Contact Rigidbody Check")]
    public bool isContactRigidbodyCheck = false;
    public string nameOfContactRigidbodyToCheck = default;
    public List<ControllerInteraction> controllerInteractionList = new List<ControllerInteraction>();

    [Space]
    [Header("Input_Events")]
    public UnityEvent_Register UITag_Register_Event;
    public UnityEvent_Register UITag_DeRegister_Event;
    public void RegisterEvent_To_Conplete() => isEventCompleted = true;//AddListener(() => regEvent; );}

    public bool isEventCompleted;

    [Space]

    [Header("Output_Events")]
    public UnityEvent UITag_Complete_Event;
    public void TagComplete() => UITag_Complete_Event?.Invoke();

    public void Awake()
    {
        _thisTransform = transform;
    }

    private bool isUpdateReady = false;
    private IEnumerator Start()
    {
        yield return new WaitUntil(()=> GameStateManager.Instance.isClientAvatarLoading_Finished);

        isUpdateReady = true;

        if (_targetForLineIndication != null)
            _thisLineRenderer = GetComponent<LineRenderer>();

        _NonVRCamera = GameObject.FindGameObjectWithTag("Player").transform;
        _VRCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;// Camera.main.transform;

        if (isContactRigidbodyCheck)
        {
            GameObject[] hands = GameObject.FindGameObjectsWithTag("Hand");
            
            foreach (GameObject hand in hands)
            {
                controllerInteractionList.Add(hand.GetComponent<ControllerInteraction>());
            }

        }
        //(_NonVRCamera, _VRCamera)  = (GameObject.FindGameObjectWithTag("Player").transform, Camera.main.transform);
    }

    public bool isTimerComplete = false;
    public float timeUntiComplete = 0;
    //SEND EVENT TO BE CHECKED FOR INPUT -> NEED TO SET UP FUNCTIONS IN THE EXTERIOR TO REGISTER IT WITH THEIR UNITY EVENTS
    private void OnEnable()
    {
        UITag_Register_Event.Invoke(RegisterEvent_To_Conplete);

        if (isTimerComplete)
            Invoke("RegisterEvent_To_Conplete", timeUntiComplete);
    }

    private void OnDisable()
    {
        isEventCompleted = false;


        UITag_DeRegister_Event.Invoke(RegisterEvent_To_Conplete);

        if (isTimerComplete)
            CancelInvoke("RegisterEvent_To_Conplete");
    }

    void Update()
    {
        if (isEventCompleted)
        {
            UITag_Complete_Event.Invoke();
            isEventCompleted = false;
            return;

        }

        if (!isUpdateReady)
            return;

        if (_targetForLineIndication != null)
        {
            Vector3 modInitialYOffset = new Vector3(_LineRendOffsetTransform.position.x, _LineRendOffsetTransform.position.y - _RenderLineTag_Y_Offset, _LineRendOffsetTransform.position.z);
            _thisLineRenderer.SetPosition(0, modInitialYOffset);

            _thisLineRenderer.SetPosition(1, _targetForLineIndication.position);
        }

        Transform cameraToFollow = isInVR ? _VRCamera : _NonVRCamera;


        if (isContactRigidbodyCheck)
        {
            foreach (ControllerInteraction curRb in controllerInteractionList)
            {
                //if (curRb.currentTransform == null)
                //        return;

                //if(curRb.currentTransform.name == nameOfContactRigidbodyToCheck)
                //{
                //    RegisterEvent_To_Conplete();
                //}
            }
        }


        
    }
}
