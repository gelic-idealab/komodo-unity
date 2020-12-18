using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerExtensions : SingletonComponent<SceneManagerExtensions>
{
    public static SceneManagerExtensions Instance
    {
        get { return ((SceneManagerExtensions)_Instance); }
        set { _Instance = value; }
    }

    public void LoadSceneAdditiveAsync(string scene, bool mergeScenes = false)
    {

       AsyncOperation aSync = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        if (mergeScenes)
            StartCoroutine(MergeScenes(aSync, SceneManager.GetSceneByName(scene), SceneManager.GetActiveScene()));
            
    }
    public void LoadSceneAdditiveAsync(Scene scene, bool mergeScenes = false)
    {

        AsyncOperation aSync = SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);

        if (mergeScenes)
            StartCoroutine(MergeScenes(aSync, SceneManager.GetSceneByName(scene.name), SceneManager.GetActiveScene()));

    }
    public IEnumerator MergeScenes(AsyncOperation async, Scene source, Scene target)
    {
        yield return new WaitUntil(() => async.isDone);
        SceneManager.MergeScenes(source, target);

    }
}
