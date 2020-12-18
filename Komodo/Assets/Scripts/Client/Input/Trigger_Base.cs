using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Trigger_Base : MonoBehaviour
{


    public virtual void Start()
    {

    }

    public virtual void OnTriggerEnter(Collider other)
    {
       
    }

    public virtual void OnTriggerStay(Collider other)
    {

    }

    public virtual void OnTriggerExit(Collider other)
    {
     
    }

    //THIS IS WHERE FUNCTIONS ARE INVOKED (ON RELEASE OF TRIGGER BUTTON WHICH DEACTIVATES PARENT OBJECT
    public virtual void OnDisable()
    {
     

        

    }
}
