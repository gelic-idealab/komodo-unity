using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    public class ConnectionAdapter : MonoBehaviour
    {
        public Text socketIODisplay;

        public void Start ()
        {
            if (socketIODisplay == null)
            {
                Debug.LogError("socketIODisplay Text component was not assigned in ConnectionAdapter");
            }

            NetworkUpdateHandler netHandler = NetworkUpdateHandler.Instance;

            KomodoEventManager.StartListening("connection.leaveAndRejoin", () =>
            {
                SocketIOAdapter.Instance.LeaveAndRejoin();
            });

            KomodoEventManager.StartListening("connection.closeConnectionAndRejoin", () =>
            {
                SocketIOAdapter.Instance.CloseConnectionAndRejoin();
            });
        }

        public void DisplayReconnectAttempt (string socketId, string attemptNumber)
        {
            DisplayStatus($"Reconnecting... (attempt {attemptNumber})");
        }

        public void DisplayReconnectError (string error)
        {
            DisplayStatus($"Reconnect error: {error}");
        }

        public void DisplayReconnectFailed ()
        {
            DisplayStatus($"Reconnect failed. Maximum attempts exceeded.");
        }

        public void DisplayReconnectSucceeded ()
        {
            DisplayStatus("Successfully reconnected.");
        }

        public void DisplayConnected (string id)
        {
            DisplayStatus($"Connected.\n({id})");
        }

        public void DisplayConnectTimeout ()
        {
            DisplayStatus("Connect timeout.");
        }

        public void DisplayConnectError (string error)
        {
            DisplayStatus($"Connect error: {error}");
        }

        public void DisplayDisconnect (string reason)
        {
            DisplayStatus($"Disconnected: {reason}");
        }

        public void DisplayError (string error)
        {
            DisplayStatus($"Error: {error}");
        }

        public void DisplayPing ()
        {
            DisplayStatus("Ping");
        }

        public void DisplayPong (int latency)
        {
            DisplayStatus($"Pong: {latency} ms");
        }

        public void DisplaySocketIOAdapterError(string status)
        {
            DisplayStatus($"[SocketIOAdapter] {status}");
        }
        public void DisplaySessionInfo(string info)
        {
            DisplayStatus($"SessionInfo: {info}");
        }

        private void DisplayStatus(string status)
        {
            socketIODisplay.text = status;
        }
    }
}
