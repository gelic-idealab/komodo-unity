using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    [System.Serializable]
    public struct EntityState
    {
        public int id;
        public Position latest;
        public bool render;
        public bool locked;
    }
}
