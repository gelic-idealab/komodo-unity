using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Komodo.Runtime;

namespace Komodo.Runtime
{
    public class EraseTool : MonoBehaviour
    {
        public PlayerReferences playerRefs;

        private UnityAction _enable;

        private UnityAction _disable;

        void OnValidate ()
        {
            if (playerRefs == null)
            {
                throw new UnassignedReferenceException("playerRefs");
            }
        }

        void Start ()
        {
            _enable += Enable;

            KomodoEventManager.StartListening("eraserTool.enable", _enable);

            _disable += Disable;

            KomodoEventManager.StartListening("eraserTool.disable", _disable);
        }


        [ContextMenu("Test EraseTool: Start Erasing")]
        public void TestEraseToolStart ()
        {
            playerRefs.handLSelector.gameObject.SetActive(true);

            Enable();
        }

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

            playerRefs.eraseL.gameObject.SetActive(true); playerRefs.eraseR.gameObject.SetActive(true);

            playerRefs.displayEraserL.SetActive(true); playerRefs.displayEraserR.SetActive(true);
        }

        // Our own function. Not to be confused with Unity's OnDisable.
        public void Disable ()
        {
            playerRefs.drawL.Set_DRAW_UPDATE(false);

            playerRefs.drawR.Set_DRAW_UPDATE(false);

            playerRefs.eraseL.gameObject.SetActive(false); playerRefs.eraseR.gameObject.SetActive(false);

            playerRefs.displayEraserL.SetActive(false); playerRefs.displayEraserR.SetActive(false);
        }
    }
}
