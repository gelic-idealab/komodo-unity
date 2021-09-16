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
        }

        public void Start ()
        {
            eraseTab.onTabSelected.AddListener(() => 
            {
                KomodoEventManager.TriggerEvent("eraser.enable");
            });

            eraseTab.onTabDeselected.AddListener(() => 
            {
                KomodoEventManager.TriggerEvent("eraser.disable");
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
        }
    }
}
