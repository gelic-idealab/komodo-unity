using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Komodo.Runtime
{
    [RequireComponent(typeof(Toggle))]
    public class VisibilityToggle : MonoBehaviour
    {
        private int index;

        private Toggle toggle;

        public GameObject visibleIcon;

        public GameObject invisibleIcon;

        public ModelItem modelItem;

        public void Toggle (bool doShow)
        {
            if (UIManager.IsAlive)
            {
                UIManager.Instance.ToggleModelVisibility(this.index, doShow);

                UIManager.Instance.SendVisibilityUpdate(this.index, doShow);
            }

            SelectOrDeselect(doShow);

            UpdateUI(doShow);
        }

        public void UpdateUI (bool doShow)
        {
            visibleIcon.SetActive(doShow);

            invisibleIcon.SetActive(!doShow);
        }

        public void SelectOrDeselect(bool doSelect)
        {
            //TODO(Brandon): what does EventSystem do here?

            if (doSelect)
            {
                EventSystem.current.SetSelectedGameObject(modelItem.gameObject);

                return;
            }

            EventSystem.current.SetSelectedGameObject(null);
        }

        public void Initialize (int index)
        {
            if (!modelItem)
            {
                throw new UnassignedReferenceException("ModelItem on object with VisibilityToggle script on it");
            }

            toggle = GetComponent<Toggle>();

            if (visibleIcon == null || invisibleIcon == null)
            {
                throw new UnassignedReferenceException("visibleIcon or invisibleIcon on VisibilityToggle component");
            }

            toggle.onValueChanged.AddListener((bool doShow) =>
            {
                Toggle(doShow);
            });

            this.index = index;

            Toggle(false);
        }

        public void ProcessNetworkToggle (bool doShow)
        {
            ProcessNetworkToggle(doShow, this.index);
        }

        public void ProcessNetworkToggle (bool doShow, int index)
        {
            if (UIManager.IsAlive)
            {
                UIManager.Instance.ToggleModelVisibility(index, doShow);
            }

            SelectOrDeselect(doShow);

            UpdateUI(doShow);

            toggle.isOn = doShow;
        }
    }
}
