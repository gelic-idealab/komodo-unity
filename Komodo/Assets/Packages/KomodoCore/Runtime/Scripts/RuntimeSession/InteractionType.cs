using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{   
    /// <summary>
    /// These are the interaction types that will be captured through our capture functionality. These interaction types are represented with integers.
    /// </summary>
    public enum INTERACTIONS
    {   
        /// <summary>
        /// The moment when users rotate, turn, or move their heads. We use 0 to represent this interaction type.
        /// </summary>
        LOOK = 0,

        /// <summary>
        /// The moment when users stop rotating, turning, or moving their heads. We use 1 to represent this interaction type.
        /// </summary>
        LOOK_END = 1,

        /// <summary>
        /// The action that users click on the show-button that enables models in Komodo menu.
        /// </summary>
        SHOW = 2,

        /// <summary>
        /// The action that users click on the hide-button that hides the models in Komodo menu.
        /// </summary>
        HIDE = 3,
        GRAB = 4,
        DROP = 5,
        CHANGE_SCENE = 6,
        SLICE_OBJECT = 7,
        LOCK = 8,
        UNLOCK = 9,
        LINE = 10,
        LINE_END = 11,
        SHOW_MENU = 12,
        HIDE_MENU = 13,

        SETTING_TAB = 14,
        PEOPLE_TAB = 15,
        INTERACTION_TAB = 16,
        CREATE_TAB = 17,
    }
}
