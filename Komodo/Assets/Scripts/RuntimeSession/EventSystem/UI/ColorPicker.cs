using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

//we do not add it to gamestatemanager since this only updates when it is enabled to disabled
public class ColorPicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler,  IPointerExitHandler
{
    public LineRenderer[] targets;
    public List<TriggerDraw> drawTargets;
    public Transform colorTargetLocation;
    public Image colorDisplay;
    /*  
    /!\ Don't forget to make the texture readable
    (Select your texture : in Inspector
    [Texture Import Setting] > Texture Type > Advanced > Read/Write enabled > True  then Apply).
    */
    public Texture2D colorPicker;
    public Rect colorPanelRect = new Rect(0, 0, 200, 200);

    private void Awake()
    {
        
        foreach (var item in targets)
        {
            var triggerDraw = item.GetComponent<TriggerDraw>();

            if (triggerDraw == null)
            {
                Debug.LogError("There is no TriggerDraw.cs in Color Tool LineRenderer ", this.gameObject);
                continue ;
            }
            drawTargets.Add(triggerDraw);
        }

        colorPicker = (Texture2D) GetComponent<RawImage>().texture;

        if (!colorPicker)
            Debug.LogError("Color_Picker.cs is missing RawTexture", gameObject);

        colorPanelRect.width = colorPicker.width;
        colorPanelRect.height = colorPicker.height;
        
    }

    Vector3 lastPos;
    void Update()
    {

        Vector2 pickpos = new Vector2(colorTargetLocation.localPosition.x, colorTargetLocation.localPosition.z);// Event.current.mousePosition;//new Vector2(colorTargetLocation.position.x, colorTargetLocation.position.y);

        float aaa = 0.1f * Mathf.Abs((pickpos.x - colorPanelRect.x) / 10f * 10f - 10f) * 512f;

        float bbb = 0.1f * Mathf.Abs((pickpos.y - colorPanelRect.y) / 10f * 10f) * 512f;

        int aaa2 = (int)(aaa * (colorPicker.width / (colorPanelRect.width + 0.0f)));

        int bbb2 = (int)((colorPanelRect.height - bbb) * (colorPicker.height / (colorPanelRect.height + 0.0f)));

        Color col = colorPicker.GetPixel(aaa2, bbb2);

        foreach (var item in targets)
        {
            item.startColor = col;
            item.endColor = col;
        }
        
        colorDisplay.color = col;
    }

    //change color marker depending on image selection location
    public void OnPointerClick(PointerEventData eventData) => colorTargetLocation.position = Input.mousePosition;

  
    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var item in drawTargets)
        {
            item.isSelectingColorPicker = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (var item in drawTargets)
        {
            item.isSelectingColorPicker = false;
        }
    }
    //on pointerexit does not get called when turning off UI so also do behavior when its disabled aswell
    public void OnDisable()
    {
        foreach (var item in drawTargets)
        {
            item.isSelectingColorPicker = false;
        }
    }
}
