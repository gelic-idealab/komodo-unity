// University of Illinois/NCSA
// Open Source License
// http://otm.illinois.edu/disclose-protect/illinois-open-source-license

// Copyright (c) 2020 Grainger Engineering Library Information Center.  All rights reserved.

// Developed by: IDEA Lab
//               Grainger Engineering Library Information Center - University of Illinois Urbana-Champaign
//               https://library.illinois.edu/enx

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal with
// the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to
// do so, subject to the following conditions:
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimers.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimers in the documentation
//   and/or other materials provided with the distribution.
// * Neither the names of IDEA Lab, Grainger Engineering Library Information Center,
//   nor the names of its contributors may be used to endorse or promote products
//   derived from this Software without specific prior written permission.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE
// SOFTWARE.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Komodo.AssetImport;
using Komodo.Utilities;
using System.Collections.Generic;

namespace Komodo.Runtime
{

    /// <summary>
    /// To Invoke our process of downloading and setting up imported objects to be used in session
    /// </summary>
    public class ModelImportInitializer : SingletonComponent<ModelImportInitializer>
    {
        public static ModelImportInitializer Instance
        {
            get { return ((ModelImportInitializer)_Instance); }
            set { _Instance = value; }
        }
        //download progress displasy
        public Text progressDisplay;

        public ModelDownloaderAndLoader loader;

        //downloadable models list
        public ModelDataTemplate modelData;

        public ModelImportSettings settings;

        //root object of runtime-imported models
        private GameObject list;

        private string listName = "Imported Models";

        public List<NetworkedGameObject> networkedGameObjects = new List<NetworkedGameObject>();

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;
        }


        private IEnumerator Start()
        {
            if (loader == null)
            {
                throw new System.Exception("Missing loader");
            }
            if (modelData == null)
            {
                throw new System.Exception("Missing model data");
            }

            //create root parent in scene to contain all imported models
            list = new GameObject(listName);
            list.transform.parent = transform;

            //initialize a list of blank gameObjects so we can instantiate models even if they load out-of-order. 
            for (int i = 0; i < modelData.models.Count; i += 1)
            {
                NetworkedGameObject netObject = new NetworkedGameObject();
                networkedGameObjects.Add(netObject);
            }

            //since we have coroutines and callbacks, we should keep track of the number of models that have finished instantiating. 
            GameStateManager.Instance.modelsToInstantiate = modelData.models.Count;

            WebGLMemoryStats.SetMemoryLimitForDevice();

            //Wait until all objects are finished loading
            yield return StartCoroutine(LoadAllGameObjectsFromURLs());

            //Debug.Log("Models finished importing.");

            yield return new WaitUntil(() =>
            {
                //Debug.Log($"{GameStateManager.Instance.modelsToInstantiate} models left to instantiate.");
                return GameStateManager.Instance.modelsToInstantiate == 0;
            });

            GameStateManager.Instance.isAssetImportFinished = true;

        }

        public IEnumerator LoadAllGameObjectsFromURLs()
        {
            //set default text if there is no uimanager in scene
            //Text text = null;
            //if (!UIManager.IsAlive)
            //    text = gameObject.AddComponent<Text>();
            //else
            Text text = UIManager.Instance.initialLoadingCanvasProgressText;

            //wait for each loaded object to process
            for (int i = 0; i < modelData.models.Count; i += 1)
            {
                int menuIndex = i;

                var model = modelData.models[i];
                VerifyModelData(model);


                //download or load our model
                yield return loader.GetFileFromURL(model, text, menuIndex, gObject =>
                {
                //Debug.Log($"instantiating model #{menuIndex}");
                //Debug.Log($"{modelData.name}");

                //set up gameObject properties for a Komodo session 
                GameObject komodoImportedModel = ModelImportPostProcessor.SetUpGameObject(menuIndex, model, gObject, settings ?? null);

                 //   Debug.Log(komodoImportedModel.name);
                //set it as a child of the imported models list
                komodoImportedModel.transform.SetParent(list.transform, true);

                    GameStateManager.Instance.modelsToInstantiate -= 1;

                });
            }
        }

        public void VerifyModelData(ModelDataTemplate.ModelImportData data)
        {
            if (string.IsNullOrEmpty(data.name) || string.IsNullOrWhiteSpace(data.name))
            {
                throw new System.Exception("model name cannot be empty.");
            }

            if (string.IsNullOrEmpty(data.url) || string.IsNullOrWhiteSpace(data.url))
            {
                throw new System.Exception("Asset Data URL cannot be empty.");
            }

            if (data.scale < 0.001 && data.scale > -0.001)
            {
                Debug.LogWarning($"Scale of imported model {data.name} is between -0.001 and 0.001. Results may not be as expected.");
            }

            if (data.scale > 1000 || data.scale < -1000)
            {
                Debug.LogWarning($"Scale of imported model {data.name} is above 1000 or below -1000. Results may not be as expected.");
            }
        }
    }
}
