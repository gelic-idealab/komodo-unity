using UnityEngine;

namespace Komodo.Runtime
{
    [System.Serializable]
    public struct Position
    {
        public int clientId;
        public int entityId;
        public int entityType;
        public float scaleFactor;
        public Quaternion rot;
        public Vector3 pos;

        public Position(int clientId, int entityId, int entityType, float scaleFactor, Quaternion rot, Vector3 pos)
        {
            this.clientId = clientId;
            this.entityId = entityId;
            this.entityType = entityType;
            this.scaleFactor = scaleFactor;
            this.rot = rot;
            this.pos = pos;
        }
    }
}