using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEditor;

namespace Komodo.Runtime
{
    /// <summary>
    /// A structure for hodling scene information.
    /// </summary>
    [System.Serializable]
    public struct SceneReference
    {
        /// <summary>
        /// Name of the scene.
        /// </summary>
        public string name;

        /// <summary>
        /// The index of the scene; starting from 0.
        /// </summary>
        public int sceneIndex;
    }

    /// <summary>
    /// The list of scene. Although this class is being used at some places, it does not change anything to Komodo yet since the change-scene feature is not implemented yet.
    /// </summary>
    [CreateAssetMenu(fileName = "Scene_List", menuName = "new_Scene_List", order = 0)]
    public class SceneList : ScriptableObject
    {
        /// <summary>
        /// A list that store scenes to be shown in Komodo.
        /// </summary>
        [Header("Add scenes to show in game to this list")]
        public List<Object> scenes;

        /// <summary>
        /// Adding scenes to the above list updates the list shown in game, according to the below field
        /// </summary>
        [Tooltip("Adding scenes to the above list updates the list shown in game, according to the below field")]
        public List<SceneReference> references;

        /// <summary>
        /// check if the user change the asset to change available scenes displayed
        /// </summary>
        public void OnValidate()
        {
            references.Clear();

            int currentScene = 0;

            foreach (var item in scenes)
            {
                references.Add(new SceneReference
                {
                    name = item.name,
                    sceneIndex = currentScene,
                });

                ++currentScene;
            }
        }
    }
}