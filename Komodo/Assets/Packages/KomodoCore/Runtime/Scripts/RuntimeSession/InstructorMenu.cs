using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Komodo.Runtime;

namespace Komodo.Runtime
{
    /// <summary>
    /// The instructor menu in the Desktop mode; it contains start and stop capturing.
    /// </summary>
    public class InstructorMenu : MonoBehaviour
    {
        /// <summary>
        /// The record button in the instructor menu.
        /// </summary>
        [Tooltip("Hierarchy: InstructorOnlyMenu -> Record")]
        public Button recordButtons;
        
        /// <summary>
        /// Despite being a GameObject, it is the start capture button.
        /// </summary>
        [Tooltip("Hierarchy: InstructorOnlyMenu -> Record -> startCapture")]
        public GameObject startCapture;

        /// <summary>
        /// Despite being a GameObject, it is the stop capture button.
        /// </summary>
        [Tooltip("Hierarchy: InstructorOnlyMenu -> Record -> stopCapture")]
        public GameObject stopCapture;

        /// <summary>
        /// This is only called in editor, but it checks to see if <c>recordButtons</c> is null.
        /// </summary>
        /// <exception cref="UnassignedReferenceException"></exception>
        void OnValidate ()
        {
            if (recordButtons == null) 
            {
                throw new UnassignedReferenceException("recordButtons");
            }
        }

        /// <summary>
        /// Add listener when <c>recordButtons</c> is clicked. If <c>startCapture</c> is active in the scene, start capturing data. If not stop capturing data.
        /// </summary>
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
