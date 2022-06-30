using UnityEngine;
using System.Runtime.InteropServices;

namespace Komodo.Runtime
{
    // TODO(rob): move this to GlobalMessageManager.cs
    /// <summary>
    /// Message System: WIP
    /// to send a message
    /// 1. pack a struct with the data you need
    /// 2. serialize that struct
    /// 3. pass the message `type` and the serialized struct in the constructor
    /// 4. call the .Send() method
    /// 5. write a handler and register it in the ProcessMessage function below
    /// 6. this is still a hacky way to do it, so feel free to change/improve as you see fit. 
    /// </summary>
    [System.Serializable]
    public struct KomodoMessage
    {
        /// <summary>
        /// The type of the message being sent.
        /// </summary>
        public string type;

        /// <summary>
        /// The data in the message.
        /// </summary>
        public string data;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The type of the message being sent.</param>
        /// <param name="messageData">The data in the message.</param>
        public KomodoMessage(string type, string messageData)
        {
            this.type = type;
            this.data = messageData;
        }

        /// <summary>
        /// If the current runtime environment is on WEBGL, use <c>SocketIOJSLib</c> to emit a message. Otherwise, use <c>socketSim</c>, which is a simulator that mimics <c>SocketIOJSLib</c>.
        /// </summary>
        public void Send()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SocketIOJSLib.BrowserEmitMessage(this.type, this.data);
#else
            var socketSim = SocketIOEditorSimulator.Instance;

            if (!socketSim)
            {
                Debug.LogWarning("No SocketIOEditorSimulator found");
            }

            socketSim.BrowserEmitMessage(this.type, this.data);
#endif
        }
    }
}