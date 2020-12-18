using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="State_List", menuName = "new_State_List", order = 0)]
public class State_List : ScriptableObject
{
    public List<Bool_StateCheck_SO> stateList;
  
}
