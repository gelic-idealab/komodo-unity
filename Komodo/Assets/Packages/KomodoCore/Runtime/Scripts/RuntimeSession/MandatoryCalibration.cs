using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


// TODO: maybe add more description of this file? 
/**
* MandatoryCalibration.cs acts as a subscriber for KomodoEventManager.cs. It will subscribe to 
*       events registered in KomodoEventManager. In this case, MandatoryCalibration controls 
*       the prompt that asks users to adjust height, when Komodo first runs. It will disable 
*       or enable the prompt whenever it gets called.
* 
*/
namespace Komodo.Runtime
{
    /// <summary>
    /// Class <c>MandatoryCalibration</c> contains a set of event listener for mandatory height 
    /// calibration. 
    /// </summary>
    ///
    public class MandatoryCalibration : MonoBehaviour
    {
        
        private UnityAction ShowCalibrationPromptListener; 
        private UnityAction HideCalibrationPromptListener; 
        private UnityAction TeleportationCountListener; 

        /// <summary> 
        /// <c>HeightCalibrationPrompt</c> refers to the game object 
        /// that this script is attached to.
        /// </summary>
        /// <see cref = "HeightCalibrationPrompt">
        public GameObject HeightCalibrationPrompt; //TODO (Jonathan): automatically assign variable by finding type
        private bool teleportedTwice = false;
        
        /// <summary> 
        /// <c>Start()</c> is called on the frame when a script 
        /// is enabled just before any of the Update methods are called the first time. In this case,
        /// it is used to check if <c>HeightCalibrationPrompt</c> is null.
        /// </summary>
        ///
        void Start() 
        {
            if (HeightCalibrationPrompt == null) 
            {
                Debug.LogError ("There is no game object assigned to HeightCalibrationMenu");
            }
        }

        void Awake() 
        {
            ShowCalibrationPromptListener = new UnityAction (ShowPrompt);
            HideCalibrationPromptListener = new UnityAction (HidePrompt);
            TeleportationCountListener = new UnityAction (IsTeleportedTwice);
        }

        void OnEnable() 
        {
            KomodoEventManager.StartListening("MandatoryHeightCalibration", ShowCalibrationPromptListener);
            KomodoEventManager.StartListening("FinishedHeightCalibration", HideCalibrationPromptListener);
            KomodoEventManager.StartListening("TeleportedTwice", TeleportationCountListener);
        }

        void OnDisable() 
        {
            KomodoEventManager.StopListening("MandatoryHeightCalibration", ShowCalibrationPromptListener);
            KomodoEventManager.StopListening("FinishedHeightCalibration", HideCalibrationPromptListener);
            KomodoEventManager.StopListening("TeleportedTwice", TeleportationCountListener);
        }

        void ShowPrompt() 
        {
            HeightCalibrationPrompt.SetActive(true); // set the height calibration prompt to visiable.
            
        }

        void HidePrompt() 
        {
            if (teleportedTwice == true) 
            {
                HeightCalibrationPrompt.SetActive(false); // set the height calibration prompt to invisiable.
            }
        }

        void IsTeleportedTwice() 
        {
            teleportedTwice = true;
        }
    }
}
