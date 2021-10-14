using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


// TODO: add more description of this file. 
// hold reference of the game object, that is the game panel/canvas that prompts users to calibrate the height.
namespace Komodo.Runtime
{
    public class MandatoryCalibration : MonoBehaviour
    {
        private UnityAction ShowCalibrationPromptListener;
        private UnityAction HideCalibrationPromptListener;
        public GameObject HeightCalibrationPrompt; //TODO (Jonathan): automatically assign variable by finding type

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
        }
        void OnEnable() 
        {
            KomodoEventManager.StartListening("MandatoryHeightCalibration", ShowCalibrationPromptListener);
            KomodoEventManager.StartListening("FinishedHeightCalibration", HideCalibrationPromptListener);
        }
        void OnDisable() 
        {
            KomodoEventManager.StopListening("MandatoryHeightCalibration", ShowCalibrationPromptListener);
            KomodoEventManager.StopListening("FinishedHeightCalibration", HideCalibrationPromptListener);
        }
        void ShowPrompt() 
        {
            HeightCalibrationPrompt.SetActive(true);
            //This will call some methods from HeightCalibration.cs. It will do the calibration work.
        }
        void HidePrompt() 
        {
            HeightCalibrationPrompt.SetActive(false);
            Debug.Log("You have successfully calibrated the height.");
        }
    }
}
