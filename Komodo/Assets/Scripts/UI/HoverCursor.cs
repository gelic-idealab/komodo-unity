using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject cursor;
    private Image cursorImage;
    public Color hoverColor;
    private Color originalColor;

    [Header("Add both Draw objects to avoid using draw features when interacting with UI")]
    public Trigger_Draw[] objectsToDeactivateOnHover;
    
    public void Awake()
    {
        cursorImage = GetComponent<Image>();
    }
    void Start ()
    {

        if (!cursor) {
            throw new Exception("You must set a cursor");
        }
   

        if (!cursorImage) {
            throw new Exception("You must have an Image component on your cursor");
        }

        //do not turn them on as default for desktop
        cursorImage.color = originalColor;
        cursor.SetActive(false);
    }
    

    public void OnPointerEnter(PointerEventData eventData)
    {
        //do not have these interactions in desktop mode
        if (EventSystemManager.Instance.GetXRCurrentState() != WebXR.WebXRState.ENABLED)
            return;
      
        foreach (var item in objectsToDeactivateOnHover)
        {
            item.enabled = false;
        }
      
        originalColor = cursorImage.color;
        cursorImage.color = hoverColor;
        cursor.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //do not have these interactions in desktop mode
        if (EventSystemManager.Instance.GetXRCurrentState() != WebXR.WebXRState.ENABLED)
            return;

        foreach (var item in objectsToDeactivateOnHover)
        {
            item.enabled = true;
        }

        cursorImage.color = originalColor;
        cursor.SetActive(false);
    }

    //on pointerexit does not get called when turning off UI so also do behavior when its disabled aswell
    public void OnDisable()
    {
        foreach (var item in objectsToDeactivateOnHover)
        {
            item.enabled = true;
        }

        if (!cursorImage)
            cursorImage = cursor.GetComponent<Image>();

        cursorImage.color = originalColor;
        cursor.SetActive(false);
    }


}
