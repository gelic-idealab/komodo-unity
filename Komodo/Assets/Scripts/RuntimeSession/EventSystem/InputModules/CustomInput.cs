using UnityEngine;
using UnityEngine.EventSystems;

namespace Komodo.Runtime
{
    /// <summary>
    /// Used to override behavior of Unity EventSystem to use our camera for UI interaction
    /// </summary>
    public class CustomInput : BaseInput
    {

        public Camera controllerCameraRay;
        StandaloneXRInputModule standaloneInputModule;


        protected override void Awake()
        {
            standaloneInputModule = GetComponent<StandaloneXRInputModule>();
            if (standaloneInputModule) standaloneInputModule.inputOverride = this;
        }

        public CustomInput(Camera evemtCamera) => controllerCameraRay = evemtCamera;

        public void Set_EventCamera(Camera eventCamera)
        {
            controllerCameraRay = eventCamera;
        }

        public override bool GetMouseButtonDown(int button)
        {
            return true;
        }

        public override Vector2 mouseScrollDelta => Vector2.one;

        public override bool mousePresent => true;

        public override Vector2 mousePosition => new Vector2(controllerCameraRay.pixelWidth / 2, controllerCameraRay.scaledPixelHeight / 2);

    }
}