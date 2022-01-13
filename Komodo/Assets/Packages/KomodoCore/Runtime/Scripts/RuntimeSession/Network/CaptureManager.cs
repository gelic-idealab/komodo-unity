//#define TESTING_BEFORE_BUILDING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Komodo.Utilities;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    public static class CaptureManager
    {
        static UnityAction startCapture;
        static UnityAction stopCapture;


        [DllImport("__Internal")]
        private static extern void ToggleCapture(int operation, int session_id);

        // Update is called once per frame
        public static void Start_Record()
        {
            int session_id;
            session_id = NetworkUpdateHandler.Instance.session_id;
#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
            ToggleCapture(0, session_id);
#else
            SocketIOEditorSimulator.Instance.ToggleCapture(0, session_id);
#endif
        }

        public static void End_Record()
        {
            int session_id;
            session_id = NetworkUpdateHandler.Instance.session_id;
#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
            ToggleCapture(1, session_id);
#else
            SocketIOEditorSimulator.Instance.ToggleCapture(1, session_id);
#endif
        }


        public static void Initialize () 
        {
            startCapture += Start_Record;
            stopCapture += End_Record;

            KomodoEventManager.StartListening("capture.start", startCapture);
            KomodoEventManager.StartListening("capture.stop", stopCapture);
        }

        public static void Deinitialize() 
        {
            KomodoEventManager.StopListening("capture.start", startCapture);
            KomodoEventManager.StopListening("capture.stop", stopCapture);

            startCapture -= Start_Record;
            stopCapture -= End_Record;
        }
    }
}