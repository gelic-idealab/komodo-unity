//#define TESTING_BEFORE_BUILDING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    /// <summary>
    /// This is one of the files for recording functionality.
    /// </summary>
    public class RecordEvent : MonoBehaviour
    {
        /// <summary>
        /// The button for starting or ending record.
        /// </summary>
        Alternate_Button_Function ABF;

        /// <summary>
        /// This is one of the functions implemented by socket-funcs.jslib. It toggles the capture functionality.
        /// </summary>
        /// <param name="operation">to start recording or stop recording; 0 for start recording and 1 for end recording.</param>
        /// <param name="session_id">id of the current session.</param>
        [DllImport("__Internal")]
        private static extern void ToggleCapture(int operation, int session_id);

        /// <summary>
        /// Current session's id.
        /// </summary>
        public int session_id;

        /// <summary>
        /// Initialize the button by getting its component and adding event listeners (for both start recording and end recording).
        /// </summary>
        void Start()
        {
            ABF = GetComponent<Alternate_Button_Function>();
            ABF.onFirstClick.AddListener(() => Start_Record());
            ABF.onSecondClick.AddListener(() => End_Record());
        }

        /// <summary>
        /// Start recording. If the current environment is WEBGL and not Unity Editor, call <c>ToggleCapture</c> directly. Otherwise, call through <c>SocketIOEditorSimulator</c>.
        /// </summary>
        public void Start_Record()
        {
            session_id = NetworkUpdateHandler.Instance.session_id;
#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
        ToggleCapture(0,session_id);
#else
            SocketIOEditorSimulator.Instance.ToggleCapture(0, session_id);
#endif
        }

        /// <summary>
        /// End recording. If the current environment is WEBGL and not Unity Editor, call <c>ToggleCapture</c> directly. Otherwise, call through <c>SocketIOEditorSimulator</c>.
        /// </summary>
        public void End_Record()
        {
            session_id = NetworkUpdateHandler.Instance.session_id;
#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
        ToggleCapture(1, session_id);
#else
            SocketIOEditorSimulator.Instance.ToggleCapture(0, session_id);
#endif
        }
    }
}
