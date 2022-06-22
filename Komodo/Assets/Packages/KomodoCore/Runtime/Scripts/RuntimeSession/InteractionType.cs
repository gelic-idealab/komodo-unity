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
        /// The action when users click on the show-button that enables models in Komodo menu. We use 2 to represent this interaction.
        /// </summary>
        SHOW = 2,

        /// <summary>
        /// The action when users click on the hide-button that hides the models in Komodo menu. We use 3 to represent this interaction.
        /// </summary>
        HIDE = 3,

        /// <summary>
        /// The action when users grab something in the Komodo. We use 4 to represent this interaction.
        /// </summary>
        GRAB = 4,

        /// <summary>
        /// The action when users release something after grabbing it. We use 5 to represent this interaction.
        /// </summary>
        DROP = 5,

        /// <summary>
        /// We do not have a change scene functionality implemented yet, so this interaction type is not necessary.
        /// </summary>
        CHANGE_SCENE = 6,

        /// <summary>
        /// We do not have a change scene functionality implemented yet, so this interaction type is not necessary.
        /// </summary>
        SLICE_OBJECT = 7,

        /// <summary>
        /// The action when users lock models through Komodo UI menu. We use 8 to represent this interaction.
        /// </summary>
        LOCK = 8,

        /// <summary>
        /// The action when users unlock models through Komodo UI menu. We use 9 to represent this interaction.
        /// </summary>
        UNLOCK = 9,

        /// <summary>
        /// The action when users start drawing. We use 10 to represent this interaction.
        /// </summary>
        LINE = 10,

        /// <summary>
        /// The action when users stop drawing. We use 11 to represent this interaction.
        /// </summary>
        LINE_END = 11,

        /// <summary>
        /// The action when user calls out Komodo UI menu. We use 12 to represent this interaction.
        /// </summary>
        SHOW_MENU = 12,

        /// <summary>
        /// The action when user hides Komodo UI menu. We use 13 to rperesent this interaction. 
        /// </summary>
        HIDE_MENU = 13,

        /// <summary>
        /// The action when user selects the setting tab in the Komodo UI menu. We use 14 to represent this interaction.
        /// </summary>
        SETTING_TAB = 14,

        /// <summary>
        /// The action when user selects the people tab in the Komodo UI menu. We use 15 to represent this interaction.
        /// </summary>
        PEOPLE_TAB = 15,

        /// <summary>
        /// The action when user selects the interaction tab in the Komodo UI menu. We use 16 to represent this interaction.
        /// </summary>
        INTERACTION_TAB = 16,
        
        /// <summary>
        /// The action when user selects the create tab in the Komodo UI menu. We use 17 to represent this interaction.
        /// </summary>
        CREATE_TAB = 17,
    }
}
