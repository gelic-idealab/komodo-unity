using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinderBox : MonoBehaviour
{
    public List<Camera> cameras;

    public string layerToToggle = "Walkable";

    public GameObject box;

    public void Awake ()
    {
        if (!box) 
        {
            throw new System.Exception("You must set Box");
        }
    }

    public void Start ()
    {
        if (!box) 
        {
            throw new System.Exception("You must set Box");
        }
    }

    /**
    * otherTransform: location that blinder box should be centered on.
    */
    public void BeginBlinding (Transform otherTransform) 
    {
        transform.position = otherTransform.position;

        box.SetActive(true);

        HideLayerObjects();
    }

    public void EndBlinding () 
    {
        ShowLayerObjects();

        box.SetActive(false);
    }

    public void HideLayerObjects ()
    {
        for (int i = 0; i < cameras.Count; i += 1)
        {
            DisableLayer(cameras[i], layerToToggle);
        }
    }

    public void ShowLayerObjects ()
    {
        for (int i = 0; i < cameras.Count; i += 1)
        {
            EnableLayer(cameras[i], layerToToggle);
        }
    }

    private void EnableLayer (Camera camera, string name)
    {
        camera.cullingMask |= (1 << LayerMask.NameToLayer(name));
    }

    private void DisableLayer (Camera camera, string name)
    {
        camera.cullingMask &= ~(1 << LayerMask.NameToLayer(name));
    }
}
