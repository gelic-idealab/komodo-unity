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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Komodo.AssetImport;
using Komodo.Utilities;

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

        public bool doForceMobileMemoryLimit = false;

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;
        }

        public List<ModelFile> localFiles;

        public int modelsToRetrieve;

        public int modelsToInstantiate;

        private IEnumerator Start()
        {
            //WebGLMemoryStats.LogMoreStats("ModelImportInitializer.Start Setup BEFORE");

            if (loader == null) {
                throw new System.Exception("Missing loader");
            }
            if (modelData == null)
            {
                throw new System.Exception("Missing model data");
            }
            if (progressDisplay == null) 
            {
                throw new System.Exception("Missing progress display");
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

            //WebGLMemoryStats.LogMoreStats("ModelImportInitializer.Start Setup AFTER");

            WebGLMemoryStats.ChooseMemoryLimitForDevice(doForceMobileMemoryLimit);

            localFiles = new List<ModelFile>();

            //since we have coroutines and callbacks, we should keep track of the number of models that have finished retrieving. 
            GameStateManager.Instance.isAssetImportFinished = false;
            modelsToRetrieve = modelData.models.Count;

            //Wait until all objects are finished loading
            StartCoroutine(RetrieveModelFiles());

            //Debug.Log("Model files finished loading.");

            yield return new WaitUntil(() =>
            {
                //Debug.Log($"{GameStateManager.Instance.modelsToInstantiate} models left to instantiate.");
                return modelsToRetrieve == 0;
            });

            //since we have coroutines and callbacks, we should keep track of the number of models that have finished instantiating. 
            GameStateManager.Instance.isAssetImportFinished = false;
            modelsToInstantiate = modelData.models.Count;

            //Wait until all objects are finished loading
            StartCoroutine(InstantiateModels());

            //Debug.Log("Finished instantiating models.");

            yield return new WaitUntil(() =>
            {
                //Debug.Log($"{GameStateManager.Instance.modelsToInstantiate} models left to instantiate.");
                return modelsToInstantiate == 0;
            });

            GameStateManager.Instance.isAssetImportFinished = true;

        }

        /** 
        * Download uncached or load cached model files.
        */ 
        public IEnumerator RetrieveModelFiles()
        {
            
            Text text = UIManager.Instance.initialLoadingCanvasProgressText;

            for (int i = 0; i < modelData.models.Count; i += 1 )
            {
                //Debug.Log($"retrieving model #{i}");

                int menuIndex = i;

                var model = modelData.models[i];
                VerifyModelData(model);

                progressDisplay.text = $"{model.name}: Retrieving";

                //download or load our model
                yield return loader.GetFileFromURL(model, progressDisplay, menuIndex, localFile => {
                    localFiles.Add(localFile);
                    modelsToRetrieve -= 1;
                });
            }
        }

        public IEnumerator InstantiateModels()
        {
            for (int i = 0; i < localFiles.Count; i += 1 )
            {
                int menuIndex = i;

                var model = modelData.models[i];
                VerifyModelData(model);

                yield return loader.TryLoadLocalFile(localFiles[i].location, localFiles[i].name, localFiles[i].size, progressDisplay, gObject =>
                {
                //Debug.Log($"instantiating model #{menuIndex}");
                //Debug.Log($"{modelData.name}");
                    //WebGLMemoryStats.LogMoreStats($"ModelImportPostProcessor.SetUpGameObject {model.name} BEFORE");
                //set up gameObject properties for a Komodo session 
                GameObject komodoImportedModel = ModelImportPostProcessor.SetUpGameObject(menuIndex, model, gObject, settings ?? null);
                    //WebGLMemoryStats.LogMoreStats($"ModelImportPostProcessor.SetUpGameObject {model.name} AFTER");

                 //   Debug.Log(komodoImportedModel.name);
                //set it as a child of the imported models list
                komodoImportedModel.transform.SetParent(list.transform, true);

                    modelsToInstantiate -= 1;
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
