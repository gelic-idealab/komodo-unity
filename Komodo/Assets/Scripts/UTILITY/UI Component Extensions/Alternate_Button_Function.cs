using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Alternate_Button_Function : MonoBehaviour
{
    public UnityEvent onFirstClick;
    public UnityEvent onSecondClick;

    [ShowOnly]  public bool isFirstClick;

    public void AlternateButtonFunctions()
    {
        if (!isFirstClick)
            onFirstClick.Invoke();
       
        else
            onSecondClick.Invoke();
        
        isFirstClick = !isFirstClick;
    }

    public void CallSecondActionIfFirstActionWasMade()
    {
        if(isFirstClick)
            onSecondClick.Invoke();
    }
}
