using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    public class ToolPlacement : MonoBehaviour
    {
        public ToolAnchor leftHandedToolAnchor;

        public ToolAnchor rightHandedToolAnchor;

        public ToolAnchor screenToolAnchor;

        private ToolAnchor.Kind _currentKind;

        public void OnValidate ()
        {
            if (leftHandedToolAnchor == null)
            {
                throw new UnassignedReferenceException("leftHandedToolAnchor");
            }

            if (rightHandedToolAnchor == null)
            {
                throw new UnassignedReferenceException("rightHandedToolAnchor");
            }

            if (screenToolAnchor == null)
            {
                throw new UnassignedReferenceException("screenToolAnchor");
            }
        }

        public void Awake ()
        {
            _currentKind = ToolAnchor.Kind.UNKNOWN;
        }

        public void Start ()
        {
            KomodoEventManager.StartListening("tools.setRightHanded", () =>
            {
                SetCurrentToolAnchor(ToolAnchor.Kind.RIGHT_HANDED);
            });

            KomodoEventManager.StartListening("tools.setLeftHanded", () =>
            {
                SetCurrentToolAnchor(ToolAnchor.Kind.LEFT_HANDED);
            });

            KomodoEventManager.StartListening("tools.setScreen", () =>
            {
                SetCurrentToolAnchor(ToolAnchor.Kind.SCREEN);
            });
        }

        public void SetCurrentToolAnchor (ToolAnchor.Kind kind)
        {
            _currentKind = kind;
        }

        public ToolAnchor GetCurrentToolAnchor ()
        {
            if (_currentKind == ToolAnchor.Kind.LEFT_HANDED)
            {
                return leftHandedToolAnchor;
            }

            if (_currentKind == ToolAnchor.Kind.RIGHT_HANDED)
            {
                return rightHandedToolAnchor;
            }

            if (_currentKind == ToolAnchor.Kind.SCREEN)
            {
                return screenToolAnchor;
            }

            return null; // this covers the case for ToolAnchor.Kind.UNKNOWN
        }

        public Transform GetCurrentToolAnchorTransform ()
        {
            ToolAnchor anchor = GetCurrentToolAnchor();

            if (anchor == null)
            {
                return null;
            }

            return anchor.transform;
        }
    }
}
