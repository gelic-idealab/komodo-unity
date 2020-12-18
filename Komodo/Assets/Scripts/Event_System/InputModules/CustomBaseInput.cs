using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Used to overide behavior of Unity EventSystem to use our camera for UI interaction
/// </summary>
public class CustomBaseInput : BaseInput
{
    
    public Camera controllerCameraRay;
    StandaloneInputModule_XR standaloneInputModule;

  

    protected override void Awake()
    {
        standaloneInputModule = GetComponent<StandaloneInputModule_XR>();
        if (standaloneInputModule) standaloneInputModule.inputOverride = this;
    }

    public CustomBaseInput(Camera evemtCamera) => controllerCameraRay = evemtCamera;
    
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
