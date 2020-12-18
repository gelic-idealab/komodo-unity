using UnityEngine;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class SetCustomRenderQueue : MonoBehaviour
{

    public UnityEngine.Rendering.CompareFunction comparison = UnityEngine.Rendering.CompareFunction.Always;

    [SerializeField] private bool hasUIText;
    [SerializeField] private bool hasUIImage;
    [SerializeField] private bool hasRawImage;
    [SerializeField] private bool hasMesh = false;
    public bool apply = false;

    private void Start()
    {
        if (apply)
        {
            apply = false;
            Debug.Log("Updated material val");

            if (hasUIImage)
            {

                Image image = GetComponent<Image>();
                Material existingGlobalMat = image.materialForRendering;
                Material updatedMaterial = new Material(existingGlobalMat);
                updatedMaterial.SetInt("unity_GUIZTestMode", (int)comparison);
                image.material = updatedMaterial;

            }

            if (hasUIText)
            {
                Text text = GetComponent<Text>();
                Material existingGlobalMat = text.materialForRendering;
                Material updatedMaterial = new Material(existingGlobalMat);
                updatedMaterial.SetInt("unity_GUIZTestMode", (int)comparison);
                text.material = updatedMaterial;

            }
            if (hasRawImage)
            {
                RawImage rawImage = GetComponent<RawImage>();
                Material existingGlobalMat = rawImage.materialForRendering;
                Material updatedMaterial = new Material(existingGlobalMat);
                updatedMaterial.SetInt("unity_GUIZTestMode", (int)comparison);
                rawImage.material = updatedMaterial;

            }
            if (hasMesh)
            {
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                Material existingGlobalMat = meshFilter.GetComponent<Material>();
                Material updatedMaterial = new Material(existingGlobalMat);
                updatedMaterial.SetInt("_Mode", (int)comparison);
                existingGlobalMat = updatedMaterial;

            }
        }
    }
}

