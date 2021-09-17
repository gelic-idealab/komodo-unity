using System;
using UnityEngine;

namespace Komodo.Runtime
{
    public class ModelItem : MonoBehaviour
    {
        private int index;

        private VisibilityToggle visibilityToggle;

        private LockToggle lockToggle;

        private ModelNameDisplay nameDisplay;

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