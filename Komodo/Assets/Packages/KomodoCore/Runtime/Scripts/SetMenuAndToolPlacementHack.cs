using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

namespace Komodo.Runtime
{
    // We need to manually fire an event whenever the handedness changes.
    // This could break if the implementation of PlayerReferences changes,
    // so this is in a "Komodo v0.3.2 fix" file.
    public class SetMenuAndToolPlacementHack : MonoBehaviour
    {
        public PlayerReferences playerRefs;

        private UIManager _manager;

        private bool _wasInVRLastFrame;

        public void Awake ()
        {
            if (!playerRefs)
            {
                throw new UnassignedReferenceException("playerRefs");
            }
        }

        public void Start ()
        {
            if (!UIManager.IsAlive)
            {
                Debug.LogWarning("There is no UIManager in the scene, so SetHandednessEvent may not work properly.");
            }

            _manager = UIManager.Instance;

            playerRefs.LeftHandSwitchMenuAction.onFirstClick.AddListener(() =>
            {
                KomodoEventManager.TriggerEvent("menu.setLeftHanded");

                KomodoEventManager.TriggerEvent("tools.setRightHanded");
            });

            playerRefs.RightHandSwitchMenuAction.onFirstClick.AddListener(() =>
            {
                KomodoEventManager.TriggerEvent("menu.setRightHanded");

                KomodoEventManager.TriggerEvent("tools.setLeftHanded");
            });

            KomodoEventManager.TriggerEvent("menu.setScreen");

            KomodoEventManager.TriggerEvent("tools.setScreen");

            _wasInVRLastFrame = false;
        }

        public void Update ()
        {
            // This is a hack! Until ToggleMenuDisplayMode is updated to allow 
            // callbacks for the functions SetVRViewPort and 
            // SetDesktopViewport, we must continually check to see which 
            // rendering mode the menu is in to infer whether we're in VR or on 
            // a Screen. Otherwise, we must reference the WebXR assembly 
            // directly. 

            bool isInVRThisFrame = _manager.menuCanvas.renderMode == RenderMode.WorldSpace;

            if (_wasInVRLastFrame && !isInVRThisFrame)
            {
                // Switch to Screen mode.

                KomodoEventManager.TriggerEvent("menu.setScreen");

                KomodoEventManager.TriggerEvent("tools.setScreen");

                _wasInVRLastFrame = false;
            }

            if (!_wasInVRLastFrame && isInVRThisFrame)
            {
                // Switch to VR mode.

                KomodoEventManager.TriggerEvent("menu.setLeftHanded");

                KomodoEventManager.TriggerEvent("tools.setRightHanded");

                _wasInVRLastFrame = true;
            }
        }
    }
}