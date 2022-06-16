using Komodo.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{   
    /// <summary>
    /// This class allows users to perform various actions, such as switching menu to either left or right hand and call out menu, in the VR mode.
    /// </summary>
    //TODO add null checks to all public variables
    public class PlayerReferences : MonoBehaviour
    {
    
        [Header("Hand References")]
        [Tooltip("PlayerSet -> WebXRCameraSet -> PlayspaceAnchor -> Hands -> handL")]
        public Transform handL;
        
        [Tooltip("PlayerSet -> WebXRCameraSet -> PlayspaceAnchor -> Hands -> handR")]
        public Transform handR;

        [Tooltip("PlayerSet -> ... -> Hands -> handL -> Armature -> Palm -> F1cTeleportLeft")]
        public HandTeleporter handLTeleporter;

        [Tooltip("PlayerSet -> ... -> Hands -> handR -> Armature -> Palm -> F1cTeleportRight")]
        public HandTeleporter handRTeleporter;

        [Tooltip("PlayerSet -> ... -> Hands -> handL -> Armature -> Palm -> F1cSelectLeft")]
        public HandSelector handLSelector;

        [Tooltip("PlayerSet -> ... -> Hands -> handR -> Armature -> Palm -> F1cSelectRight")]
        public HandSelector handRSelector;

        [Header("DrawSystem References")]
        [Tooltip("PlayerSet -> ... -> Hands -> handL -> Armature -> Palm -> F1cSelectLeft -> DrawLeft")]
        public TriggerDraw drawL;

        [Tooltip("PlayerSet -> ... -> Hands -> handR -> Armature -> Palm -> F1cSelectRight -> DrawRight")]
        public TriggerDraw drawR;

        [Tooltip("PlayerSet -> ... -> Hands -> handL -> Armature -> Palm -> F1cSelectLeft -> EraseLeft")]
        public TriggerEraseDraw eraseL;

        [Tooltip("PlayerSet -> ... -> Hands -> handR -> Armature -> Palm -> F1cSelectRight -> EraseRight")]
        public TriggerEraseDraw eraseR;

        [Tooltip("PlayerSet -> ... -> Armature -> handL -> Palm -> LDisplayTools -> DisplayEraseLeft")] 
        public GameObject displayEraserL;

        [Tooltip("PlayerSet -> ... -> Armature -> handR -> Palm -> RDisplayTools -> DisplayEraseRight")]
        public GameObject displayEraserR;


        [Header("EventSystem References")]
        [Tooltip("PlayerSet -> ... -> handL -> Armature -> Palm -> F1cSelectLeft -> selectLeft")]
        public TriggerEventInputSource triggerEventInputSourceL;

        [Tooltip("PlayerSet -> ... -> handR -> Armature -> Palm -> F1cSelectRight -> selectRight")]
        public TriggerEventInputSource triggerEventInputSourceR;

        [Tooltip("PlayerSet")]
        public AvatarEntityGroup thisAvatarGroup;

        [Header("Canvas Button Funcionality")]

        [Tooltip("PlayerSet -> WebXRCameraSet -> PlayspaceAnchor -> Hands -> handL")]
        public Alternate_Button_Function LeftHandSwitchMenuAction;
        
        [Tooltip("PlayerSet -> WebXRCameraSet -> PlayspaceAnchor -> Hands -> handR")]
        public Alternate_Button_Function RightHandSwitchMenuAction;

        /// <summary>
        /// Switch menu to the left hand while maintaining cursor.
        /// </summary>
        [ContextMenu("Set Left-Handed Menu")]
        public void SetLeftHandedMenu()
        {
            UIManager.Instance.SetRightHandedMenu(); 
            UIManager.Instance.ToggleMenuVisibility(true);

            //switch event inputs if switching hands so the cursor can reapear with the alternate hand
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.xrStandaloneInput.RegisterInputSource(triggerEventInputSourceR, true);
                EventSystemManager.Instance.RemoveInputSourceWithoutClick(triggerEventInputSourceL);
            }
        }

        /// <summary>
        /// Switch menu to the right hand while maintaining cursor. 
        /// </summary>
        [ContextMenu("Set Right-Handed Menu")]
        public void SetRightHandedMenu()
        {
            UIManager.Instance.SetLeftHandedMenu(); 
            UIManager.Instance.ToggleMenuVisibility(true);

            //switch event inputs if switching hands so the cursor can reapear with the alternate hand
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.xrStandaloneInput.RegisterInputSource(triggerEventInputSourceL, true);
                EventSystemManager.Instance.RemoveInputSourceWithoutClick(triggerEventInputSourceR);
            }
        }

        /// <summary>
        /// Add listeners for checking if the user has pressed buttons to swtich UI menu.
        /// </summary>
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
                LeftHandSwitchMenuAction.onFirstClick.AddListener(() => {       
                    UIManager.Instance.SetRightHandedMenu(); 
                    UIManager.Instance.ToggleMenuVisibility(true);

                    //switch event inputs if switching hands so the cursor can reapear with the alternate hand
                    if (EventSystemManager.IsAlive)
                    {
                        EventSystemManager.Instance.xrStandaloneInput.RegisterInputSource(triggerEventInputSourceR, true);
                        EventSystemManager.Instance.RemoveInputSourceWithoutClick(triggerEventInputSourceL);
                    }
                });

                LeftHandSwitchMenuAction.onSecondClick.AddListener(() => { 
                    UIManager.Instance.SetRightHandedMenu(); 
                    UIManager.Instance.ToggleMenuVisibility(false);
                });

                RightHandSwitchMenuAction.onFirstClick.AddListener(() => { 
                    UIManager.Instance.SetLeftHandedMenu(); 
                    UIManager.Instance.ToggleMenuVisibility(true);

                    //switch event inputs if switching hands so the cursor can reapear with the alternate hand
                    if (EventSystemManager.IsAlive)
                    {
                        EventSystemManager.Instance.xrStandaloneInput.RegisterInputSource(triggerEventInputSourceL, true);
                        EventSystemManager.Instance.RemoveInputSourceWithoutClick(triggerEventInputSourceR);
                    }
                });

                RightHandSwitchMenuAction.onSecondClick.AddListener(() => { 
                    UIManager.Instance.SetLeftHandedMenu(); 
                    UIManager.Instance.ToggleMenuVisibility(false); 
                });
            }
        }
    }
}
