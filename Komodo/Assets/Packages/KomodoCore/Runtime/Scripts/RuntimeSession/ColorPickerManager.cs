using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

namespace Komodo.Runtime
{
    public static class ColorPickerManager
    {
        public static List<LineRenderer> lineRenderers;

        public static List<TriggerDraw> triggers;

        public static Camera handCamera;

        [Tooltip("Requires a RectTransform and a RawImage component with a texture in it. Assumes its image completely fills its RectTransform.")]
        public static GameObject colorImageObject;

        public static GameObject selectedColorCursor;

        public static GameObject previewColorCursor;

        public static Image selectedColorDisplay;

        public static Image previewColorDisplay;

        public static MenuPlacement menuPlacement;
        /*  

        /!\ Don't forget to make the texture readable. Select your texture in the Inspector. Choose [Texture Import Setting] > Texture Type > Advanced > Read/Write enabled > True, then Apply.

        */
        private static Texture2D colorTexture;

        private static RectTransform colorRectTransform;

        private static Vector2 selectedColorCursorNormalizedPosition;

        private static Vector2 previewColorCursorNormalizedPosition;

        private static bool _isPreviewing;

        private static bool _isDragging;

        private static bool _isInVR;

        private static UnityAction _enable;

        private static UnityAction _disable;

        private static RectTransform previewColorCursorRectTransform;

        private static RectTransform selectedColorCursorRectTransform;

        public static void Init ()
        {
            _isPreviewing = false;

            _isDragging = false;
        }

        public static void AssignComponentReferences (List<LineRenderer> _lineRenderers, List<TriggerDraw> _triggers, Camera _handCamera, GameObject _colorImageObject, GameObject _selectedColorCursor, GameObject _previewColorCursor, Image _selectedColorDisplay, Image _previewColorDisplay)
        {
            colorImageObject = _colorImageObject;

            InitColorComponents();

            lineRenderers = _lineRenderers;

            triggers = _triggers;

            foreach (var lineRenderer in lineRenderers)
            {
                var triggerDraw = lineRenderer.GetComponent<TriggerDraw>();

                if (triggerDraw == null)
                {
                    Debug.LogError("There is no TriggerDraw.cs in Color Tool LineRenderer ");

                    continue;
                }

                triggers.Add(triggerDraw);
            }

            selectedColorCursor = _selectedColorCursor;

            previewColorCursor = _previewColorCursor;

            InitColorCursors();

            previewColorDisplay = _previewColorDisplay;

            selectedColorDisplay = _selectedColorDisplay;
        }

        private static void InitColorCursors ()
        {
            if (!previewColorCursor.transform)
            {
                throw new MissingFieldException("transform on previewColorCursor");
            }

            previewColorCursorRectTransform = previewColorCursor.GetComponent<RectTransform>();

            if (!previewColorCursorRectTransform)
            {
                throw new MissingComponentException("RectTransform on previewColorCursor");
            }

            if (!selectedColorCursor.transform)
            {
                throw new MissingFieldException("transform on selectedColorCursor");
            }

            selectedColorCursorRectTransform = selectedColorCursor.GetComponent<RectTransform>();

            if (!selectedColorCursorRectTransform)
            {
                throw new MissingComponentException("RectTransform on selectedColorCursor");
            }
        }

        private static void InitColorComponents ()
        {
            colorRectTransform = colorImageObject.GetComponent<RectTransform>();

            if (colorRectTransform == null)
            {
                throw new MissingComponentException("RectTransform on colorImageObject");
            }

            RawImage colorImage = colorImageObject.GetComponent<RawImage>();

            if (colorImage == null)
            {
                throw new MissingComponentException("RawImage on colorImageObject");
            }

            colorTexture = (Texture2D) colorImage.texture;

            if (!colorTexture)
            {
                throw new MissingReferenceException("texture in colorImage");
            }
        }

        public static void AssignMenuPlacement (MenuPlacement placement)
        {
            menuPlacement = placement;
        }

        public static void InitListeners ()
        {
            _enable += Enable;

            KomodoEventManager.StartListening("drawTool.enable", _enable);

            _disable += Disable;

            KomodoEventManager.StartListening("drawTool.disable", _disable);
        }

        public static void TryGrabPlayerDrawTargets()
        {
            var player = GameObject.FindWithTag(TagList.player);

            if (player && player.TryGetComponent(out PlayerReferences playRef))
            {
                lineRenderers.Add(playRef.drawL.GetComponent<LineRenderer>());

                lineRenderers.Add(playRef.drawR.GetComponent<LineRenderer>());
            }
        }

        private static Vector2 GetMouseLocalPositionInRectFromPointerEventData(RectTransform rectTransform, PointerEventData data)
        {
            if (data.pressEventCamera == null)
            {
                // pressEventCamera was null for PointerEventData. This is expected for Screen Space - Overlay canvases, but not otherwise.

                // For some reason, in VR mode, pressEventCamera is null.

                //TODO - catch errors for VR mode.
            }

            return GetMouseLocalPositionInRect(rectTransform, data.position.x, data.position.y, data.enterEventCamera);
        }

