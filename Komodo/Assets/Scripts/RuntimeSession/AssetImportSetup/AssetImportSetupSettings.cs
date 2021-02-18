using UnityEngine;

namespace Komodo.Runtime
{
    //method to set custom setup 
    [System.Serializable]
    [CreateAssetMenu(fileName = "AssetImportSettings", menuName = "AssetImportSettings", order = 0)]
    public class AssetImportSetupSettings : ScriptableObject
    {
        [Header("Size Threshold of our loaded object")]
        public float fitToScale = 2;

        [Header("Height Reference to offset our import objects on Y")]
        public float assetSpawnHeight = 0;

        [Header("Create and set a bounding box for wholeobjects and decomposed ones. Note: Only top level collider is created for skinned mesh renderers")]
        public bool doSetUpColliders = true;

        [Header("Add reference to use imported object in network")]
        public bool isNetworked = true;
    }
}