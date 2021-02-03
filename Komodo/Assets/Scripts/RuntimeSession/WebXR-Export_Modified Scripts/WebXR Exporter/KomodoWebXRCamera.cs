using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using WebXR;


    public class KomodoWebXRCamera : MonoBehaviour
    {
        public enum CameraID
        {
            Main,
            LeftVR,
            RightVR,
            LeftAR,
            RightAR
        }

        //changed from serializedfield
        public Camera cameraMain, cameraL, cameraR, cameraARL, cameraARR;
        private WebXRState xrState = WebXRState.NORMAL;
        private Rect leftRect, rightRect;
        private int viewsCount = 1;

        void OnEnable()
        {
            WebXRManager.OnXRChange += onVRChange;
            WebXRManager.OnHeadsetUpdate += onHeadsetUpdate;

            cameraMain.transform.localPosition = new Vector3(0, 0, 0);//WebXRManager.Instance.DefaultHeight, 0);
        }

        private void OnDisable()
        {
      
        }
        public void OnDestroy()
        {
            WebXRManager.OnXRChange -= onVRChange;
            WebXRManager.OnHeadsetUpdate -= onHeadsetUpdate;

        }

        public Camera GetCamera(CameraID cameraID)
        {
            switch (cameraID)
            {
                case CameraID.LeftVR:
                    return cameraL;
                case CameraID.RightVR:
                    return cameraR;
                case CameraID.LeftAR:
                    return cameraARL;
                case CameraID.RightAR:
                    return cameraARR;
            }
            return cameraMain;
        }

        private void onVRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            this.viewsCount = viewsCount;
            this.leftRect = leftRect;
            this.rightRect = rightRect;

            if (xrState == WebXRState.VR)
            {
                //set complete camera gameobject to false to prevent update calls from freeflight controller
                cameraMain.gameObject.SetActive(false);

                cameraL.enabled = viewsCount > 0;
                cameraL.rect = leftRect;
                cameraR.enabled = viewsCount > 1;
                cameraR.rect = rightRect;

                cameraARL.enabled = false;
                cameraARR.enabled = false;
            }
            else if(xrState == WebXRState.AR)
            {
                cameraMain.gameObject.SetActive(false);

                cameraL.enabled = false;
                cameraR.enabled = false;

                cameraARL.enabled = viewsCount > 0;
                cameraARL.rect = leftRect;
                cameraARR.enabled = viewsCount > 1;
                cameraARR.rect = rightRect;
            }
            else if (xrState == WebXRState.NORMAL)
            {
                cameraMain.gameObject.SetActive(true);

                cameraL.enabled = false;
                cameraR.enabled = false;

                cameraARL.enabled = false;
                cameraARR.enabled = false;
            }
        }

        private void onHeadsetUpdate(
        Matrix4x4 leftProjectionMatrix,
        Matrix4x4 rightProjectionMatrix,
        Matrix4x4 leftViewMatrix,
        Matrix4x4 rightViewMatrix,
        Matrix4x4 sitStandMatrix)
        {
            if (xrState == WebXRState.VR)
            {
                WebXRMatrixUtil.SetTransformFromViewMatrix(cameraL.transform, leftViewMatrix * sitStandMatrix.inverse);
                cameraL.projectionMatrix = leftProjectionMatrix;
                WebXRMatrixUtil.SetTransformFromViewMatrix(cameraR.transform, rightViewMatrix * sitStandMatrix.inverse);
                cameraR.projectionMatrix = rightProjectionMatrix;
            }
            else if (xrState == WebXRState.AR)
            {
                WebXRMatrixUtil.SetTransformFromViewMatrix(cameraARL.transform, leftViewMatrix * sitStandMatrix.inverse);
                cameraARL.projectionMatrix = leftProjectionMatrix;
                WebXRMatrixUtil.SetTransformFromViewMatrix(cameraARR.transform, rightViewMatrix * sitStandMatrix.inverse);
                cameraARR.projectionMatrix = rightProjectionMatrix;
            }
        }
    }
    
    //for checking for NAN Errors with projection
//    private void onHeadsetUpdate(
//    Matrix4x4 leftProjectionMatrix,
//    Matrix4x4 rightProjectionMatrix,
//    Matrix4x4 leftViewMatrix,
//    Matrix4x4 rightViewMatrix,
//    Matrix4x4 sitStandMatrix)
//    {
//        if (xrActive)
//        {
//            WebXRMatrixUtil.SetTransformFromViewMatrix(cameraL.transform, leftViewMatrix * sitStandMatrix.inverse);

//            if (leftProjectionMatrix.GetColumn(1).IsValid())
//                cameraL.projectionMatrix = leftProjectionMatrix;

//            WebXRMatrixUtil.SetTransformFromViewMatrix(cameraR.transform, rightViewMatrix * sitStandMatrix.inverse);

//            if (rightProjectionMatrix.GetColumn(1).IsValid())
//                cameraR.projectionMatrix = rightProjectionMatrix;

//        }
//    }
//}
