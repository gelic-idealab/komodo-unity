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

        public Button recordButtons;
        public GameObject startCapture;


        void OnValidate ()
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
                    recordButtons.transform.Find("startCapture").gameObject.SetActive(false);
                    recordButtons.transform.Find("stopCapture").gameObject.SetActive(true);
                } else if (!startCapture.activeSelf) 
                {
                    KomodoEventManager.TriggerEvent("capture.stop");
                    recordButtons.transform.Find("startCapture").gameObject.SetActive(true);
                    recordButtons.transform.Find("stopCapture").gameObject.SetActive(false);
 
                } else {
                    return;
                }
            });
        }
    }
}
