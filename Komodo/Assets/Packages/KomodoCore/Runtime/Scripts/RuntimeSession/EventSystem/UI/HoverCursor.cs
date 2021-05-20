using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Komodo.Runtime
{
    public class HoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject cursorGraphic;

        private Image cursorImage;

        public Color hoverColor;

        private Color originalColor;

        private bool _doShow;

        [Header("GameObjects to deactivate and activate when selecting in UI")]
        public GameObject[] objectsToDeactivateOnHover;

        public void Awake()
        {
            cursorImage = GetComponent<Image>();
        }

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

        public void OnPointerEnter(PointerEventData eventData)
        {        
            ShowCursor();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideCursor();
        }
        
        
        private void ShowCursor() {        
            originalColor = cursorImage.color;

            cursorImage.color = hoverColor;

            cursorGraphic.SetActive(true);
        }
        
        private void HideCursor() 
        {

            cursorImage.color = originalColor;

            cursorGraphic.SetActive(false);
        }

        //onpointerexit does not get called when turning off UI, so also do behavior when it's disabled as well
        public void OnDisable()
        {
            HideCursor();
        }
    }
}
