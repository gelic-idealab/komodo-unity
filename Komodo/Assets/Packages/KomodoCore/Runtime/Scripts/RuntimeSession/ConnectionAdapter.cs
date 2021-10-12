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
        }

        public void DisplayReconnectAttempt (string socketId, string attemptNumber)
        {
            DisplayStatus($"Reconnecting... (attempt {attemptNumber})");
        }

        private void DisplayStatus(string status)
        {
            socketIODisplay.text = status;
        }
    }
}
