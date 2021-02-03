using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLookAt : MonoBehaviour//,IUpdatable
{

    //not in main updatable because I want it to be shut off when not in view and not on;

    
    public Transform lookAtTarget;
    public Transform thisTransform;
    void Start()
    {
        thisTransform = transform;

        if (lookAtTarget == null)
            lookAtTarget = GameObject.FindGameObjectWithTag("XRCamera").transform;
    }
    public void Update()
    {
        thisTransform.LookAt(lookAtTarget, Vector3.up);
    }

    ////public void OnUpdate(float realTime)
    ////{
    ////    throw new System.NotImplementedException();
    ////}

    //// Start is called before the first frame update


    //// Update is called once per frame
    //void Update()
    //{

    //}
}
