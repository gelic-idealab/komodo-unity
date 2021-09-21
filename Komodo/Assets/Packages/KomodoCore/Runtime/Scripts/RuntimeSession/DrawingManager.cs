using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Komodo.Runtime;

namespace Komodo.Runtime
{
    // TODO(Brandon) - rename this to BrushManager
    public class DrawingManager : DrawingInstanceManager
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

            KomodoEventManager.StartListening("drawTool.enable", _enable);

            _disable += Disable;

            KomodoEventManager.StartListening("drawTool.disable", _disable);
        }

        [ContextMenu("Test DrawTool: Start Drawing")]
        public void TestDrawToolStart ()
        {
            playerRefs.handLSelector.gameObject.SetActive(true);

            Enable();
        }

        [ContextMenu("Test DrawTool: Stop Drawing")]
        public void TestDrawToolStop ()
        {
            Disable();

            playerRefs.handLSelector.gameObject.SetActive(false);
        }

        // Our own function. Not to be confused with Unity's OnEnable.
        public void Enable ()
        {
            playerRefs.drawL.gameObject.SetActive(true);

            playerRefs.drawR.gameObject.SetActive(true);
        }

        // Our own function. Not to be confused with Unity's OnDisable.
        public void Disable ()
        {
            playerRefs.drawL.gameObject.SetActive(false);

            playerRefs.drawR.gameObject.SetActive(false);
        }
    }
}
