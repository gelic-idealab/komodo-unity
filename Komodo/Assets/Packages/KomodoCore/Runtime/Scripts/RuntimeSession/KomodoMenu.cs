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
        public TabButton eraseTab;

        public Button undoButton;

        public TabButton drawTab;

        public Toggle brushToggle;

        public Button leaveAndRejoinButton;

        public Button closeConnectionAndRejoinButton;

        public TabButton settingsTab;

        public TabButton peopleTab;

        public TabButton interactTab;

        public TabButton createTab;

        public GameObject instructorOnlyMenu;

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
