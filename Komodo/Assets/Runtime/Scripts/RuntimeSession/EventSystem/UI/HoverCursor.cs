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

        //public void OnEnable()
        //{


        //}

        //public void OnDisable()
        //{


        //}
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

            //do not turn them on as default for desktop
            cursorImage.color = originalColor;
            //cursorGraphic.SetActive(false);

            ShowCursor();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //if (!_doShow) {
            //    return;
            //}
        
            ShowCursor();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //if (!_doShow) {
            //    return;
            //}

            HideCursor();
        }
        
        
        private void ShowCursor() {
            //foreach (var item in objectsToDeactivateOnHover)
            //{
            //    item.SetActive(false);
            //}
        
            originalColor = cursorImage.color;
            cursorImage.color = hoverColor;
            cursorGraphic.SetActive(true);
        }
        
        private void HideCursor() 
        {
            //foreach (var item in objectsToDeactivateOnHover)
            //{
            //    item.SetActive(true);
            //}

            cursorImage.color = originalColor;
            cursorGraphic.SetActive(false);
        }

        
        public void EnableHoverCursor () {
            _doShow = true;
        }

        public void DisableHoverCursor () {
            _doShow = false;
        }

        //on pointerexit does not get called when turning off UI so also do behavior when its disabled aswell
        public void OnDisable()
        {
            HideCursor();
        }
    }
}
