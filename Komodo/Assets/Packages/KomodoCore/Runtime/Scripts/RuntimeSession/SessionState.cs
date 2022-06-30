using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    /// <summary>
    /// The state of the current session; information includes numbers of clients, which scene it is, and whether the session is being recorded.
    /// </summary>
    [System.Serializable]
    public class SessionState
    {
        /// <summary>
        /// A list that stores clients' IDs.
        /// </summary>
        public int[] clients;

        public EntityState[] entities;

        /// <summary>
        /// The current scene of the session.
        /// </summary>
        public int scene;

        /// <summary>
        /// Whether the session is being recorded.
        /// </summary>
        public bool isRecording;
    }
}
