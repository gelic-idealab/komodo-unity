using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    /// <summary>
    /// This class controls the expandability of Komodo UI menu.
    /// </summary>
    public class ToggleExpandability : MonoBehaviour
    {
        /// <summary>
        /// An array of game objects that are expanders.
        /// </summary>
        public GameObject[] expanders;
        
        /// <summary>
        /// An array of game objects that are collapsers.
        /// </summary>
        public GameObject[] collapsers;

        /// <summary>
        /// And array of game objects that are panels.
        /// </summary>
        public GameObject[] panels;

        /// <summary>
        /// Convert an array of collapsers to expandable.
        /// </summary>
        /// <param name="doExpand">a boolean value for deciding whether to expand or not. True for expanding, false for collapsing.</param>
        public void ConvertToExpandable(bool doExpand)
        {
            foreach (GameObject collapser in collapsers)
            {
                collapser.SetActive(doExpand);
            }
            foreach (GameObject expander in expanders)
            {
                expander.SetActive(!doExpand);
            }
            foreach (GameObject panel in panels)
            {
                panel.SetActive(doExpand);
            }
        }

        /// <summary>
        /// Set an array of collapsers to always expanded.
        /// </summary>
        public void ConvertToAlwaysExpanded()
        {
            foreach (GameObject collapser in collapsers)
            {
                collapser.SetActive(false);
            }
            foreach (GameObject expander in expanders)
            {
                expander.SetActive(false);
            }
            foreach (GameObject panel in panels)
            {
                panel.SetActive(true);
            }
        }
    }
}
