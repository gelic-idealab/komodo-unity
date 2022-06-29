using UnityEngine;

namespace Komodo.Runtime
{
    /// <summary>
    /// The structure for drawings.
    /// </summary>
    public struct Draw
    {
        /// <summary>
        /// The id of the user who is sending the draw update
        /// </summary>
        public int clientId;

        /// <summary>
        /// A unique ID among all entities (draw strokes, models, model pack subobjects).
        /// </summary>
        public int strokeId;

        /// <summary>
        /// The kind of action being performed. See <c>Entity_TypeLine ... Entity_Type.LineNotRender</c>
        /// </summary>
        public int strokeType;

        /// <summary>
        /// Used for continue line. The visual thickness of the line.
        /// </summary>
        public float lineWidth;

        /// <summary>
        /// Used for continue line and end lin. The endpoint of the currently drawn line segment.
        /// </summary>
        public Vector3 curStrokePos;

        /// <summary>
        /// Used for continue line. The color of the line segment.
        /// </summary>
        public Vector4 curColor;

        /// <summary>
        /// This is being called by the DrawingInstanceManager. It sends update to NetworkUpdateHandler.
        /// </summary>
        /// <param name="clientId"> client ID </param>
        /// <param name="strokeId"> ID of the current line</param>
        /// <param name="strokeType"> Kind of action being performed.</param>
        /// <param name="lineWidth">The visual thickness of the line.</param>
        /// <param name="curStrokePos">The endpoint of the currently drawn line segment.</param>
        /// <param name="curColor">The color of the line segment.</param>
        public Draw(int clientId, int strokeId, int strokeType, float lineWidth, Vector3 curStrokePos, Vector4 curColor)
        {
            this.clientId = clientId;
            this.strokeId = strokeId;
            this.strokeType = strokeType;
            this.lineWidth = lineWidth;
            this.curStrokePos = curStrokePos;
            this.curColor = curColor;
        }

        // TODO: Add constructors for ContinueLine, EndLine, ShowLine, HideLIne
    }
}