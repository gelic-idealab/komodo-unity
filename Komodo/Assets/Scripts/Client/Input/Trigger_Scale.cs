using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Scale : Trigger_Base
{

    public float attenuation;
    public Vector3 initialPos;
    public Transform _posOfHandLaser;

    public Transform thisTransform;
    public string _interactableTag = "UIInteractable";

    InputSelection inputSelection;

    public float _magnitudeOfScaling = 3f;

    public float _minScale = 0.01f;

   // private LayerMask layerToIgnore = (1 << 5); //UI IGNORE
    public override void Start()
    {
        inputSelection = transform.parent.GetComponent<InputSelection>();
        _posOfHandLaser = transform.parent;
        //_interactableTag = inputSelection._tagToLookFor;
        thisTransform = transform;
    }

    public float initialOffset;
    public Vector3 initialScale;
    public GameObject currentGO;
    public override void OnTriggerEnter(Collider other)
    {
        //if ui skip
        if (other.gameObject.layer == 5)
            return;

        //THIS IS TO AVOID LOOSING CONNECTION (INITIALPOS) TO INITIAL OBJECT - DISABLED REMOVE THE CONNECTION
        if (other.gameObject != currentGO)
            currentGO = other.gameObject;
        else
            return;

        if (other.CompareTag(_interactableTag))
        {
           initialPos = thisTransform.position;
           initialOffset = Vector3.Distance(_posOfHandLaser.position, initialPos);
           initialScale = other.transform.localScale;

            //NETWORK REGISTER
            try
            {
                var currentInteractiveObject = other.GetComponent<Net_Register_GameObject>();

                NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                {
                    sourceEntity_id = ClientSpawnManager.Instance.mainPlayer_RootTransformData.entityID,//GetComponent<Entity_Container>().entity_data.entityID,
                    targetEntity_id = currentInteractiveObject.assetImportIndex,
                    interactionType = (int)INTERACTIONS.GRAB,
                });

                MainClientUpdater.Instance.PlaceInNetworkUpdateList(currentInteractiveObject);
                // currentInteractiveObject.entity_data.isCurrentlyGrabbed = true;
            }
            catch
            {
                Debug.LogWarning("Custom Warning: " + "Could not send Interaction : ");
            }
        }
    }

    public override void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 5)
            return;

        if (inputSelection.isOverObject)
        {
            attenuation = Vector3.Distance(_posOfHandLaser.position, initialPos) / initialOffset;
            // Debug.Log(attenuation - 1);
            //RELATIVE TO CURRENT INITIAL SCALE
            //other.transform.localScale = initialScale + (initialScale * ((attenuation - 1) * _magnitudeOfScaling ));
            //ABSOLUTE?
            if (Mathf.Infinity == attenuation)
                return;

            other.transform.localScale = initialScale + (Vector3.one * ((attenuation - 1) * _magnitudeOfScaling ));

            other.transform.localScale = new Vector3(Mathf.Max(_minScale, other.transform.localScale.x), Mathf.Max(_minScale, other.transform.localScale.y), Mathf.Max(_minScale, other.transform.localScale.z));
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        if (currentGO == null)
            return;

        try
        {
            //Net_Register_GameObject netRegisterObj = currentRigidBody.GetComponent<Net_Register_GameObject>();
            //#if !UNITY_EDITOR && UNITY_WEBGL
            var currentInteractiveObject = currentGO.GetComponent<Net_Register_GameObject>();

            NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
            {
                sourceEntity_id = ClientSpawnManager.Instance.mainPlayer_RootTransformData.entityID,
                targetEntity_id = currentInteractiveObject.assetImportIndex,
                interactionType = (int)INTERACTIONS.DROP,
            });


            MainClientUpdater.Instance.RemoveFromInNetworkUpdateList(currentInteractiveObject);
            //    currentInteractiveObject.entity_data.isCurrentlyGrabbed = false;


        }
        catch
        {
            Debug.LogWarning("Custom Warning: " + "Could not send Interaction : ");
        }
    }
    public override void OnDisable()
    {
        currentGO = null;
    }
}
