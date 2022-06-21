using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    [RequireComponent(typeof(Toggle))]
    public class VisibilityToggle : MonoBehaviour, IPointerClickHandler
    {   
        /// <summary>
        /// Indexes for the models.
        /// </summary>
        private int index;

        /// <summary>
        /// Toggle functionality in the model sections in Komodo UI menu.
        /// </summary>
        private Toggle toggle;

        /// <summary>
        /// The game object icon for visible toggle.
        /// </summary>
        public GameObject visibleIcon;

        /// <summary>
        /// The game object icon for invisible toggle.
        /// </summary>
        public GameObject invisibleIcon;

        /// <summary>
        /// 
        /// </summary>
        public ModelItem modelItem;

        /// <summary>
        /// Toggle models' visibility based on the the parameter. Update the UI.
        /// If the <c>UIManager</c> is active, show the model and update the visibility to the network.
        /// </summary>
        /// <param name="doShow">true for showing the model; false for hiding the model</param>
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

        /// <summary>
        /// Change the visibility icons in the Komodo UI menu when users toggle a model. 
        /// </summary>
        /// <param name="doShow">true for showing the visible icon and false for showing the invisible icon.</param>
        public void UpdateUI (bool doShow)
        {
            visibleIcon.SetActive(doShow);

            invisibleIcon.SetActive(!doShow);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doSelect"></param>
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

        /// <summary>
        /// Initialize the models as well as the toggle part in Komodo's UI with <c>Toogle()</c>.
        /// </summary>
        /// <param name="index">the index of the model.</param>
        /// <exception cref="UnassignedReferenceException">thrown if modelItem does not have VisibilityToggle script on it or if visibleIcon/invisibleIcon is null.</exception>
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

            this.index = index;

            Toggle(false);
        }

        /// <summary>
        /// This is a Unity callback function. It toggles the visibility of models and icons of the UI based on clicking.
        /// </summary>
        /// <param name="data"> Data passed in (typically by the event system)</param>
        public void OnPointerClick (PointerEventData data)
        {
            Toggle(this.toggle.isOn); // The value of toggle should be changed by the time this event handler fires, so we should be able to use its updated value here.
        }

        /// <summary>
        /// This calls <c>ProcessNetworkToggle(bool doShow, int index)</c>.
        /// </summary>
        /// <param name="doShow"></param>
        public void ProcessNetworkToggle (bool doShow)
        {
            ProcessNetworkToggle(doShow, this.index);
        }

        /// <summary>
        /// This is a helper function for <c>ProcessNetworkToggle(bool doShow)</c>. What it does is similary to <c>Toggle()</c>, except this method is used for handling network toggle (which is located in <c>UIManager.cs</c>).
        /// </summary>
        /// <param name="doShow">boolean value for deciding whether to show or not</param>
        /// <param name="index">integer representation for models</param>
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
