﻿using System.Collections.Generic;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;

namespace Komodo.Runtime
{
    /// <summary>
    /// Functions to move our avatar
    /// </summary>
    [RequireComponent(typeof(CameraOffset))]
    public class TeleportPlayer : MonoBehaviour
    {
        public float snapTurnAmount = 22.5f;

        public bool useManualHeightOffset = false;

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

        public float manualYOffset = 1.0f;

        float originalFixedDeltaTime;

        private int teleportationCount = 0;


        public void Start()
        {
            originalFixedDeltaTime = Time.fixedDeltaTime;

            originalHeight = cameraOffset.cameraYOffset;

            currentScale = 1;

            SetPlayerSpawnCenter();

            SetPlayerPositionToHome2();
        }
        
        public void Awake()
        {
            if (!cameraSet) 
            {
                cameraSet = GameObject.FindWithTag(TagList.cameraSet).transform;
            }
            
            if (!playspace) 
            {
                playspace = GameObject.FindWithTag(TagList.xrCamera).transform;
            }

            if (!leftEye)
            {
                leftEye = GameObject.FindWithTag(TagList.leftEye).transform;
            }

            if (!rightEye)
            {
                rightEye = GameObject.FindWithTag(TagList.rightEye).transform;
            }

            if (!centerEye)
            {
                centerEye = leftEye;
            }
            
            if (!spectatorCamera)
            {
                spectatorCamera = GameObject.FindWithTag(TagList.desktopCamera).transform;
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
            const string generatedSpawnCenterName = TagList.playerSpawnCenter;

            var spawnCentersFound = GameObject.FindGameObjectsWithTag(TagList.playerSpawnCenter);

            if (currentSpawnCenter == null) {
                Debug.Log("currentSpawnCenter was not found for TeleportPlayer.Proceeding.");
            }

            // If we found gameObjects with the right tag,
            // pick the first one that's different from the current one

            for (int i = 0; i < spawnCentersFound.Length; i += 1) {

                if (currentSpawnCenter == null || spawnCentersFound[i] != currentSpawnCenter.gameObject) {

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

                generatedSpawnCenter.tag = TagList.playerSpawnCenter;

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

            cameraSet.localRotation = rot;
        }

        public void SnapTurnLeft ()
        {
            SnapTurnLeft(snapTurnAmount);
        }

        public void SnapTurnLeft (float degrees)
        {
            UpdateCenterEye();

            playspace.RotateAround(centerEye.position, Vector3.up, degrees);
        }

        public void SnapTurnRight ()
        {
            SnapTurnRight(snapTurnAmount);
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
            cameraSet.position = finalPosition;
//#elif UNITY_WEBGL
            playspace.position = finalPosition;
//#endif
            //  mainPlayer_RootTransformData.pos = finalPosition;
        }

        public void UpdatePlayerPosition2 (Position newData)
        {
            if (teleportationCount >= 2) 
            {
                KomodoEventManager.TriggerEvent("TeleportedTwice");
            }
            UpdateCenterEye();

            UpdatePlayerXZPosition(newData.pos.x, newData.pos.z);

            UpdatePlayerYPosition(newData.pos.y);

            teleportationCount += 1;
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
        /// This function helps teleport the user in spectator/PC mode.It takes in the gameobject teleportAnchor
        /// and assign the position of this gameobject to spectatorCamera, so that player will get teleported.
        /// </summary>
        /// <param name="floorIndicator"> the gameobject that highlights the floor when trying to teleport in spectator/PC mode. </param>
        public void TeleportPlayerPC (GameObject floorIndicator) 
        {
            Vector3 teleportDestination = floorIndicator.transform.position;
            teleportDestination.y = 2.0f; // manually bump player by 2; otherwise, player will get stuck in floor after every teleportation. 
            spectatorCamera.position = teleportDestination;
            //UpdateCenterEye();
        }

        /// <summary>
        /// Update our XRAvatar according to the height
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
                spectatorCamera = GameObject.FindWithTag(TagList.desktopCamera).transform;

            if (!playspace)
                playspace = GameObject.FindWithTag(TagList.xrCamera).transform;

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