using System.Collections.Generic;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;
/// <summary>
/// Funcions to move our avatar
/// </summary>
[RequireComponent(typeof(CameraOffset))]
public class TeleportPlayer : MonoBehaviour
{
    private Transform cameraRootTransform;

    //move desktopPlayer
    private Transform desktopCameraTransform;
    //move xrPlayer
    private Transform xrPlayer;

    public CameraOffset cameraOffset;

    [UnityEngine.Serialization.FormerlySerializedAs("lRToAdjustWidth")]
    public List<LineRenderer> lineRenderersToScaleWithPlayer;

    public void Awake()
    {
        if (!cameraRootTransform) 
            cameraRootTransform = GameObject.FindGameObjectWithTag("Player").transform;
        
        //Get xr player to change position
        if (!xrPlayer) 
            xrPlayer = GameObject.FindGameObjectWithTag("XRCamera").transform;
        
        if(!desktopCameraTransform)
            desktopCameraTransform = GameObject.FindGameObjectWithTag("DesktopCamera").transform;
    }
    float originalHeight;
    float originalFixedDeltaTime;
    public void Start()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
        originalHeight = cameraOffset.cameraYOffset;
        currentScale = 1;
    }

    /// <summary>
    ///  Used to update the position and rotation of our XR Player
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public void SetXRPlayerPositionAndLocalRotation(Vector3 pos, Quaternion rot)
    {
        xrPlayer.position = pos;
        xrPlayer.localRotation = rot;
    }
    public void SetXRPayerDesktopPlayerRotation(Quaternion rot)
    {
        xrPlayer.localRotation = rot;

//#if UNITY_EDITOR
        cameraRootTransform.localRotation = rot;
//#endif
    }

    public void SetPlayerPositionToHome()
    {
        var homePos =  (Vector3.up * cameraOffset.cameraYOffset); //SceneManagerExtensions.Instance.anchorPositionInNewScene.position +//defaultPlayerInitialHeight);

        desktopCameraTransform.position = homePos;//UIManager.Instance.anchorPositionInNewScene.position;//Vector3.up * defaultPlayerInitialHeight;

        UpdatePlayerPosition(new Position { pos =  homePos});
    }
    //used in vr
    public void UpdatePlayerPosition(Position newData)
    {
        //used in VR
        var finalPosition = newData.pos;
        finalPosition.y = newData.pos.y + cameraOffset.cameraYOffset;//defaultPlayerInitialHeight; //+ WebXR.WebXRManager.Instance.DefaultHeight;

//#if UNITY_EDITOR
        cameraRootTransform.position = finalPosition;
//#elif UNITY_WEBGL
        xrPlayer.position = finalPosition;
//#endif
      //  mainPlayer_RootTransformData.pos = finalPosition;
    }

    /// <summary>
    /// Update our XRAvatar according to the height a
    /// </summary>
    /// <param name="newHeight"></param>
    public void UpdatePlayerHeight(float newHeight)
    {
        var ratioScale = currentScale / 1;
        var offsetFix = ratioScale * newHeight;// 1.8f;

        cameraOffset.cameraYOffset = offsetFix;//(newHeight);// * currentScale);
    }

    
    private float currentScale;
    /// <summary>
    /// Scale our player and adjust the line rendering lines we are using with our player transform
    /// </summary>
    /// <param name="newScale">We can only set it at 0.35 since we get near cliping issues any further with 0.01 on the camera </param>
    public void UpdatePlayerScale(float newScale)
    {
        currentScale = newScale;
        var ratioScale = newScale / 1;
        var offsetFix = ratioScale * 1.8f;
     
        desktopCameraTransform.transform.localScale = Vector3.one * newScale;
        xrPlayer.transform.localScale = Vector3.one * newScale;

        cameraOffset.cameraYOffset = offsetFix;//newScale;

        
        //adjust the line renderers our player uses to be scalled accordingly
        foreach (var item in lineRenderersToScaleWithPlayer)
        {
            item.widthMultiplier = newScale;
        }

    }
}
