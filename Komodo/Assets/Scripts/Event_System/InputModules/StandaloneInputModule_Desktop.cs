using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StandaloneInputModule_Desktop : StandaloneInputModule
{
    public GameObject GetCurrentFocusedObject_Desktop()
    {
        return base.GetCurrentFocusedGameObject();
    }
}
