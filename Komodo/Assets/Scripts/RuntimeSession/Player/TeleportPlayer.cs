using System.Collections.Generic;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;

namespace Komodo.Runtime
{
    /// <summary>
    /// Funcions to move our avatar
    /// </summary>
    [RequireComponent(typeof(CameraOffset))]
    public class TeleportPlayer : MonoBehaviour
    {
        public bool useManualHeightOffset = false;

        public string playerSetTag = "Player";

        public string cameraSetTag = "CameraSet";

        public string leftEyeTag = "LeftEye";

        public string rightEyeTag = "RightEye";

        public string playspaceTag = "XRCamera";

        public string spectatorCameraTag = "DesktopCamera";

        public string playerSpawnCenterTag = "PlayerSpawnCenter";

        private Transform playerSet;

        private Transform cameraSet;

        private Transform spectatorCamera;

        private Transform playspace;

        private Transform rightEye;

        private Transform leftEye;

        private Transform centerEye;

        private Transform currentSpawnCenter;

        private bool justBumped = false;

        public CameraOffset cameraOffset;

        [UnityEngine.Serialization.FormerlySerializedAs("lRToAdjustWidth")]
        public List<LineRenderer> lineRenderersToScaleWithPlayer;

        float originalHeight;

        public float manualYOffset = 0.0f;

        float originalFixedDeltaTime;


        public void Start()
        {
            originalFixedDeltaTime = Time.fixedDeltaTime;

            originalHeight = cameraOffset.cameraYOffset;

            currentScale = 1;

            SetPlayerSpawnCenter();
        }
        
        public void Awake()
        {
            if (!playerSet) 
            {
                //get child to transform, we keep the webxrcameraset at origin
                playerSet = GameObject.FindGameObjectWithTag(playerSetTag).transform.GetChild(0);
            }

            if (!cameraSet) 
            {
                cameraSet = GameObject.FindGameObjectWithTag(cameraSetTag).transform;
            }
            
            if (!playspace) 
            {
                playspace = GameObject.FindGameObjectWithTag(playspaceTag).transform;
            }

            if (!leftEye)
            {
                leftEye = GameObject.FindGameObjectWithTag(leftEyeTag).transform;
            }

            if (!rightEye)
            {
                rightEye = GameObject.FindGameObjectWithTag(rightEyeTag).transform;
            }

            if (!centerEye)
            {
                centerEye = leftEye;
            }
            
            if (!spectatorCamera)
            {
                spectatorCamera = GameObject.FindGameObjectWithTag(spectatorCameraTag).transform;
            }
        }

        public Transform GetXRPlayer () 
        {
            return playspace;
        }

        /**
        * Finds a gameObject whose Transform represents the center of the circle
        * where players may spawn.  Use this, for example, on each scene load 
        * to set the new additive scene's spawn center correctly.
        * 
        * Importantly, this will help set the y-height 
        * of the floor for an arbitrary scene. To use this, in each additive 
        * scene, create an empty, place it at the floor of the environment 
        * where you want players to spawn, and tag the empty with 
        * <playerSpawnCenterTag>.
        */
        public void SetPlayerSpawnCenter ()
        {
            const string generatedSpawnCenterName = "PlayerSpawnCenter";

            var spawnCentersFound = GameObject.FindGameObjectsWithTag(playerSpawnCenterTag);

            // If we found gameObjects with the right tag,
            // pick the first one that's different from the current one

            for (int i = 0; i < spawnCentersFound.Length; i += 1) {

                if (spawnCentersFound[i] != currentSpawnCenter.gameObject) {

                    //Debug.Log($"[PlayerSpawnCenter] New center found: {spawnCentersFound[i].name}");

                    currentSpawnCenter = spawnCentersFound[i].transform;

                    return;
                }
            }

            // If we didn't find any new gameObjects with the right tag,
            // and there's no existing one, make a new one with default settings

            if (spawnCentersFound.Length == 0 && currentSpawnCenter == null) {
                //Debug.LogWarning($"[PlayerSpawnCenter] No GameObjects with tag {playerSpawnCenterTag} were found. Generating one with position <0, 0, 0>.");

                var generatedSpawnCenter = new GameObject(generatedSpawnCenterName);

                generatedSpawnCenter.tag = playerSpawnCenterTag;

                generatedSpawnCenter.transform.SetParent(null);

                currentSpawnCenter = generatedSpawnCenter.transform;

                return;
            }

            // If no gameObjects with the right tag were found, and there is an 
            // existing one, use the existing one. 

            //Debug.Log($"[PlayerSpawnCenter] Using existing Player Spawn Center: {currentSpawnCenter.gameObject.name}");
        }

        /// <summary>
        ///  Used to update the position and rotation of our XR Player
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        public void SetXRPlayerPositionAndLocalRotation(Vector3 pos, Quaternion rot)
        {
            playspace.position = pos;

            playspace.localRotation = rot;
        }
        
        public void SetXRAndSpectatorRotation(Quaternion rot)
        {
            playspace.localRotation = rot;

            playerSet.localRotation = rot;
        }

