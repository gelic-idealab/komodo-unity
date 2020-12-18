using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEditor;

[System.Serializable]
public struct SceneReference
{
    public string name;
    public int sceneIndex;
}

[CreateAssetMenu(fileName = "Scene_List", menuName = "new_Scene_List", order = 0)]
public class SceneList : ScriptableObject
{
    [Header("Add scenes to show in game to this list")]
    public List<Object> sceneList;

    [Tooltip("Adding scenes to the above list updates the list shown in game, according to the below field")]
    public List<SceneReference> sceneReferenceList;

    //check if the user change the asset to change available scens displayed
    public void OnValidate()
    {
        sceneReferenceList.Clear();

        int currentScene = 0;

        foreach (var item in sceneList)
        {
            sceneReferenceList.Add(new SceneReference
            {
                name = item.name,
                sceneIndex = currentScene,
            });

            ++currentScene;
        }
    }
}