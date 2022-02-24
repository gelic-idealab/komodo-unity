using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Komodo.Runtime;

namespace Komodo.Runtime
{
    public class InstructorMenu : MonoBehaviour
    {
        [Tooltip("Hierarchy: InstructorOnlyMenu -> Record")]
        public Button recordButtons;
        
        [Tooltip("Hierarchy: InstructorOnlyMenu -> Record -> startCapture")]
        public GameObject startCapture;

        [Tooltip("Hierarchy: InstructorOnlyMenu -> Record -> stopCapture")]
        public GameObject stopCapture;


        void Awake ()
        {
            if (recordButtons == null) 
            {
                throw new UnassignedReferenceException("recordButtons");
            }
        }

        public void Start ()
        {

            recordButtons.onClick.AddListener(() => 
            {
                if (startCapture.activeSelf) 
                {
                    KomodoEventManager.TriggerEvent("capture.start");
                    startCapture.SetActive(false);
                    stopCapture.SetActive(true);

                } else if (!startCapture.activeSelf) {

                    KomodoEventManager.TriggerEvent("capture.stop");
                    startCapture.SetActive(true);
                    stopCapture.SetActive(false);
 
                } else {

                    return;

                }
            });
        }
    }
}
