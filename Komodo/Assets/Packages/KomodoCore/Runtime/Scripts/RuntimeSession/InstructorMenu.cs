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
            CaptureManager.Initialize();
        }

        // As of Komodo v0.3.2, UIManager does not have a public IsRightHanded function, so we must make do with this workaround. Returns a MenuAnchor.Location value, including UNKNOWN if the parent is not a MenuAnchor.
        public MenuAnchor.Kind GetMenuLocation ()
        {
            if (transform.parent.TryGetComponent(out MenuAnchor anchor))
            {
                return anchor.kind;
            }

            return MenuAnchor.Kind.UNKNOWN;
        }

        public void OnDestroy() 
        {
            CaptureManager.Deinitialize();
        }
    }
}
