using System;
using UnityEngine;

namespace Komodo.Runtime
{   
    /// <summary>
    /// This is the model item class for assets being imported to a Komodo scene. Its field contains index, visibility toggle, lock toggle, and name for the model.
    /// </summary>
    public class ModelItem : MonoBehaviour
    {
        /// <summary>
        /// Index of the model/asset.
        /// </summary>
        private int index;

        /// <summary>
        /// Visibility toggle.
        /// </summary>
        private VisibilityToggle visibilityToggle;
        
        /// <summary>
        /// Lock toggle.
        /// </summary>
        private LockToggle lockToggle;

        /// <summary>
        /// Name of the model/asset.
        /// </summary>
        private ModelNameDisplay nameDisplay;

        /// <summary>
        /// Initialize the model/asset with the given indexn and name. Then add visibility and lock toggles to <c>UIManager</c>.
        /// </summary>
        /// <param name="index">index for the asset/model</param>
        /// <param name="name">name for the asset/model</param>
        /// <exception cref="MissingReferenceException">if either <c>nameDisplay</c>, <c>visibilityToggle</c>, or <c>lockToggle</c> is null, throw out exceptions.</exception>
        public void Initialize (int index, String name)
        {
            nameDisplay = GetComponentInChildren<ModelNameDisplay>(true);

            if (!nameDisplay)
            {
                throw new MissingReferenceException("nameDisplay on ModelItem");
            }

            visibilityToggle = GetComponentInChildren<VisibilityToggle>(true);

            if (!visibilityToggle)
            {
                throw new MissingReferenceException("visibilityToggle on a child of ModelItem");
            }

            lockToggle = GetComponentInChildren<LockToggle>();

            if (!lockToggle)
            {
                throw new MissingReferenceException("lockToggle on a child of ModelItem");
            }

            this.index = index;

            visibilityToggle.Initialize(this.index);

            UIManager.Instance.modelVisibilityToggleList.Add(visibilityToggle);

            lockToggle.Initialize(this.index);

            UIManager.Instance.modelLockToggleList.Add(lockToggle);

            nameDisplay.Initialize(name);
        }
    }
}