using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    public class KomodoMenuFixes : MonoBehaviour
    {
        public Button reconnectButton;

        void OnValidate ()
        {
            if (reconnectButton == null)
            {
                throw new UnassignedReferenceException("reconnectButton");
            }
        }

        public void Start ()
        {
            reconnectButton.onClick.AddListener(() =>
            {
                KomodoEventManager.TriggerEvent("network.reconnect");
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
    }
}
