using Komodo.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    public class PlayerReferences : MonoBehaviour
    {
        [Header("Hand References")]
        public Transform handL;
        public Transform handR;

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

        [ContextMenu("Set Left-Handed Menu")]
        public void SetLeftHandMenu()
        {
            UIManager.Instance.SetRightHandedMenu(); UIManager.Instance.ToggleMenuVisibility(true);

            //switch event inputs if switching hands so the cursor can reapear with the alternate hand
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.xrStandaloneInput.RegisterInputSource(triggerEventInputSourceR, true);
                EventSystemManager.Instance.RemoveInputSourceWithoutClick(triggerEventInputSourceL);
                //EventSystemManager.Instance.xrStandaloneInput.RemoveInputSource(triggerEventInputSourceR);
            }
        }

        [ContextMenu("Set Right-Handed Menu")]
        public void SetRightHandMenu()
        {
            UIManager.Instance.SetLeftHandedMenu(); UIManager.Instance.ToggleMenuVisibility(true);

            //switch event inputs if switching hands so the cursor can reapear with the alternate hand
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.xrStandaloneInput.RegisterInputSource(triggerEventInputSourceL, true);
                EventSystemManager.Instance.RemoveInputSourceWithoutClick(triggerEventInputSourceR);//RemoveInputSource(triggerEventInputSourceL);
            }
        }
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
                LeftHandSwitchMenuAction.onFirstClick.AddListener(() => { UIManager.Instance.SetRightHandedMenu(); UIManager.Instance.ToggleMenuVisibility(true);

                    //switch event inputs if switching hands so the cursor can reapear with the alternate hand
                    if (EventSystemManager.IsAlive)
                    {
                        EventSystemManager.Instance.xrStandaloneInput.RegisterInputSource(triggerEventInputSourceR, true);
                        EventSystemManager.Instance.RemoveInputSourceWithoutClick(triggerEventInputSourceL);
                    }

                });
                LeftHandSwitchMenuAction.onSecondClick.AddListener(() => { UIManager.Instance.SetRightHandedMenu(); UIManager.Instance.ToggleMenuVisibility(false);
                });


                RightHandSwitchMenuAction.onFirstClick.AddListener(() => { UIManager.Instance.SetLeftHandedMenu(); UIManager.Instance.ToggleMenuVisibility(true);

                    //switch event inputs if switching hands so the cursor can reapear with the alternate hand
                    if (EventSystemManager.IsAlive)
                    {
                        EventSystemManager.Instance.xrStandaloneInput.RegisterInputSource(triggerEventInputSourceL, true);
                        EventSystemManager.Instance.RemoveInputSourceWithoutClick(triggerEventInputSourceR);
                    }


                });
                RightHandSwitchMenuAction.onSecondClick.AddListener(() => { UIManager.Instance.SetLeftHandedMenu(); UIManager.Instance.ToggleMenuVisibility(false); });
            }
        }
    }
}
