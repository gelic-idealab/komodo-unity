using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    public class MenuPlacement : MonoBehaviour
    {
        public MenuAnchor leftHandedMenuAnchor;

        public MenuAnchor rightHandedMenuAnchor;

        public MenuAnchor screenMenuAnchor;

        private MenuAnchor.Kind _currentKind;

        public void Start ()
        {
            if (leftHandedMenuAnchor == null)
            {
                throw new UnassignedReferenceException("leftHandedMenuAnchor");
            }

            if (rightHandedMenuAnchor == null)
            {
                throw new UnassignedReferenceException("rightHandedMenuAnchor");
            }

            if (screenMenuAnchor == null)
            {
                throw new UnassignedReferenceException("screenMenuAnchor");
            }

            _currentKind = MenuAnchor.Kind.UNKNOWN;

            KomodoEventManager.StartListening("menu.setRightHanded", () =>
            {
                SetCurrentMenuAnchor(MenuAnchor.Kind.RIGHT_HANDED);
            });

            KomodoEventManager.StartListening("menu.setLeftHanded", () =>
            {
                SetCurrentMenuAnchor(MenuAnchor.Kind.LEFT_HANDED);
            });

            KomodoEventManager.StartListening("menu.setScreen", () =>
            {
                SetCurrentMenuAnchor(MenuAnchor.Kind.SCREEN);
            });
        }

        public void SetCurrentMenuAnchor (MenuAnchor.Kind kind)
        {
            _currentKind = kind;
        }

        public MenuAnchor GetCurrentMenuAnchor ()
        {
            if (_currentKind == MenuAnchor.Kind.LEFT_HANDED)
            {
                return leftHandedMenuAnchor;
            }

            if (_currentKind == MenuAnchor.Kind.RIGHT_HANDED)
            {
                return rightHandedMenuAnchor;
            }

            if (_currentKind == MenuAnchor.Kind.SCREEN)
            {
                return screenMenuAnchor;
            }

            return null; // this covers the case for MenuAnchor.Kind.UNKNOWN
        }

        public Transform GetCurrentMenuAnchorTransform ()
        {
            MenuAnchor anchor = GetCurrentMenuAnchor();

            if (anchor == null)
            {
                return null;
            }

            return anchor.transform;
        }
    }
}
