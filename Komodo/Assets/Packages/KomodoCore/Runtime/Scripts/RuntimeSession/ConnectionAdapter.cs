using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    public class ConnectionAdapter : MonoBehaviour
    {
        public Text socketIODisplay;

        private string pingPongClients;

        private string socketID;

        private string connectDisconnectReconnect;

        private string session;

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
            connectDisconnectReconnect = $"[!] Disconnected: {reason}";

            DisplayStatus();
        }

        public void DisplayError (string error)
        {
            SetError(error);

            DisplayStatus();
        }

        public void SetError (string error)
        {
            this.error = $"Error: {error}. See console.";
        }

        public void DisplayPing ()
        {
            this.pingPongClients = "Ping";

            DisplayStatus();
        }

        public void DisplayPong (int latency)
        {
            this.pingPongClients = $"Pong: {latency} ms";

            DisplayStatus();
        }

        public void DisplaySocketIOAdapterError(string status)
        {
            this.connectDisconnectReconnect = "[!] SocketIOAdapter";

            this.error = $"[SocketIOAdapter] {status}";

            DisplayStatus();
        }

        public void DisplaySessionInfo (string info)
        {
            this.session = $"{info}";

            DisplayStatus();
        }

        public void DisplayOtherClientJoined (int client_id)
        {
            this.pingPongClients = "Someone just joined.";
        }

        public void DisplayOwnClientJoined (int session_id)
        {
            this.session = $"{session_id}";

            SetError("");

            DisplayStatus();
        }

        public void DisplayFailedToJoin (int session_id)
        {
            this.session = $"[!] {session_id}";

            SetError($"Failed to join session {session_id}.");

            DisplayStatus();
        }

        public void DisplayFailedToLeave (int session_id)
        {
            this.session = $"[!] {session_id}";

            SetError($"Failed to leave session {session_id}");

            DisplayStatus();
        }

        public void DisplayOwnClientLeft (int session_id)
        {
            this.session = $"[Left] {session_id}";

            SetError($"");

            DisplayStatus();
        }

        public void DisplayOtherClientDisconnected (int client_id)
        {
            this.pingPongClients = "Someone just left.";

            DisplayStatus();
        }

        public void DisplayBump (int session_id)
        {
            this.connectDisconnectReconnect = "[!]";

            this.session = $"[!] {session_id}";

            SetError("You're logged in to the same session twice. Press Close Connection & Rejoin to rejoin. This will close the other connection for your other tab, window, or device.");

            DisplayStatus();
        }

        public void DisplaySendMessageFailed(string reason)
        {
            SetError($"Send message failed: {reason}");

            DisplayStatus();
        }

        private void DisplayStatus()
        {
            socketIODisplay.text = $"{connectDisconnectReconnect}\n{session}\n{socketID}\n{pingPongClients}\n{error}";
        }
    }
}
