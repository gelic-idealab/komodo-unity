using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Komodo.Runtime;

namespace Komodo.Runtime
{
    public class EraseTool : MonoBehaviour
    {   
        /// <summary>
        /// Player reference. This is assigned in the inspector through drag and drop.
        /// </summary>
        public PlayerReferences playerRefs;

        private UnityAction _enable;

        private UnityAction _disable;

        /// <summary>
        /// Checks if the playerRefs is null. This function is called when the script is loaded or a value is changed in the inspector. 
        /// </summary>
        /// <exception cref="UnassignedReferenceException">Thrwon when playerRefs is null.</exception>
        void OnValidate ()
        {
            if (playerRefs == null)
            {
                throw new UnassignedReferenceException("playerRefs");
            }
        }

        /// <summary>
        /// Adds listeners to KomodoEvenManager for eraseTool, either enable or disable.
        /// </summary>
        void Start ()
        {
            _enable += Enable;

            KomodoEventManager.StartListening("eraseTool.enable", _enable);

            _disable += Disable;

            KomodoEventManager.StartListening("eraseTool.disable", _disable);
        }

        /// <summary>
        /// activate eraser models???? I don't understand the purpose of this part.
        /// </summary>
        [ContextMenu("Test EraseTool: Start Erasing")]
        public void TestEraseToolStart ()
        {
            playerRefs.handLSelector.gameObject.SetActive(true);

            Enable();
        }

        /// <summary>
        /// activate eraser models???? I don't understand the purpose of this part.
        /// </summary>
        [ContextMenu("Test EraseTool: Stop Erasing")]
        public void TestEraseToolStop ()
        {
            Disable();

            playerRefs.handLSelector.gameObject.SetActive(false);
        }

        // Our own function. Not to be confused with Unity's OnEnable.
        public void Enable ()
        {
            playerRefs.drawL.Set_DRAW_UPDATE(true);

            playerRefs.drawR.Set_DRAW_UPDATE(true);

            playerRefs.eraseL.gameObject.SetActive(true);

            playerRefs.eraseR.gameObject.SetActive(true);

            playerRefs.displayEraserL.SetActive(true);

            playerRefs.displayEraserR.SetActive(true);
        }

        // Our own function. Not to be confused with Unity's OnDisable.
        public void Disable ()
        {
            playerRefs.drawL.Set_DRAW_UPDATE(false);

            playerRefs.drawR.Set_DRAW_UPDATE(false);

            playerRefs.eraseL.gameObject.SetActive(false);

            playerRefs.eraseR.gameObject.SetActive(false);

            playerRefs.displayEraserL.SetActive(false);

            playerRefs.displayEraserR.SetActive(false);
        }
    }
}
