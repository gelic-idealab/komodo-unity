using UnityEngine;

/// <summary>
/// Funcions to move our avatar
/// </summary>
public class TeleportPlayer : MonoBehaviour
{
    //data container to offset hand movement for Webxr controller movement update 
    [HideInInspector] public Entity_Data mainPlayer_RootTransformData;
    //move desktopPlayer
    private Transform player;
    
    private Transform xrPlayer;
   // private Transform handsParent;

    public void Awake()
    {
        if (!player) {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        //Get xr player to change position
        if (!xrPlayer) {
            xrPlayer = GameObject.FindGameObjectWithTag("XRCamera").transform;
        }
        //Get xr player to change position
        //if (!handsParent) {
        //    handsParent = GameObject.FindGameObjectWithTag("Hands").transform;
        //}
    }
    public void Start()
    {
        //grab reference to our mainclientData
        mainPlayer_RootTransformData = NetworkUpdateHandler.Instance.mainEntityData;
    }

    //public void UpdateHandsParentPosition() {
    //    handsParent.SetPositionAndRotation(player.position, player.rotation);
    //}

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

#if UNITY_EDITOR
        player.localRotation = rot;
#endif
    }

    //used in vr
    public void UpdatePlayerPosition(Position newData)
    {
        //used in VR
        var finalPosition = newData.pos;
        finalPosition.y = newData.pos.y + WebXR.WebXRManager.Instance.DefaultHeight;

#if UNITY_EDITOR
        player.position = finalPosition;
#elif UNITY_WEBGL
        xrPlayer.position = finalPosition;
#endif

        mainPlayer_RootTransformData.pos = finalPosition;
    }

    /// <summary>
    /// Update our XRAvatar according to the height a
    /// </summary>
    /// <param name="newHeight"></param>
    public void UpdatePlayerHeight(float newHeight)
    {
        WebXR.WebXRManager.Instance.DefaultHeight = newHeight;
        //used in VR

        var finalPosition = Vector3.zero;
#if UNITY_EDITOR
        finalPosition = player.position;
        finalPosition.y = WebXR.WebXRManager.Instance.DefaultHeight;

        player.position = finalPosition;
#elif UNITY_WEBGL
        finalPosition = xrPlayer.position;
        finalPosition.y = WebXR.WebXRManager.Instance.DefaultHeight;

        xrPlayer.position = finalPosition;
#endif

        mainPlayer_RootTransformData.pos = finalPosition;
    }

}
