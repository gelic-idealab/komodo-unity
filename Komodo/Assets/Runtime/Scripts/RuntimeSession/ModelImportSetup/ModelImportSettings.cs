using UnityEngine;

namespace Komodo.AssetImport
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "ModelImportSettings", menuName = "ModelImportSettings", order = 0)]
    public class ModelImportSettings : ScriptableObject
    {
        [Header("Size Threshold of our loaded object")]
        public float fitToScale = 2;

        [Header("Height Reference to offset our import objects on Y")]
        public float spawnHeight = 0;

        [Header("Create and set a bounding box for wholeobjects and decomposed ones. Note: Only top level collider is created for skinned mesh renderers")]
        public bool doSetUpColliders = true;

        [Header("Add reference to use imported object in network")]
        public bool isNetworked = true;
    }
}