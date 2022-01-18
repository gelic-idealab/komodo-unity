using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    public class ColorPickerPrefab : MonoBehaviour
    {
        [ShowOnly]
        public List<LineRenderer> lineRenderers;

        [ShowOnly]
        public List<TriggerDraw> triggers;

        [ShowOnly]
        public Camera handCamera;

        [Tooltip("Requires a RectTransform and a RawImage component with a texture in it. Assumes its image completely fills its RectTransform.")]
        public GameObject colorImageObject;

        public GameObject selectedColorCursor;

        public GameObject previewColorCursor;

        public Image selectedColorDisplay;

        public Image previewColorDisplay;
        /*  

        /!\ Don't forget to make the texture readable. Select your texture in the Inspector. Choose [Texture Import Setting] > Texture Type > Advanced > Read/Write enabled > True, then Apply.

        */

        public void Start()
        {
            if (!colorImageObject)
            {
                throw new UnassignedReferenceException("colorImageObject");
            }

            ColorPickerManager.AssignComponentReferences(lineRenderers, triggers, handCamera, colorImageObject, selectedColorCursor, previewColorCursor, selectedColorDisplay, previewColorDisplay);

            TryGrabPlayerDrawTargets();
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
    }
}