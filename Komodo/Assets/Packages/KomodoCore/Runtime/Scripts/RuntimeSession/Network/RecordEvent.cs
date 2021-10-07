//#define TESTING_BEFORE_BUILDING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    public class RecordEvent : MonoBehaviour
    {
        // Start is called before the first frame update
        Alternate_Button_Function ABF;

        [DllImport("__Internal")]
        private static extern void ToggleCapture(int operation, int session_id);

        public int session_id;

        void Start()
        {
            ABF = GetComponent<Alternate_Button_Function>();
            ABF.onFirstClick.AddListener(() => Start_Record());
            ABF.onSecondClick.AddListener(() => End_Record());
        }

        // Update is called once per frame
        public void Start_Record()
        {
            session_id = NetworkUpdateHandler.Instance.session_id;
#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
        ToggleCapture(0,session_id);
#else
            SocketIOEditorSimulator.Instance.ToggleCapture(0, session_id);
#endif
        }


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
