using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Komodo.Runtime;

namespace Komodo.Runtime
{
    public class KomodoMenu : MonoBehaviour
    {
        /** 
         * @breif The erase is icon in the create tab, and it is a Button type. Once the KomodoMenu script is attached to a game object, 
         * this variable need to be assigned through drag-and-drop.
         * Hierarchy in the scene: KomodoMenu -> Panels -> CreateMenu -> ButtonsHorizontalLayout -> Erase.
        */
       [Tooltip("Hierarchy: KomodoMenu -> Panels -> CreateMenu -> ButtonsHorizontalLayout -> Erase")]
        public TabButton eraseTab;


        /** 
         * @breif The undo icon is in the create tab, and it is a Button type. Once the KomodoMenu script is attached to a game object, 
         * this variable need to be assigned through drag-and-drop.
         * Hierarchy: KomodoMenu -> Panels -> CreateMenu -> ButtonsHorizontalLayout -> Erase
        */
        [Tooltip("Hierarchy: KomodoMenu -> Panels -> CreateMenu -> ButtonsHorizontalLayout -> Undo")]
        public Button undoButton;


        /** 
         * @breif The draw icon is in the create tab, and it is a Button type. Once the KomodoMenu script is attached to a game object, 
         * this variable need to be assigned through drag-and-drop.
         * Hierarchy: Hierarchy: KomodoMenu -> Panels -> CreateMenu -> VerticalLayoutGroup -> Tabs -> Draw Tab
        */
        [Tooltip("Hierarchy: KomodoMenu -> Panels -> CreateMenu -> VerticalLayoutGroup -> Tabs -> Draw Tab")]
        public TabButton drawTab;


        /** 
         * @breif The brush icon is in the draw panel, and it is a Button type. Once the KomodoMenu script is attached to a game object, 
         * this variable need to be assigned through drag-and-drop.
         * Hierarchy: KomodoMenu -> Panels -> CreateMenu -> VerticalLayoutGroup -> DrawVerticalLayout -> ShapesGridLayout-> Brush
        */
        [Tooltip("Hierarchy: KomodoMenu -> Panels -> CreateMenu -> VerticalLayoutGroup -> DrawVerticalLayout -> ShapesGridLayout-> Brush")]
        public Toggle brushToggle;


        /** 
         * @breif The leaveAndRejoinButton is at the People tab, and it is a Button type. Once the KomodoMenu script is attached to a game object, 
         * this variable need to be assigned through drag-and-drop.
         * Hierarchy: KomodoMenu -> Panels -> PeopleMenu -> leaveAndRejoinButton
        */
        [Tooltip("Hierarchy: KomodoMenu -> Panels -> PeopleMenu -> leaveAndRejoinButton")]
        public Button leaveAndRejoinButton;


        /**
         * @breif The closeConnectionAndRejoinButton is at the People tab, and it is a Button type. 
         * Once the KomodoMenu script is attached to a game object,this variable need to be assigned through drag-and-drop. 
         * Hierarchy: KomodoMenu -> Panels -> PeopleMenu -> closeConnectionAndRejoinButton
        */
        [Tooltip("Hierarchy: KomodoMenu -> Panels -> PeopleMenu -> closeConnectionAndRejoinButton")]
        public Button closeConnectionAndRejoinButton;
        

        /** 
         * @breif The settingsTab represents the settings tab from our menu, and it is a TabButton type. 
         * Once the KomodoMenu script is attached to a game object,this variable need to be assigned through drag-and-drop. 
         * Hierarchy: KomodoMenu -> Tabs -> Settings
        */
        [Tooltip("Hierarchy: KomodoMenu -> Tabs -> Settings")]
        public TabButton settingsTab;


        /** 
         * @breif The peopleTab represents the people tab from our menu, and it is a TabButton type. 
         * Once the KomodoMenu script is attached to a game object,this variable need to be assigned through drag-and-drop. 
         * Hierarchy: KomodoMenu -> Tabs -> People
        */
        [Tooltip("Hierarchy: KomodoMenu -> Tabs -> People")]
        public TabButton peopleTab;


        /** 
         * @breif The interactTab represents the interact tab from our menu, and it is a TabButton type. 
         * Once the KomodoMenu script is attached to a game object,this variable need to be assigned through drag-and-drop. 
         * Hierarchy: KomodoMenu -> Tabs -> Interact
        */
        [Tooltip("Hierarchy: KomodoMenu -> Tabs -> Interact")]
        public TabButton interactTab;


