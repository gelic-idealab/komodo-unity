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
    /// <summary>
    /// This controls the capture functionality that locates at capture button in the desktop view's menu.
    /// </summary>
    public static class CaptureManager
    {
        /// <summary>
        /// Unity action for starting capture.
        /// </summary>
        static UnityAction startCapture;

        /// <summary>
        /// Unity action for stopping capture.
        /// </summary>
        static UnityAction stopCapture;

        /// <summary>
        /// <c>ToggleCapture</c> is defined/implemented externally, and it used the DllImport attribute. You can find the implementation in <c>socket-funcs.jslib</c>. This file can be found in:
        /// 
        /// <code>Assets -> Packages -> KomodoCore -> KomodoCoreAssets -> Plugin -> jslib </code>
        /// </summary>
        /// <param name="operation">we use 0 to indicate start recording. Other numbers mean stop recording</param>
        /// <param name="session_id">user's current session_id</param>
        [DllImport("__Internal")]
        private static extern void ToggleCapture(int operation, int session_id);


        /// <summary>
        /// <c>Start_Record()</c> first uses <c>NetworkUpdateHandler</c> to assign a session_id and then calls to <c>ToggleCapture(operation, session_id)</c>;
        /// </summary>
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

        /// <summary>
        /// <c>End_Record()</c> first uses <c>NetworkUpdateHandler</c> to assign a session_id and then calls to <c>ToggleCapture(operation, session_id)</c>;
        /// </summary>
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

        /// <summary>
        /// <c>Initialize()</c> occurs in <c>KomodoMenu.cs</c>. As the method name suggests, it initializes KomodoEventManager and adds listeners for both <c>startCapture</c> and <c>stopCapture</c> events.
        /// </summary>
        public static void Initialize () 
        {
            startCapture += Start_Record;
            stopCapture += End_Record;

            KomodoEventManager.StartListening("capture.start", startCapture);
            KomodoEventManager.StartListening("capture.stop", stopCapture);
        }

        /// <summary>
        /// As the method name suggests, it deinitializes KomodoEventManager and adds listeners for both <c>startCapture</c> and <c>stopCapture</c> events.
        /// </summary>
        public static void Deinitialize() 
        {
            KomodoEventManager.StopListening("capture.start", startCapture);
            KomodoEventManager.StopListening("capture.stop", stopCapture);

            startCapture -= Start_Record;
            stopCapture -= End_Record;
        }
    }
}