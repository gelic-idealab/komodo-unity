using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    public class MandatoryCalibrationTrigger : MonoBehaviour
    {
        void Start() 
        {
                KomodoEventManager.TriggerEvent("MandatoryHeightCalibration");
        }

    }
}
