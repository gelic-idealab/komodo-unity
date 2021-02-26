// Gaze Input Module by Peter Koch <peterept@gmail.com>
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// To use:
// 1. Drag onto your EventSystem game object.
// 2. Disable any other Input Modules (eg: StandaloneInputModule & TouchInputModule) as they will fight over selections.
// 3. Make sure your Canvas is in world space and has a GraphicRaycaster.
// 4. If you have multiple cameras then make sure to drag your VR (center eye) camera into the canvas.
public class GazeInputModule : PointerInputModule 
{
	private PointerEventData pointerEventData;
	private GameObject currentLookAtHandler;
	private float currentLookAtHandlerClickTime;
    public Camera cameraToUseForGazeEvents;

	public override void Process()
	{ 
		HandleLook();
	}
	void HandleLook()
	{
       
		if (pointerEventData == null)
		{
			pointerEventData = new PointerEventData(eventSystem);
		}
		// fake a pointer always being at the center of the screen
#if UNITY_EDITOR
		pointerEventData.position = new Vector2(cameraToUseForGazeEvents.pixelWidth / 2, cameraToUseForGazeEvents.pixelHeight / 2);//new Vector2(Screen.width/2, Screen.height/2);
#elif UNITY_ANDROID
		pointerEventData.position = new Vector2 (XRSettings.eyeTextureWidth/2, XRSettings.eyeTextureHeight/2);
#endif
        pointerEventData.delta = Vector2.zero;

		List<RaycastResult> raycastResults = new List<RaycastResult>();
		eventSystem.RaycastAll(pointerEventData, raycastResults);
		pointerEventData.pointerCurrentRaycast = FindFirstRaycast(raycastResults);

        HandlePointerExitAndEnter(pointerEventData, pointerEventData.pointerCurrentRaycast.gameObject);
        //GameObject handler = ExecuteEvents.GetEventHandler<IPointerEnterHandler>(pointerEventData.pointerCurrentRaycast.gameObject);

        //if (currentLookAtHandler != handler)
        //{
        //    currentLookAtHandler = handler;
        //    ExecuteEvents.Execute(handler, pointerEventData, ExecuteEvents.pointerEnterHandler);
    }


}