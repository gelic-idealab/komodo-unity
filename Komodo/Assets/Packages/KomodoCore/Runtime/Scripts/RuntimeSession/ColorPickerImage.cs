using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

namespace Komodo.Runtime
{
    public class ColorPickerImage : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        // Should return x and y values between 0 and 1, measuring from the upper-left corner of the rect.
        private Vector2 GetMouseLocalPositionInRect (RectTransform rectTransform, float globalX, float globalY, Camera eventCamera)
        {
            Vector2 globalPoint = new Vector2(globalX, globalY);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, globalPoint, eventCamera, out Vector2 localPoint))
            {
                float localX = localPoint.x / rectTransform.rect.width;

                float localY = Mathf.Abs(localPoint.y / rectTransform.rect.height);

                return new Vector2(localX, localY);
            }

            // The user clicked outside the color picker.

            return new Vector2(-12345f, -12345f);
        }

        public void Update()
        {
            ColorPickerManager.UpdateColors();
        }

        // Change color marker to match image selection location
        public void OnPointerClick(PointerEventData eventData)
        {
            ColorPickerManager.SetSelectedColorCursorPosition(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ColorPickerManager.StartPreviewing();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ColorPickerManager.StopPreviewing();
        }

        public void OnPointerDown (PointerEventData eventData)
        {
            ColorPickerManager.StartDragging();
        }

        public void OnPointerUp (PointerEventData eventData)
        {
            ColorPickerManager.StopDragging();
        }

        // Stop selecting color, because OnPointerExit will not fire if the GameObject is disabled.
        public void OnDisable()
        {
            ColorPickerManager.StopSelectingColor();
        }
    }
}