using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    //types of data in scene
    public enum Entity_Type
    {
        none = -1,
        users_head = 0,
        users_Lhand = 1,
        users_Rhand = 2,
        objects = 3,
        physicsObject = 4,
        main_Player = 5,
        physicsEnd = 8,
        Line = 10,
        LineEnd = 11,
        LineDelete = 12,
        LineRender = 13,
        LineNotRender = 14,
    }
}
