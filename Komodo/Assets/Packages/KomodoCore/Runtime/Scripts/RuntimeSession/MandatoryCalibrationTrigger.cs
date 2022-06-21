using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{   
    /// <summary>
    /// This class contains a function that triggers Mandatory Height Calibration. However, I think this class is uncessary at this point. It requires further inspection though.
    /// </summary>
    public class MandatoryCalibrationTrigger : MonoBehaviour
    {   
        /// <summary>
        /// Trigger Mandatory Height Calibration event. 
        /// </summary>
        void Start() 
        {
                KomodoEventManager.TriggerEvent("MandatoryHeightCalibration");
        }

    }
}
