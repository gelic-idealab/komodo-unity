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

        private string serverName;

        private string sessionStatus;

        private string sessionName;

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

        public void DisplayError (string error)
        {
            SetError(error);

            DisplayStatus();
        }

        public void SetError (string error)
        {
            this.error = $"Error: {error}. See console.";
        }

        public void ClearError ()
        {
            this.error = "";
        }

        public void SetServerName (string name)
        {
            serverName = name;
        }

        public void SetSessionName (string name)
        {
            sessionName = name;
        }

        public void SetSocketID (string id)
        {
            socketID = id;
        }

        public void ClearSocketID ()
        {
            socketID = "[No Socket]";
        }

        public void SetSessionName (int sessionID)
        {
            this.sessionName = $"{sessionID}";
        }

        public void ClearSessionName ()
        {
            this.sessionName = "[No Session]";
        }

        public void DisplayReconnectAttempt (string socketId, string attemptNumber)
        {
            this.socketID = socketId;

            connectDisconnectReconnect = $"[!] {serverName}";

            sessionStatus = $"[!] {sessionName}";

            this.pingPongClients = $"Reconnecting... (attempt {attemptNumber})";

            ClearError();

            DisplayStatus();
        }

        public void DisplayReconnectError (string error)
        {
            connectDisconnectReconnect = $"[!] {serverName}";

            sessionStatus = $"[!] {sessionName}";

            this.pingPongClients = "Reconnect error.";

            DisplayError($"{error}");

            DisplayStatus();
        }

        public void DisplayReconnectFailed ()
        {
            connectDisconnectReconnect = $"[!] {serverName}";

            sessionStatus = $"[!] {sessionName}";

            pingPongClients = "Reconnect failed.";

            SetError("Maximum attempts exceeded.");

            DisplayStatus();
        }

        public void DisplayReconnectSucceeded ()
        {
            connectDisconnectReconnect = $"{serverName}";

            sessionStatus = $"[...] {sessionName}";

            pingPongClients = "Reconnect succeeded.";

            ClearError();

            DisplayStatus();
        }

        public void DisplayConnected ()
        {
            connectDisconnectReconnect = $"{serverName}";

            sessionStatus = $"[...] {sessionName}";

            pingPongClients = "Connected.";

            ClearError();

            DisplayStatus();
        }

        public void DisplayConnectTimeout ()
        {
            connectDisconnectReconnect = $"[!] {serverName}";

            sessionStatus = $"[!] {sessionName}";

            pingPongClients = "Connect timeout.";

            ClearError();

            DisplayStatus();
        }

        public void DisplayConnectError (string error)
        {
            connectDisconnectReconnect = $"[!] {serverName}";

            sessionStatus = $"[!] {sessionName}";

            pingPongClients = "Connect error.";

            SetError($"{error}");

            DisplayStatus();
        }

        public void DisplayDisconnect (string reason)
        {
            connectDisconnectReconnect = $"[!] {serverName}";

            sessionStatus = $"[!] {sessionName}";

            pingPongClients = $"Disconnected. {reason}";

            ClearError();

            DisplayStatus();
        }

        public void DisplayPing ()
        {
            this.pingPongClients = "Ping";

            DisplayStatus();
        }

        public void DisplayPong (int latency)
        {
            connectDisconnectReconnect = $"{serverName}";

            // Don't change session status, in case we're not connected.

            pingPongClients = $"Pong: {latency} ms.";

            ClearError();

            DisplayStatus();
        }

        public void DisplaySocketIOAdapterError(string status)
        {
            this.connectDisconnectReconnect = $"[!] {serverName}";

            sessionStatus = $"[!] {sessionName}";

            pingPongClients = "";

            this.error = $"[SocketIOAdapter] {status}";

            DisplayStatus();
        }

        public void DisplaySessionInfo (string info)
        {
            this.sessionStatus = $"{info}";

            DisplayStatus();
        }

        public void DisplayOtherClientJoined (int client_id)
        {
            this.pingPongClients = "Someone just joined.";

            ClearError();
        }

        public void DisplayOwnClientJoined (int session_id)
        {
            this.connectDisconnectReconnect = $"{serverName}";

            this.sessionStatus = $"{sessionName}";

            ClearError();

            DisplayStatus();
        }

        public void DisplayFailedToJoin (int session_id)
        {
            this.connectDisconnectReconnect = $"{serverName}";

            this.sessionStatus = $"[!] {sessionName}";

            SetError($"Failed to join session {session_id}.");

            DisplayStatus();
        }

        public void DisplayOwnClientLeft (int session_id)
        {
            this.connectDisconnectReconnect = $"{serverName}";

            this.sessionStatus = $"Left {session_id}.";

            ClearError();

            DisplayStatus();
        }

        public void DisplayFailedToLeave (int session_id)
        {
            this.connectDisconnectReconnect = $"{serverName}";

            this.sessionStatus = $"[!!] {sessionName}";

            SetError($"Failed to leave session {session_id}");

            DisplayStatus();
        }

        public void DisplayOtherClientDisconnected (int client_id)
        {
            this.connectDisconnectReconnect = $"{serverName}";

            this.sessionStatus = $"{sessionName}";

            this.pingPongClients = "Someone just left.";

            ClearError();

            DisplayStatus();
        }

        public void DisplayBump (int session_id)
        {
            this.connectDisconnectReconnect = $"[!] {serverName}";

            this.sessionStatus = $"[!] {sessionName}";

            this.pingPongClients = "You were bumped.";

            SetError("You're logged in to the same session in another tab. Press Close Connection & Rejoin to close the other tab's connection.");

            DisplayStatus();
        }

        public void DisplaySendMessageFailed(string reason)
        {
            SetError($"Send message failed: {reason}");

            DisplayStatus();
        }

        private void DisplayStatus()
        {
            socketIODisplay.text = $"{connectDisconnectReconnect}\n{sessionStatus}\n{socketID}\n{pingPongClients}\n{error}";
        }
    }
}
