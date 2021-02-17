using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject cursorGraphic;
    private Image cursorImage;
    public Color hoverColor;
    private Color originalColor;

    [Header("GameObjects to deactivate and activate when selecting in UI")]
    public GameObject[] objectsToDeactivateOnHover;
    
    public void Awake()
    {
        cursorImage = GetComponent<Image>();
    }
    void Start ()
    {

        if (!cursorGraphic) {
            throw new Exception("You must set a cursor");
        }
   

        if (!cursorImage) {
            throw new Exception("You must have an Image component on your cursor");
        }

        //do not turn them on as default for desktop
        cursorImage.color = originalColor;
        cursorGraphic.SetActive(false);
    }
    

    public void OnPointerEnter(PointerEventData eventData)
    {
        //do not send interactions when quiting app (to avoid recreating instance on exit) and to disable interactions in desktop mode
        if (!EventSystemManager.IsAlive || EventSystemManager.Instance.GetXRCurrentState() != WebXR.WebXRState.VR)
            return;
      
        foreach (var item in objectsToDeactivateOnHover)
        {
            item.SetActive(false);
        }
      
        originalColor = cursorImage.color;
        cursorImage.color = hoverColor;
        cursorGraphic.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //do not have these interactions in desktop mode
        if (!EventSystemManager.IsAlive || EventSystemManager.Instance.GetXRCurrentState() != WebXR.WebXRState.VR)
            return;

        foreach (var item in objectsToDeactivateOnHover)
        {
            item.SetActive(true);
        }

        cursorImage.color = originalColor;
        cursorGraphic.SetActive(false);
    }

    //on pointerexit does not get called when turning off UI so also do behavior when its disabled aswell
    public void OnDisable()
    {
        foreach (var item in objectsToDeactivateOnHover)
        {
            item.SetActive(true);
        }

        if (!cursorImage)
            cursorImage = cursorGraphic.GetComponent<Image>();

        cursorImage.color = originalColor;
        cursorGraphic.SetActive(false);
    }


}
