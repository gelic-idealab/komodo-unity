using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    public class ConnectionAdapter : MonoBehaviour
    {
        public Text socketIODisplay;

        private string pingPong;

        private string socketID;

        private string connectDisconnectReconnect;

        private string error;

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
            this.socketID = socketId;

            connectDisconnectReconnect = $"Reconnecting... (attempt {attemptNumber})";

            DisplayStatus();
        }

        public void DisplayReconnectError (string error)
        {
            connectDisconnectReconnect = $"Reconnect error: {error}";

            DisplayStatus();
        }

        public void DisplayReconnectFailed ()
        {
            connectDisconnectReconnect = "Reconnect failed. Maximum attempts exceeded.";

            DisplayStatus();
        }

        public void DisplayReconnectSucceeded ()
        {
            connectDisconnectReconnect = "Successfully reconnected.";

            DisplayStatus();
        }

        public void DisplayConnected (string id)
        {
            socketID = id;

            connectDisconnectReconnect = "Connected.";

            DisplayStatus();
        }

        public void DisplayConnectTimeout ()
        {
            connectDisconnectReconnect = "Connect timeout.";

            DisplayStatus();
        }

        public void DisplayConnectError (string error)
        {
            connectDisconnectReconnect = $"Connect error: {error}";

            DisplayStatus();
        }

        public void DisplayDisconnect (string reason)
        {
            connectDisconnectReconnect = $"Disconnected: {reason}";

            DisplayStatus();
        }

        public void DisplayError (string error)
        {
            this.error = $"Error: {error}";

            DisplayStatus();
        }

        public void DisplayPing ()
        {
            this.pingPong = "Ping";

            DisplayStatus();
        }

        public void DisplayPong (int latency)
        {
            this.pingPong = $"Pong: {latency} ms";

            DisplayStatus();
        }

        public void DisplaySocketIOAdapterError(string status)
        {
            this.error = $"[SocketIOAdapter] {status}";

            DisplayStatus();
        }
        public void DisplaySessionInfo(string info)
        {
            this.error = $"SessionInfo: {info}";

            DisplayStatus();
        }

        private void DisplayStatus()
        {
            socketIODisplay.text = $"{connectDisconnectReconnect}\n{socketID}\n{pingPong}\n{error}";
        }
    }
}