        // Should return x and y values between 0 and 1, measuring from the upper-left corner of the rect.
        private static Vector2 GetMouseLocalPositionInRect (RectTransform rectTransform, float globalX, float globalY, Camera eventCamera)
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

        private static Vector2 g3dcnpir;

        private static Vector2 Get3DCursorNormalizedPositionInRect (RectTransform rectTransform, float localX, float localZ)
        {
            float normX = localX / rectTransform.rect.width;

            float normY = Mathf.Abs(localZ / rectTransform.rect.height);

            g3dcnpir = new Vector2(normX, normY);

            return new Vector2(normX, normY);
        }

        private static Color GetPixelFromNormalizedPosition (Texture2D texture, Vector2 normalizedPosition)
        {
            float normYFromBottom = (normalizedPosition.y * -1f) + 1f;

            int textureY = (int) (normYFromBottom * texture.height);

            int textureX = (int) (normalizedPosition.x * texture.width);

            return texture.GetPixel(textureX, textureY);
        }

        private static void SetLineRenderersColor (List<LineRenderer> _lineRenderers, Color color)
        {
            foreach (var lineRenderer in _lineRenderers)
            {
                lineRenderer.startColor = color;

                lineRenderer.endColor = color;
            }
        }

        private static void DisplayColor (Image displayObject, Color color)
        {
            displayObject.color = color;
        }

        private static void Enable ()
        {
            MenuAnchor anchor = menuPlacement.GetCurrentMenuAnchor();

            if (!anchor) {
                Debug.LogError("Error: could not enable color picker. Could not find current menu anchor from the MenuPlacement script.");

                return;
            }

            if (anchor.kind == MenuAnchor.Kind.SCREEN)
            {
                _isInVR = false;

                return;
            }

            _isInVR = true;
        }

        private static void Disable ()
        {
            // do nothing.
        }

        public static void StartDragging ()
        {
            SetIsDragging(true);
        }

        public static void StopDragging ()
        {
            SetIsDragging(false);
        }

        public static void SetIsDragging (bool value)
        {
            _isDragging = value;
        }

        public static void SetIsPreviewing (bool value)
        {
            _isPreviewing = value;
        }

        public static void SetSelectedColorCursorPosition (PointerEventData eventData)
        {
            selectedColorCursor.transform.position = Input.mousePosition;

            selectedColorCursorNormalizedPosition = GetMouseLocalPositionInRectFromPointerEventData(colorRectTransform, eventData);
        }

        public static void UpdateColors()
        {
            if (_isPreviewing)
            {
                if(!_isInVR)
                {
                    previewColorCursorNormalizedPosition = GetMouseLocalPositionInRect(colorRectTransform, Input.mousePosition.x, Input.mousePosition.y, null);
                }
                else
                {
                    previewColorCursorNormalizedPosition = Get3DCursorNormalizedPositionInRect(colorRectTransform,previewColorCursorRectTransform.localPosition.x, previewColorCursorRectTransform.localPosition.z);
                }

                Color previewColor = GetPixelFromNormalizedPosition(colorTexture, previewColorCursorNormalizedPosition);

                DisplayColor(previewColorDisplay, previewColor);
            }

            if (_isDragging)
            {
                if(!_isInVR)
                {
                    selectedColorCursorNormalizedPosition = GetMouseLocalPositionInRect(colorRectTransform, Input.mousePosition.x, Input.mousePosition.y, null);
                }
                else
                {
                    selectedColorCursorNormalizedPosition = Get3DCursorNormalizedPositionInRect(colorRectTransform, selectedColorCursorRectTransform.localPosition.x, selectedColorCursorRectTransform.localPosition.z);
                }

                Color selectedColor = GetPixelFromNormalizedPosition(colorTexture, selectedColorCursorNormalizedPosition);

                DisplayColor(selectedColorDisplay, selectedColor);

                SetLineRenderersColor(lineRenderers, selectedColor);
            }
        }

        public static void ShowPreviewColor ()
        {
            previewColorDisplay.enabled = true;
        }

        public static void HidePreviewColor ()
        {
            previewColorDisplay.enabled = false;
        }

        public static void StopSelectingColor ()
        {
            foreach (var item in triggers)
            {
                item.isSelectingColorPicker = false;
            }
        }

        public static void SetIsSelectingColor (bool value)
        {
            foreach (var item in triggers)
            {
                item.isSelectingColorPicker = value;
            }
        }

        public static void StartPreviewing ()
        {
            SetIsPreviewing(true);

            ShowPreviewColor();

            SetIsSelectingColor(true);
        }

        public static void StopPreviewing ()
        {
            SetIsPreviewing(false);

            HidePreviewColor();

            SetIsSelectingColor(false);
        }
    }
}