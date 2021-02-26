// Copyright 2020 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.IO;
using System.Collections;
using UnityEngine;

namespace GLTFast
{
    public class KomodoGLTFAsset : GltfAssetBase
    {
        [Tooltip("file location to load the glTF from.")]
        public string location;

        private System.Action<GameObject> callback;

        private Loading.IDownloadProvider downloadProvider;

        public void Load(string location, System.Action<GameObject> callback) {
            this.location = location;
            this.callback = callback;

            downloadProvider = new Loading.KomodoDownloadProvider();

            base.Load(location, downloadProvider);
        }

        protected override void OnLoadComplete(bool success) {
            if (!success) {
                Debug.LogError("Error loading GLTF with GLTFast.", gameObject);
            }

            StartCoroutine(Instantiate(gameObject));

            base.OnLoadComplete(success);
        }

        /**
        * Ask GLTFast to instantiate a GameObject, wait for it to finish, 
        * and then call our model setup callback when it is finished.
        */
        public IEnumerator Instantiate (GameObject result) {

            yield return new WaitUntil ( () => {
                bool success = gLTFastInstance.InstantiateGltf(result.transform);
                
                //Debug.Log($"Instantiate {gameObject.name}: {success}");

                return success;
            });

            if (callback == null) {
                Debug.LogWarning("No post-processing will be done on the imported model.");

                yield break;
            }

            callback(result);
        }
    }
}
