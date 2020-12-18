using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using WebXR;

namespace WebXR
{
    public class WebXRCamera : MonoBehaviour
    {
       
        //changed from serializedfield
        public Camera cameraMain, cameraL, cameraR;
        private bool xrActive;
        private WaitForEndOfFrame wait = new WaitForEndOfFrame();
        private Coroutine postRenderCoroutine;

        [DllImport("__Internal")]
        private static extern void XRPostRender();

        private IEnumerator endOfFrame()
        {
            // Wait until end of frame to report back to WebXR browser to submit frame.
            while (enabled)
            {
                yield return wait;
                XRPostRender();
            }
        }

        void OnEnable()
        {
            WebXRManager.Instance.OnXRChange += onVRChange;
            WebXRManager.Instance.OnHeadsetUpdate += onHeadsetUpdate;

            cameraMain.transform.localPosition = new Vector3(0, WebXRManager.Instance.DefaultHeight, 0);

#if UNITY_EDITOR
            // No editor specific funtionality
#elif UNITY_WEBGL
			postRenderCoroutine = StartCoroutine(endOfFrame());
#endif
        }

        private void OnDisable()
        {
            if (postRenderCoroutine != null)
            {
                StopCoroutine(postRenderCoroutine);
            }
        }
        public void OnDestroy()
        {
            WebXRManager.Instance.OnXRChange -= onVRChange;
            WebXRManager.Instance.OnHeadsetUpdate -= onHeadsetUpdate;

        }

        private void onVRChange(WebXRState state)
        {
            xrActive = state == WebXRState.ENABLED;

            if (xrActive)
            {
                //set complete camera gameobject to false to prevent update calls from freeflight controller
                cameraMain.gameObject.SetActive(false);
                cameraL.enabled = true;
                cameraR.enabled = true;
            }
            else
            {
                cameraMain.gameObject.SetActive(true);
                cameraL.enabled = false;
                cameraR.enabled = false;
            }
        }

        private void onHeadsetUpdate(
            Matrix4x4 leftProjectionMatrix,
            Matrix4x4 rightProjectionMatrix,
            Matrix4x4 leftViewMatrix,
            Matrix4x4 rightViewMatrix,
            Matrix4x4 sitStandMatrix)
        {
            if (xrActive)
            {
                WebXRMatrixUtil.SetTransformFromViewMatrix(cameraL.transform, leftViewMatrix * sitStandMatrix.inverse);

                if(leftProjectionMatrix.GetColumn(1).IsValid())
                cameraL.projectionMatrix = leftProjectionMatrix;

                WebXRMatrixUtil.SetTransformFromViewMatrix(cameraR.transform, rightViewMatrix * sitStandMatrix.inverse);

                if (rightProjectionMatrix.GetColumn(1).IsValid())
                    cameraR.projectionMatrix = rightProjectionMatrix;
                
            }
        }
    }
}