using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[System.Serializable]
public class Output_NewVector3_UnityEvent : UnityEvent<Vector3> { }

public class Trigger_Teleport : Trigger_Base
{
    private bool isOverButton;
    private Button currenButton;


    private InputSelection inputSelection;
    public string FlagTagMask_ForButtonUI = "UIInteractable";

    private bool isFloorDetector;

    public Coord_UnityEvent onNewTransformUpdate;


    public override void Start()
    {
        inputSelection = transform.parent.GetComponent<InputSelection>();

    }


    //THIS IS WHERE FUNCTIONS ARE INVOKED (ON RELEASE OF TRIGGER BUTTON WHICH DEACTIVATES PARENT OBJECT
    public override void OnDisable()
    {
        if (inputSelection && inputSelection.newPlayerPos != null)
          // if (inputSelection.newPlayerPos != null)
            {
                onNewTransformUpdate.Invoke(new Position
                {
                    pos = (Vector3)inputSelection.newPlayerPos,

                });

            }

        

    }
}
