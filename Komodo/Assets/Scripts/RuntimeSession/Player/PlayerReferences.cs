using Komodo.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    public class PlayerReferences : MonoBehaviour
    {
        [Header("DrawSystem References")]
        public TriggerDraw drawL;
        public TriggerDraw drawR;
        public TriggerEraseDraw eraseL;
        public TriggerEraseDraw eraseR;
        public GameObject displayEraserL;
        public GameObject displayEraserR;

        [Header("EventSystem References")]
        public TriggerEventInputSource triggerEventInputSourceL;
        public TriggerEventInputSource triggerEventInputSourceR;

        public AvatarEntityGroup thisAvatarGroup;

        [Header("Canvas Button Funcionality")]
        public Alternate_Button_Function LeftHandSwitchMenuAction;
        public Alternate_Button_Function RightHandSwitchMenuAction;

        public void Start()
        {
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.inputSource_LeftHand = triggerEventInputSourceL;
                EventSystemManager.Instance.inputSource_RighttHand = triggerEventInputSourceR;
            }

            //Add to send updates to network
            if (MainClientUpdater.IsAlive)
            {
                MainClientUpdater.Instance.mainClientAvatarEntityGroup = thisAvatarGroup;

            }

            //set up funcions to turn on and switch our UI
            if (UIManager.IsAlive)
            {
                LeftHandSwitchMenuAction.onFirstClick.AddListener(() => { UIManager.Instance.SetRightHandedMenu(); UIManager.Instance.ToggleMenuVisibility(true); });
                LeftHandSwitchMenuAction.onSecondClick.AddListener(() => { UIManager.Instance.SetRightHandedMenu(); UIManager.Instance.ToggleMenuVisibility(false); });

                RightHandSwitchMenuAction.onFirstClick.AddListener(() => { UIManager.Instance.SetLeftHandedMenu(); UIManager.Instance.ToggleMenuVisibility(true); });
                RightHandSwitchMenuAction.onSecondClick.AddListener(() => { UIManager.Instance.SetLeftHandedMenu(); UIManager.Instance.ToggleMenuVisibility(false); });
            }
        }
    }
}
