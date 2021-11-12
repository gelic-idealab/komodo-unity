using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Komodo.Utilities;
using UnityEngine.Events;

namespace Komodo.Runtime
{
    public class SceneManagerExtensions : SingletonComponent<SceneManagerExtensions>
    {
        public static SceneManagerExtensions Instance
        {
            get { return ((SceneManagerExtensions)_Instance); }
            set { _Instance = value; }
        }

        [Header("Scenes To Make Available")]

        public SceneList sceneList;

        [HideInInspector] public List<Button> sceneButtons;

        [HideInInspector] public List<string> loadedAdditiveScenes = new List<string>();

        List<AsyncOperation> asyncOperations = new List<AsyncOperation>();

        [HideInInspector] public Scene mainScene;

        [HideInInspector] public Scene additiveScene;

        public UnityEvent onNewSceneLoaded;

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;
        }

        public void Start()
        {
            if (mainScene == null)
            {
                SceneManager.SetActiveScene(mainScene);
            }

            mainScene = SceneManager.GetActiveScene();

            if (sceneList == null) {
                throw new System.Exception("You must assign a scene list in SceneManagerExtensions.cs");
            }

            if (sceneList.scenes.Count == 0)
            {
                Debug.LogError("No Scenes available to activate. Please check your scene references.");
            }
            else
            {
                SelectScene(0);
            }
        }

        /// <summary>
        /// Select which scene should be rendered by providing data of the scenereference and appropriate button of UI
        /// </summary>
        /// <param name="sceneRef"></param>
        /// <param name="button"></param>
        public void OnPressSceneButton(SceneReference sceneRef, Button button)
        {
            NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
            {
                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
                targetEntity_id = sceneRef.sceneIndex,
                interactionType = (int)INTERACTIONS.CHANGE_SCENE,//cant covert since it gives 0 at times instead of the real type?
            });

            SelectScene(sceneRef.sceneIndex);
        }

        /// <summary>
        /// Load a new scene additively and remove the other ones for this client only
        /// </summary>
        /// <param name="sceneID"></param>
        public void SelectScene(int sceneID) => StartCoroutine(CoroutineForSelectingScene(sceneID));

        public IEnumerator CoroutineForSelectingScene(int sceneID)
        {
            //check if we are currently loading any scenes. If so, wait for them to be finished to start loading a new one
            foreach (var item in asyncOperations)
            {
                yield return new WaitUntil(() => item.isDone);
            }

            //clear our loading list
            asyncOperations.Clear();

            if (IsSceneAlreadyLoaded(sceneID))
            {
                yield break;
            }

            //unload all present scenes except main one, which is at index 0
            for (int i = 1; i < SceneManager.sceneCount; i += 1)
            {
                asyncOperations.Add(SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i)));
            }

            //clear the list
            loadedAdditiveScenes.Clear();

            if (sceneID >= sceneList.references.Count) {
                Debug.LogError("sceneID was out of bounds. Make sure your sceneList object is initialized properly");

                yield break;
            }

            string sceneName = sceneList.references[sceneID].name;

            //add the scene that is being loaded to our list keeping track of our loaded scenes and its async process
            loadedAdditiveScenes.Add(sceneName);

            asyncOperations.Add(SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive));

            UpdateSceneButtons(sceneID);

            //wait for our loading process to finish on our new loading scene
            foreach (var item in asyncOperations)
            {
                yield return new WaitUntil(() => item.isDone);
            }

            //////make our new scene as the active scene to use its light settings
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            //Get a reference to our added scene
            additiveScene = SceneManager.GetActiveScene();

            onNewSceneLoaded.Invoke();
        }

        private bool IsSceneAlreadyLoaded (int sceneID) {
            //Go through our list of scene references and check if we are not loading a scene already loaded. If so, break
            foreach (string loadedScene in loadedAdditiveScenes)
            {
                foreach (SceneReference sceneInList in sceneList.references)
                {
                    if (sceneInList.name == loadedScene && sceneInList.sceneIndex == sceneID)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void UpdateSceneButtons (int sceneID) {
            //////////////////
            //enable all scene buttons
            foreach (var button in sceneButtons)
            {
                button.interactable = true;
            }

            if (sceneButtons.Count > 0 && sceneButtons[sceneID] != null)
            {
                //disable the current scene's button
                sceneButtons[sceneID].interactable = false;
            }
        }

        public void LoadSceneAdditiveAsync(string scene, bool mergeScenes = false)
        {
            AsyncOperation aSync = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

            if (mergeScenes)
            {
                StartCoroutine(MergeScenes(aSync, SceneManager.GetSceneByName(scene), SceneManager.GetActiveScene()));
            }
        }

        public void LoadSceneAdditiveAsync(Scene scene, bool mergeScenes = false)
        {
            AsyncOperation aSync = SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);

            if (mergeScenes)
            {
                StartCoroutine(MergeScenes(aSync, SceneManager.GetSceneByName(scene.name), SceneManager.GetActiveScene()));
            }
        }

        public IEnumerator MergeScenes(AsyncOperation async, Scene source, Scene target)
        {
            yield return new WaitUntil(() => async.isDone);
            SceneManager.MergeScenes(source, target);
        }
    }
}
