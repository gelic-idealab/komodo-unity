using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Komodo.Runtime
{
    /// <summary>
    /// <c>UnityEvent</c> allows us to use inspector call back for functions like: <c>onFinishedCalibration()</c>, <c>onBumpHeightUp</c>, and <c>onBumpHeightDown</c>.
    /// </summary>
    [System.Serializable]
    public class UnityEvent_Float : UnityEvent<float> 
    {
        // this empty declaration is all that's needed.
    }

    /// <summary>
    /// We use this UnityEvent class to perform inspector call back for function <c>onCalibrationUpdate()</c>.
    /// </summary>
    [System.Serializable]
    public class UnityEvent_Vector3 : UnityEvent<Vector3>
    {
        // this empty declaration is all that's needed.
    }

    /// <summary>
    /// <c>HeightCalibration</c> performs the height calibration functionality in Komodo UI menu. It allows users to adjust their heights manually or automatically (with floors as a reference).
    /// </summary>
    public class HeightCalibration : MonoBehaviour
    {
        /// <summary>
        /// TeleportPlayer script that is attached to the PlayerSet in the heirarchy. This variable will get assigned through inspector. 
        /// </summary>
        public TeleportPlayer teleportPlayer;

        /// <summary>
        /// User's left hand. This variable will get assigned through inspector. Heirarchy: PlayerSet -> PlayspaceAnchor -> Hands -> handL.
        /// </summary>
        public GameObject leftHand;

        /// <summary>
        /// User's right hand. This variable will get assigned through inspector. Heirarchy: PlayerSet -> PlayspaceAnchor -> Hands -> handR.
        /// </summary>
        public GameObject rightHand;

        /// <summary>
        /// this variable represents the walkable layermask.
        /// </summary>
        public LayerMask layerMask;

        /// <summary>
        /// Default value for small bump in height clibration. 0.2f meters.
        /// </summary>
        public float bumpAmountSmall = 0.2f; //meters

        /// <summary>
        /// Default value for large bump in height calibration. 1.0f meters.
        /// </summary>
        public float bumpAmountLarge = 1.0f; //meters

        /// <summary>
        /// UnityEvent for start calibration.
        /// </summary>
        public UnityEvent onStartedCalibration;

        /// <summary>
        /// UnityEvent for updating calibration.
        /// </summary>
        public UnityEvent_Vector3 onCalibrationUpdate;

        /// <summary>
        /// UnityEvent for finishing calibration.
        /// </summary>
        public UnityEvent_Float onFinishedCalibration;

        /// <summary>
        /// Unity Event for bumping height up.
        /// </summary>
        public UnityEvent_Float onBumpHeightUp;

        /// <summary>
        /// Unity Event for bumping height down.
        /// </summary>
        public UnityEvent_Float onBumpHeightDown;

        /// <summary>
        /// We use user's left eye as a starting point for <c>Physics.Raycast()</c> in the <c>ComputeGlobalYPositionOfTerrainBelowPlayer()</c> method.
        /// </summary>
        private Transform leftEye;

        private Vector3 floorHeightDisplayCenter;

        /// <summary>
        /// A boolean value that determines if user is currently calibrating height.
        /// </summary>
        private bool isCalibratingHeight = false;

        /// <summary>
        /// initial <c>transform.position.y</c> value.
        /// </summary>
        private float minYOfHands;

        /// <summary>
        /// Checks whether leftEye is null or not.
        /// </summary>
        /// <exception cref="System.Exception">Throw an exception error if the game object--that this script is attached to --does not have a tag named leftEye. </exception>
        public void OnValidate ()
        {
            if (GameObject.FindWithTag(TagList.leftEye) == null)
            {
                throw new System.Exception($"Could not find GameObject with tag {TagList.leftEye}");
            }
        }

        /// <summary>
        /// Initialize <c>leftEye</c>, <c>minYOfHands</c>, and <c>floorHeightDisplayCenter</c>.
        /// </summary>
        public void Start ()
        {
            if (!leftEye)
            {
                leftEye = GameObject.FindWithTag(TagList.leftEye).transform;
            }

            minYOfHands = leftHand.transform.position.y;

            floorHeightDisplayCenter = new Vector3(leftEye.position.x, minYOfHands, leftEye.position.z);
        }

        /// <summary>
        /// Check if the user is calibrating height right now, if so update the x, y, and z coordinates for users.
        /// </summary>
        public void Update ()
        {
            if (isCalibratingHeight) {
                minYOfHands = GetMinimumYPositionOfHands(leftHand, rightHand);

                floorHeightDisplayCenter.x = leftEye.position.x;
                floorHeightDisplayCenter.y = minYOfHands;
                floorHeightDisplayCenter.z = leftEye.position.z;

                onCalibrationUpdate.Invoke(floorHeightDisplayCenter);
            }
        }

        /// <summary>
        /// Bump height up with a small value.
        /// </summary>
        public void BumpHeightUpSmall ()
        {
            onBumpHeightUp.Invoke(bumpAmountSmall);
        }

        /// <summary>
        /// Bump height down with a small value.
        /// </summary>
        public void BumpHeightDownSmall ()
        {
            onBumpHeightDown.Invoke(bumpAmountSmall);
        }
        /// <summary>
        /// Bump height up with a large value.
        /// </summary>
        public void BumpHeightUpLarge ()
        {
            onBumpHeightUp.Invoke(bumpAmountLarge);
        }

        /// <summary>
        /// Bump height down with a large value.
        /// </summary>
        public void BumpHeightDownLarge ()
        {
            onBumpHeightDown.Invoke(bumpAmountLarge);
        }

        /// <summary>
        /// Start height calibration; show the calibration safety warning (this is not implemented yet) and then set <c>isCalibratingHeight</c> to true so that it will let <c>Update()</c> know the user is calibrating height.
        /// </summary>
        public void StartCalibration ()
        {
            //Debug.Log("Beginning player height calibration.");

            ShowHeightCalibrationSafetyWarning();

            bool useKnee = OfferKneeBasedHeightCalibration();

            if (useKnee) 
            {
                return;
            }

            isCalibratingHeight = true;

            onStartedCalibration.Invoke();
        }

        /// <summary>
        /// This is not yet implemented.
        /// </summary>
        public void ShowHeightCalibrationSafetyWarning ()
        {
            //TODO implement
        }

        /// <summary>
        /// Whether it is using knee based height calibration or not. This is currently not implemented
        /// </summary>
        /// <returns>return false as default since this is not implemented.</returns>
        public bool OfferKneeBasedHeightCalibration ()
        {
            return false; //TODO -- add option so user doesn't have to bend down to reach the floor
        }

        /// <summary>
        /// End calibration; calculate the height to bump player, and trigger <c>FinishedHeightCalibration</c> event. Finally, set <c>isCalibratingHeight</c> to false.
        /// </summary>
        public void EndCalibration ()
        {
            if (!isCalibratingHeight)
            {
                return;
            }

            var handHeight = minYOfHands;

            var terrainHeight = ComputeGlobalYPositionOfTerrainBelowPlayer();

            var heightToBumpPlayer = terrainHeight - handHeight;

            //Debug.Log($"[HeightCalibration] terrain height: {terrainHeight} / handHeight: {handHeight} / heightToBumpPlayer: {heightToBumpPlayer}");

            onFinishedCalibration.Invoke(heightToBumpPlayer);

            KomodoEventManager.TriggerEvent("FinishedHeightCalibration");

            minYOfHands = float.MaxValue;

            isCalibratingHeight = false;
        }

        /// <summary>
        /// Looking for a terrain (walkable) below player and return the global position of the terrain.
        /// </summary>
        /// <returns> the position of the terrain. This value is recieved through raycast.</returns>
        public float ComputeGlobalYPositionOfTerrainBelowPlayer ()
        {
            float globalHeight = 10f;

            if (Physics.Raycast(leftEye.position, Vector3.down, out RaycastHit downHitInfo, layerMask))
            {
                //Debug.Log($"[HeightCalibration] Found terrain from casting down from player position ({leftEye.position.x} {leftEye.position.y} {leftEye.position.z}).");

                return downHitInfo.point.y;
            }

            Vector3 heightenedPlayerPosition = leftEye.position;

            heightenedPlayerPosition.y += globalHeight;

            //Debug.Log($"[HeightCalibration] Could not find terrain below player. Trying to find it from {heightenedPlayerPosition.x} {heightenedPlayerPosition.y} {heightenedPlayerPosition.z}");

            if (Physics.Raycast(heightenedPlayerPosition, Vector3.down, out RaycastHit downFromAboveHitInfo, layerMask))
            {
                //Debug.Log($"[HeightCalibration] Found terrain from casting down from {globalHeight}m above player.");

                return downFromAboveHitInfo.point.y;
            }

            Debug.LogError($"[HeightCalibration] Could not find terrain below player or below  {globalHeight}m above the player. Make sure your layer mask is valid and that there are objects on that layer. Proceeding anyways and returning '0' for the height offset.");

            return 0.0f;
        }

        /// <summary>
        /// Get the minimum y position of users' hands.
        /// </summary>
        /// <param name="handL">left hand</param>
        /// <param name="handR">right hand</param>
        /// <returns>return the smaller y coordinate of the hand</returns>
        public float GetMinimumYPositionOfHands (GameObject handL, GameObject handR)
        {
            var curLeftY = handL.transform.position.y;

            var curRightY = handR.transform.position.y;

            if (curLeftY <= curRightY && curLeftY < minYOfHands)
            {
                return curLeftY;
            }

            if (curRightY <= curLeftY && curRightY < minYOfHands)
            {
                return curRightY;
            }

            return minYOfHands;
        }

        /// <summary>
        /// Get the Y position of user's head.
        /// </summary>
        /// <param name="head">user's head</param>
        /// <returns>return the y coordinate of user's head in the scene.</returns>
        public float GetGlobalYPositionOfHead (GameObject head)
        {
            return head.transform.position.y;
        }
    }
}