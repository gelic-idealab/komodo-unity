using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

namespace Komodo.Runtime
{
    public class ColorPicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public List<LineRenderer> lineRenderers;

        public List<TriggerDraw> triggers;

        public Camera handCamera;

        [Tooltip("Requires a RectTransform and a RawImage component with a texture in it. Assumes its image completely fills its RectTransform.")]
        public GameObject colorImageObject;

        public GameObject selectedColorCursor;

        public GameObject previewColorCursor;

        public Image selectedColorDisplay;

        public Image previewColorDisplay;

        public MenuPlacement menuPlacement;
        /*  

        /!\ Don't forget to make the texture readable. Select your texture in the Inspector. Choose [Texture Import Setting] > Texture Type > Advanced > Read/Write enabled > True, then Apply.

        */
        private Texture2D colorTexture;

        private RectTransform colorRectTransform;

        private Vector2 selectedColorCursorNormalizedPosition;

        private Vector2 previewColorCursorNormalizedPosition;

        private bool _isPreviewing;

        private bool _isDragging;

        private bool _isInVR;

        private UnityAction _enable;

        private UnityAction _disable;

        private RectTransform previewColorCursorRectTransform;

        private RectTransform selectedColorCursorRectTransform;

        private void Awake()
        {
            _isPreviewing = false;

            _isDragging = false;
        }

        public void Start()
        {
            if (!menuPlacement)
            {
                throw new UnassignedReferenceException("menuPlacement");
            }

            if (!colorImageObject)
            {
                throw new UnassignedReferenceException("colorImageObject");
            }

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

            foreach (var lineRenderer in lineRenderers)
            {
                var triggerDraw = lineRenderer.GetComponent<TriggerDraw>();

                if (triggerDraw == null)
                {
                    Debug.LogError("There is no TriggerDraw.cs in Color Tool LineRenderer ", gameObject);

                    continue;
                }

                triggers.Add(triggerDraw);
            }

            if (!selectedColorCursor.transform)
            {
                throw new MissingFieldException("transform on selectedColorCursor");
            }

            if (!previewColorCursor.transform)
            {
                throw new MissingFieldException("transform on previewColorCursor");
            }

            previewColorCursorRectTransform = previewColorCursor.GetComponent<RectTransform>();

            if (!previewColorCursorRectTransform)
            {
                throw new MissingComponentException("RectTransform on previewColorCursor");
            }

            selectedColorCursorRectTransform = selectedColorCursor.GetComponent<RectTransform>();

            if (!selectedColorCursorRectTransform)
            {
                throw new MissingComponentException("RectTransform on selectedColorCursor");
            }

            TryGrabPlayerDrawTargets();

            _enable += Enable;

            KomodoEventManager.StartListening("drawTool.enable", _enable);

            _disable += Disable;

            KomodoEventManager.StartListening("drawTool.disable", _disable);
        }

        public void TryGrabPlayerDrawTargets()
        {
            var player = GameObject.FindWithTag(TagList.player);

            if (player && player.TryGetComponent(out PlayerReferences playRef))
            {
                lineRenderers.Add(playRef.drawL.GetComponent<LineRenderer>());

                lineRenderers.Add(playRef.drawR.GetComponent<LineRenderer>());
            }
        }

        private Vector2 GetMouseLocalPositionInRectFromPointerEventData(RectTransform rectTransform, PointerEventData data)
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

        private Vector2 g3dcnpir;

        private Vector2 Get3DCursorNormalizedPositionInRect (RectTransform rectTransform, float localX, float localZ)
        {
            float normX = localX / rectTransform.rect.width;

            float normY = Mathf.Abs(localZ / rectTransform.rect.height);

            g3dcnpir = new Vector2(normX, normY);

            return new Vector2(normX, normY);
        }

        private Color GetPixelFromNormalizedPosition (Texture2D texture, Vector2 normalizedPosition)
        {
            float normYFromBottom = (normalizedPosition.y * -1f) + 1f;

            int textureY = (int) (normYFromBottom * texture.height);

            int textureX = (int) (normalizedPosition.x * texture.width);

            return texture.GetPixel(textureX, textureY);
        }

        private void SetLineRenderersColor (List<LineRenderer> _lineRenderers, Color color)
        {
            foreach (var lineRenderer in _lineRenderers)
            {
                lineRenderer.startColor = color;

                lineRenderer.endColor = color;
            }
        }

        private void DisplayColor (Image displayObject, Color color)
        {
            displayObject.color = color;
        }

        private void Enable ()
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

        private void Disable ()
        {
            // do nothing.
        }

        public void Update()
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

        private void ShowPreviewColor ()
        {
            previewColorDisplay.enabled = true;
        }

        private void HidePreviewColor ()
        {
            previewColorDisplay.enabled = false;
        }

        // Change color marker to match image selection location
        public void OnPointerClick(PointerEventData eventData)
        {
            selectedColorCursor.transform.position = Input.mousePosition;

            selectedColorCursorNormalizedPosition = GetMouseLocalPositionInRectFromPointerEventData(colorRectTransform, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPreviewing = true;

            ShowPreviewColor();

            foreach (var item in triggers)
            {
                item.isSelectingColorPicker = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPreviewing = false;

            HidePreviewColor();

            foreach (var item in triggers)
            {
                item.isSelectingColorPicker = false;
            }
        }

        public void OnPointerDown (PointerEventData eventData)
        {
            _isDragging = true;
        }

        public void OnPointerUp (PointerEventData eventData)
        {
            _isDragging = false;
        }

        // Stop selecting color, because OnPointerExit will not fire if the GameObject is disabled.
        public void OnDisable()
        {
            foreach (var item in triggers)
            {
                item.isSelectingColorPicker = false;
            }
        }
    }
}