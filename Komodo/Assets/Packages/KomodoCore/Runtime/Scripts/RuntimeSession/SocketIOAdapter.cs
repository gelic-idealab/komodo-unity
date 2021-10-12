using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    public class SocketIOAdapter : MonoBehaviour
    {
        private ConnectionAdapter connectionAdapter;

        void Start()
        {
            connectionAdapter = (ConnectionAdapter) FindObjectOfType(typeof(ConnectionAdapter));

            if (connectionAdapter == null)
            {
                Debug.LogError("SocketIOAdapter: No object of type ConnectionAdapter was found in the scene.");
            }

#if !UNITY_EDITOR && UNITY_WEBGL

            SocketIOJSLib.SetSocketIOAdapterName(gameObject.name);
#else

            SocketIOEditorSimulator.Instance.SetSocketIOAdapterName(gameObject.name);
#endif
        }

        public void OnReconnectAttempt (string packedString)
        {
            string[] unpackedString = packedString.Split(',');

            string socketId = unpackedString[0];

            string attemptNumber = unpackedString[1];

            connectionAdapter.DisplayReconnectAttempt(socketId, attemptNumber);
        }

        public void OnReceiveStateCatchup (string packedData)
        {
            var state = JsonUtility.FromJson<SessionState>(packedData);

            SessionStateManager.Instance.SetSessionState(state);

            //only update when things are setup if not keep reference in current session state class.
            if (UIManager.IsAlive && GameStateManager.IsAlive && UIManager.Instance.IsReady() && SessionStateManager.Instance.IsReady())
            {
               SessionStateManager.Instance.ApplyCatchup();
            }
        }
    }
}
