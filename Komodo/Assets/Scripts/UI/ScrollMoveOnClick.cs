using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollMoveOnClick : MonoBehaviour
{
    public float perClickMove;
    public Scrollbar scrollbar;

    private float currentValue;
  
    
    public void MoveScroll()
    {

        scrollbar.value += perClickMove;



    }
}
