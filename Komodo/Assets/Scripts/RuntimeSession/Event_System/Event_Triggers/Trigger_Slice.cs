using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
public class Trigger_Slice : Trigger_Base
{

    public float attenuation;
    public Vector3 initialPos;
    public Transform _posOfHandLaser;

    public Transform thisTransform;
    public string _interactableTag = "UIInteractable";

    InputSelection inputSelection;

    public float _magnitudeOfSlice_Near = 3;

    public BoxCollider thisBoxCollider;
    public Vector3 backBountsOriginalPositions;
    // public float _minScale = 0.01f;
    public override void Start()
    {
        inputSelection = transform.parent.GetComponent<InputSelection>();
        _posOfHandLaser = inputSelection.transform;
       // _interactableTag = inputSelection._tagToLookFor;
        thisTransform = transform;
        initialParent = transform.parent;

        thisBoxCollider = transform.GetComponent<BoxCollider>();
       // backBountsOriginalPositions = thisBoxCollider;
        //for (int i = 0; i < collisionBackBounds.bounds.c; i++)
        //{
        //    backBountsOriginalPositions[]
        //}
       
    }


    public float initialOffset;

    // public Vector3 initialScale;
    public GameObject currentGO;
    public override void OnTriggerEnter(Collider other)
    {
        //if ui skip
        if (other.gameObject.layer == 5)
            return;

        if (other.CompareTag(_interactableTag))
        {
            //THIS IS TO AVOID LOOSING CONNECTION (INITIALPOS) TO INITIAL OBJECT - DISABLED REMOVE THE CONNECTION
            if (other.gameObject != currentGO)
            {
                currentGO = other.gameObject;
                originalObjectParent = other.transform.parent;
            }
            else
                return;

            //Vector3 tempSize = new Vector3(thisBoxCollider.size.x, thisBoxCollider.size.y, thisBoxCollider.size.z + initialOffset); 
            //Vector3 tempOrigin = new Vector3(thisBoxCollider.bounds.center.x, thisBoxCollider.center.y, thisBoxCollider.center.z + initialOffset/2);
            //thisBoxCollider.size = tempSize;
            //thisBoxCollider.center = tempOrigin;

          //  thisBoxCollider.size = tempSize;
            //collisionBackBounds.bounds.SetMinMax(_posOfHandLaser.position, thisTransform.position);
                
               // _posOfHandLaser.position;

            initialPos = thisTransform.position;
            initialOffset = Vector3.Distance(_posOfHandLaser.position, initialPos);

            initialOBJECToffset = Vector3.Distance(_posOfHandLaser.position, other.transform.position);
            //initialScale = other.transform.localScale;
            initialPosOffset = other.transform.position - _posOfHandLaser.position;

            //  other.transform.position = _posOfHandLaser.forward.normalized * initialOBJECToffset + _posOfHandLaser.position;

        }
    }
    public float initialOBJECToffset;
    public Transform initialParent;
    public Transform originalObjectParent;
    public Vector3 initialPosOffset;
    public void FixedUpdate()
    {
        Vector3 tempSize = new Vector3(1, 1,  Mathf.Min(-1, _magnitudeOfSlice_Near * -1 * Vector3.Distance(_posOfHandLaser.position, thisTransform.position)));
        thisTransform.localScale = tempSize;
        //    Vector3 tempSize = new Vector3(thisBoxCollider.size.x, thisBoxCollider.size.y, 20);// thisBoxCollider.size.z + initialOffset);
        //    Vector3 tempOrigin = new Vector3(thisBoxCollider.bounds.center.x, thisBoxCollider.center.y, thisBoxCollider.center.z + initialOffset / 2);
        //    //thisBoxCollider.size = Vector3.one;
        //    //thisBoxCollider.center = tempOrigin;

        //    this
        //    thisBoxCollider.size.Set(tempSize.x, tempSize.y, 20);
        //    thisBoxCollider.center.Set(tempOrigin.x, tempOrigin.y, tempOrigin.z);


    }
public override void OnTriggerStay(Collider other)
    {

        if (inputSelection.isOverObject)
        {
            attenuation = Vector3.Distance(_posOfHandLaser.position, initialPos) / initialOffset;
            //Debug.Log(attenuation - 1);
            //TAKE OUT THE ONE START AT ZERO CAN DETERMINE DIRECTION OF ROTATION FORWARD RIGHT BACK LEFT
            //Vector3 tempSize = new Vector3(1,1, -1 * Vector3.Distance(_posOfHandLaser.position, other.transform.position));
            //thisTransform.localScale = tempSize;
            //  initialOBJECToffset = initialOBJECToffset + (-1 * ((attenuation - 1) * _magnitudeOfMove));




            //  Vector3 currentObj = other.transform.position + (_posOfHandLaser.forward * (attenuation - 1));
            //  initialOBJECToffset = Vector3.Distance(_posOfHandLaser.position, other.transform.position);
            //   other.transform.position = _posOfHandLaser.forward.normalized * initialOBJECToffset + _posOfHandLaser.position;
        }
    }
    //public void Update()
    //{
    //    if (currentGO != null)
    //        currentGO.transform.position = _posOfHandLaser.forward.normalized * initialOBJECToffset + _posOfHandLaser.position;
    //}
    public override void OnTriggerExit(Collider other)
    {
        //if (currentGO != null)
        //    currentGO.transform.SetParent(originalObjectParent, true);
    }
    public override void OnDisable()
    {
        //if (currentGO != null)
        //{
        //    currentGO.transform.SetParent(originalObjectParent, true);
        currentGO = null;
        //}
    }
}