        public void SnapTurnLeft (float degrees)
        {
            UpdateCenterEye();

            playspace.RotateAround(centerEye.position, Vector3.up, degrees);
        }

        public void SnapTurnRight (float degrees)
        {
            UpdateCenterEye();

            playspace.RotateAround(centerEye.position, Vector3.up, -degrees);
        }

        public void SetPlayerPositionToHome()
        {
            var homePos = (Vector3.up * cameraOffset.cameraYOffset); //SceneManagerExtensions.Instance.anchorPositionInNewScene.position +//defaultPlayerInitialHeight);

            spectatorCamera.position = homePos;//UIManager.Instance.anchorPositionInNewScene.position;//Vector3.up * defaultPlayerInitialHeight;

            UpdatePlayerPosition(new Position { pos = homePos });
        }

        public void SetPlayerPositionToHome2 () 
        {
            var homePosition = currentSpawnCenter.position;

            spectatorCamera.position = homePosition;

            UpdatePlayerPosition2(new Position { pos = homePosition });
        }
        
        public void UpdatePlayerPosition(Position newData)
        {
            //used in VR
            var finalPosition = newData.pos;
            finalPosition.y = newData.pos.y + cameraOffset.cameraYOffset;//defaultPlayerInitialHeight; //+ WebXR.WebXRManager.Instance.DefaultHeight;

//#if UNITY_EDITOR
            playerSet.position = finalPosition;
//#elif UNITY_WEBGL
            playspace.position = finalPosition;
//#endif
            //  mainPlayer_RootTransformData.pos = finalPosition;
        }

        public void UpdatePlayerPosition2 (Position newData)
        {
            UpdateCenterEye();

            UpdatePlayerXZPosition(newData.pos.x, newData.pos.z);

            UpdatePlayerYPosition(newData.pos.y);
        }

        public void UpdatePlayerPosition (Transform otherTransform)
        {
            playspace.position = otherTransform.position;
        }

        public void UpdateCenterEye () 
        { 
            centerEye.position = (leftEye.position + rightEye.position) / 2;

            centerEye.rotation = leftEye.rotation;
        }

        public void UpdatePlayerXZPosition (float teleportX, float teleportZ) 
        {
            var finalPlayspacePosition = playspace.position;

            float deltaX = teleportX - centerEye.position.x;

            float deltaZ = teleportZ - centerEye.position.z;

            finalPlayspacePosition.x += deltaX;

            finalPlayspacePosition.z += deltaZ;

            playspace.position = finalPlayspacePosition;
        }

        public void UpdatePlayerXZPosition (Transform otherTransform) 
        {
            var finalPlayspacePosition = playspace.position;

            finalPlayspacePosition.x = otherTransform.position.x;

            finalPlayspacePosition.z = otherTransform.position.z;

            playspace.position = finalPlayspacePosition;
        }

        public void UpdatePlayerYPosition (float teleportY) 
        {
            if (justBumped) 
            {
                justBumped = false;

                return;
            }

            Vector3 finalPlayspacePosition = playspace.position;

            finalPlayspacePosition.y = teleportY;

            if (useManualHeightOffset) 
            {

                finalPlayspacePosition.y += manualYOffset;

                justBumped = true;
            }
            
            playspace.position = finalPlayspacePosition;
        }

        public void SetManualYOffset (float y)
        {
            manualYOffset = y;
        }

        public void BumpYAndUpdateOffset (float deltaY)
        {
            justBumped = true;

            Vector3 finalPlayspacePosition = playspace.position;

            finalPlayspacePosition.y += deltaY;

            playspace.position = finalPlayspacePosition;

            SetManualYOffset(manualYOffset + deltaY);
        }

        public void BumpPlayerUpAndUpdate (float bumpAmount) 
        {
            justBumped = true; 
            
            Vector3 finalPlayspacePosition = playspace.position;

            finalPlayspacePosition.y += bumpAmount;

            playspace.position = finalPlayspacePosition;
            
            SetManualYOffset(manualYOffset + bumpAmount);
        }

        public void BumpPlayerDownAndUpdate (float bumpAmount)
        {
            justBumped = true; 
            
            Vector3 finalPlayspacePosition = playspace.position;

            finalPlayspacePosition.y -= bumpAmount;

            playspace.position = finalPlayspacePosition;

            SetManualYOffset(manualYOffset - bumpAmount);
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

            if (!spectatorCamera)
                spectatorCamera = GameObject.FindGameObjectWithTag("DesktopCamera").transform;

            if (!playspace)
                playspace = GameObject.FindGameObjectWithTag("XRCamera").transform;

            spectatorCamera.transform.localScale = Vector3.one * newScale;
            playspace.transform.localScale = Vector3.one * newScale;

            cameraOffset.cameraYOffset = offsetFix;//newScale;


            //adjust the line renderers our player uses to be scalled accordingly
            foreach (var item in lineRenderersToScaleWithPlayer)
            {
                item.widthMultiplier = newScale;
            }

        }
    }
}