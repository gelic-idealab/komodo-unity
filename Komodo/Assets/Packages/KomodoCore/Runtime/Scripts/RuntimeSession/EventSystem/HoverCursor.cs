using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Komodo.Runtime
{
    /// <summary>
    /// A class that manages the cursor for Komodo VR control.
    /// </summary>
    public class HoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// The cursor GameObject.
        /// </summary>
        public GameObject cursorGraphic;

        /// <summary>
        /// The image of the cursor.
        /// </summary>
        private Image cursorImage;

        /// <summary>
        /// Color of the hover.
        /// </summary>
        public Color hoverColor;

        /// <summary>
        /// Color of the cursor when it is not hovering on anything.
        /// </summary>
        private Color originalColor;

        /// <summary>
        /// A boolean value that determines whether to show the cursor or not.
        /// </summary>
        private bool _doShow;

        /// <summary>
        /// An array of GameObject to deactivate and active when selecting in UI.
        /// </summary>
        [Header("GameObjects to deactivate and activate when selecting in UI")]
        public GameObject[] objectsToDeactivateOnHover;

        /// <summary>
        /// Get the Image component that is attached to this script before the script is ran.
        /// </summary>
        public void Awake()
        {
            cursorImage = GetComponent<Image>();
        }

        /// <summary>
        /// Check if <c>cursorGraphic</c> and <c>cursorImage</c> are assigned. Set <c>cursorImage</c>'s color as <c>originalColor</c>.
        /// </summary>
        /// <exception cref="Exception"></exception>
        void Start ()
        {
            if (!cursorGraphic) 
            {
                throw new Exception("You must set a cursor");
            }

            if (!cursorImage)
            {
                throw new Exception("You must have an Image component on your cursor");
            }

            //do not turn it on as default for desktop
            cursorImage.color = originalColor;

            ShowCursor();
        }

        /// <summary>
        /// When the invisible pointer from the controller enters certain interfaces, show cursor.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {        
            ShowCursor();
        }

        /// <summary>
        /// When the invisible pointer from the controller enters certain interafaces, hide cursor.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            HideCursor();
        }
        
        /// <summary>
        /// Show cursor.
        /// </summary>
        private void ShowCursor() {        
            originalColor = cursorImage.color;

            cursorImage.color = hoverColor;

            cursorGraphic.SetActive(true);
        }
        
        /// <summary>
        /// Hide cursor.
        /// </summary>
        private void HideCursor() 
        {

            cursorImage.color = originalColor;

            cursorGraphic.SetActive(false);
        }

        //onpointerexit does not get called when turning off UI, so also do behavior when it's disabled as well
        /// <summary>
        /// When this script is disabled, hide cursor.
        /// </summary>
        public void OnDisable()
        {
            HideCursor();
        }
    }
}
