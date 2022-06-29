using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    /// <summary>
    /// Types of different entities data in the scene.
    /// </summary>
    public enum Entity_Type
    {
        none = -1,
        /// <summary>
        /// User's head; we use 0 to represent this entity type.
        /// </summary>
        users_head = 0,

        /// <summary>
        /// User's left hand; we use 1 to represent this entity type.
        /// </summary>
        users_Lhand = 1,

        /// <summary>
        /// User's right hand; we use 2 to represent this entity type.
        /// </summary>
        users_Rhand = 2,
        objects = 3,
        physicsObject = 4,
        main_Player = 5,
        physicsEnd = 8,

        /// <summary>
        /// The starting point of a line; we use 10 to represent this entity type.
        /// </summary>
        Line = 10,

        /// <summary>
        /// The ending point of a line; we use 11 to represent this entity type.
        /// </summary>
        LineEnd = 11,
        LineDelete = 12,
        LineRender = 13,
        LineNotRender = 14,
    }
}
