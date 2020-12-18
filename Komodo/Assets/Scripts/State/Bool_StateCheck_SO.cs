using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "State_Bool", menuName = "new_State_Bool", order = 0)]
public class Bool_StateCheck_SO : ScriptableObject
{
    public bool value;

    public void SetBoolValue(bool value)
    {
        this.value = value;
    }
}
