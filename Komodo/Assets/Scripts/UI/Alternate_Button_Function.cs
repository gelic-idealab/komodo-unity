using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Alternate_Button_Function : MonoBehaviour
{

    public UnityEvent onFirstClick;
    public UnityEvent onSecondClick;

    private bool isFirstClick;

    public void AlternateButtonFunctions()
    {
        if (!isFirstClick)
        {
            onFirstClick.Invoke();
       
        }
        else
        {
            onSecondClick.Invoke();
        }
        
        isFirstClick = !isFirstClick;

    }
}
