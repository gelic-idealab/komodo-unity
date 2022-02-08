using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    [System.Serializable]
    public class SessionState
    {
        public int[] clients;
        public EntityState[] entities;
        public int scene;
        public bool isRecording;
    }
}