        /** 
         * @breif The createTab represents the create tab from our menu, and it is a TabButton type. The create tab, when running in
         * spectator/PC mode, will be disabled. 
         * Once the KomodoMenu script is attached to a game object,this variable need to be assigned through drag-and-drop. 
         * Hierarchy: KomodoMenu -> Tabs -> Create
        */
        [Tooltip("Hierarchy: KomodoMenu -> Tabs -> Create")]
        public TabButton createTab;


        /** 
         * @breif The instructorOnlyMenu represents the instructor menu from our desktop mode menu, and it is a GameObject type. 
         * Once the KomodoMenu script is attached to a game object,this variable need to be assigned through drag-and-drop. 
         * Hierarchy: KomodoMenu -> Panels -> InstructorOnlyMenu
        */
       [Tooltip("Hierarchy: KomodoMenu -> Panels -> InstructorOnlyMenu")]
        public GameObject instructorOnlyMenu;


        /** 
         * @breif The instructorMenuButton represents the instructor menu button in the settings tab from our desktop mode menu, 
         * and it is a Button type. 
         * Once the KomodoMenu script is attached to a game object,this variable need to be assigned through drag-and-drop. 
         * Hierarchy: KomodoMenu -> Panels -> SettingsMenu -> NotCalibrating -> InstructorMenuButton
        */
        [Tooltip("Hierarchy: KomodoMenu -> Panels -> SettingsMenu -> NotCalibrating -> InstructorMenuButton")]

        public Button instructorMenuButton;

        void OnValidate ()
        {
            if (eraseTab == null)
            {
                throw new UnassignedReferenceException("eraseButton");
            }

            if (!drawTab)
            {
                throw new UnassignedReferenceException("drawTab");
            }

            if (undoButton == null)
            {
                throw new UnassignedReferenceException("undoButton");
            }

            if (brushToggle == null)
            {
                throw new UnassignedReferenceException("brushToggle");
            }

            if (leaveAndRejoinButton == null)
            {
                throw new UnassignedReferenceException("closeConnectionAndRejoinButton");
            }

            if (closeConnectionAndRejoinButton == null)
            {
                throw new UnassignedReferenceException("closeConnectionAndRejoinButton");
            }
        }

        public void Start ()
        {
            CaptureManager.Initialize();
            
            eraseTab.onTabSelected.AddListener(() => 
            {
                KomodoEventManager.TriggerEvent("eraseTool.enable");
            });

            eraseTab.onTabDeselected.AddListener(() => 
            {
                KomodoEventManager.TriggerEvent("eraseTool.disable");
            });

            undoButton.onClick.AddListener(() =>
            {
                UndoRedoManager.Instance.Undo();
            });

            drawTab.onTabSelected.AddListener(() =>
            {
                KomodoEventManager.TriggerEvent("drawTool.enable");
            });

            drawTab.onTabDeselected.AddListener(() =>
            {
                KomodoEventManager.TriggerEvent("drawTool.disable");

                KomodoEventManager.TriggerEvent("primitiveTool.disable");
            });

            brushToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    KomodoEventManager.TriggerEvent("drawTool.enable");

                    KomodoEventManager.TriggerEvent("primitiveTool.disable");

                    // TODO(Brandon) - is this the best way to get out of the primitive creation mode?

                    return;
                }

                KomodoEventManager.TriggerEvent("drawTool.disable");
            });

            leaveAndRejoinButton.onClick.AddListener(() =>
            {
                KomodoEventManager.TriggerEvent("connection.leaveAndRejoin");
            });

            closeConnectionAndRejoinButton.onClick.AddListener(() =>
            {
                KomodoEventManager.TriggerEvent("connection.closeConnectionAndRejoin");
            });

            settingsTab.onTabSelected.AddListener(() => 
            {
                ClientSpawnManager.Instance.SendMenuInteractionsType((int)INTERACTIONS.SETTING_TAB);
            });

            peopleTab.onTabSelected.AddListener(() => 
            {
                ClientSpawnManager.Instance.SendMenuInteractionsType((int)INTERACTIONS.PEOPLE_TAB);
            });

            interactTab.onTabSelected.AddListener(() => 
            {
                ClientSpawnManager.Instance.SendMenuInteractionsType((int)INTERACTIONS.INTERACTION_TAB);
            });

            createTab.onTabSelected.AddListener(() => 
            {
                ClientSpawnManager.Instance.SendMenuInteractionsType((int)INTERACTIONS.CREATE_TAB);
            });

            instructorMenuButton.onClick.AddListener(() => 
            {
                if (instructorOnlyMenu.activeSelf) 
                {
                    instructorOnlyMenu.SetActive(false);
                } else {
                    instructorOnlyMenu.SetActive(true);
                }
            });
            
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
