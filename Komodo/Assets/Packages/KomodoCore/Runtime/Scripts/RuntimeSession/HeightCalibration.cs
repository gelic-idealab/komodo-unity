using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Komodo.Runtime
{
    [System.Serializable]
    public class UnityEvent_Float : UnityEvent<float> 
    {
        // this empty declaration is all that's needed.
    }

    [System.Serializable]
    public class UnityEvent_Vector3 : UnityEvent<Vector3>
    {
        // this empty declaration is all that's needed.
    }

    public class HeightCalibration : MonoBehaviour
    {
        public TeleportPlayer teleportPlayer;

        public GameObject leftHand;

        public GameObject rightHand;

        public LayerMask layerMask;

        public float bumpAmountSmall = 0.2f; //meters

        public float bumpAmountLarge = 1.0f; //meters

        public UnityEvent onStartedCalibration;

        public UnityEvent_Vector3 onCalibrationUpdate;

        public UnityEvent_Float onFinishedCalibration;

        public UnityEvent_Float onBumpHeightUp;

        public UnityEvent_Float onBumpHeightDown;

        private Transform leftEye;

        private Vector3 floorHeightDisplayCenter;

        private bool isCalibratingHeight = false;

        private float minYOfHands;

        public void Awake ()
        {
            if (GameObject.FindWithTag(TagList.leftEye) == null)
            {
                throw new System.Exception($"Could not find GameObject with tag {TagList.leftEye}");
            }
        }

        public void Start ()
        {
            if (!leftEye)
            {
                leftEye = GameObject.FindWithTag(TagList.leftEye).transform;
            }

            minYOfHands = leftHand.transform.position.y;

            floorHeightDisplayCenter = new Vector3(leftEye.position.x, minYOfHands, leftEye.position.z);
        }

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

        public void BumpHeightUpSmall ()
        {
            onBumpHeightUp.Invoke(bumpAmountSmall);
        }

        public void BumpHeightDownSmall ()
        {
            onBumpHeightDown.Invoke(bumpAmountSmall);
        }
        public void BumpHeightUpLarge ()
        {
            onBumpHeightUp.Invoke(bumpAmountLarge);
        }

        public void BumpHeightDownLarge ()
        {
            onBumpHeightDown.Invoke(bumpAmountLarge);
        }

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

        public void ShowHeightCalibrationSafetyWarning ()
        {
            //TODO implement
        }

        public bool OfferKneeBasedHeightCalibration ()
        {
            return false; //TODO -- add option so user doesn't have to bend down to reach the floor
        }

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

        public float GetGlobalYPositionOfHead (GameObject head)
        {
            return head.transform.position.y;
        }
    }
}