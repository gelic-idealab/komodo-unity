using UnityEngine;

//method to set custom setup 
[System.Serializable]
[CreateAssetMenu(fileName = "AssetImportSettings", menuName = "AssetImportSettings", order = 0)]
public class AssetImportSetupSettings : ScriptableObject
{
    [Header("Size Threshold of our loaded object")]
    public float defaultSizeToLoadGO = 2;

    [Header("Height Reference to offset our import objects on Y")]
    public float environmentHeight = 0;

    [Header("Create and set a bounding box for wholeobjects and decomposed ones. Note: Only top level collider is created for skinned mesh renderers")]
    public bool setUpColliders = true;

    [Header("Add reference to use imported object in network")]
    public bool setupNetRegisterGO = true;

}