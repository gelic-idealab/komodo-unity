using UnityEngine;

namespace Komodo.Runtime
{
    public struct Draw
    {
        public int clientId;
        public int strokeId;
        public int strokeType;
        public float lineWidth;
        public Vector3 curStrokePos;
        public Vector4 curColor;

        public Draw(int clientId, int strokeId, int strokeType, float lineWidth, Vector3 curStrokePos, Vector4 curColor)
        {
            this.clientId = clientId;
            this.strokeId = strokeId;
            this.strokeType = strokeType;
            this.lineWidth = lineWidth;
            this.curStrokePos = curStrokePos;
            this.curColor = curColor;
        }
    }
}