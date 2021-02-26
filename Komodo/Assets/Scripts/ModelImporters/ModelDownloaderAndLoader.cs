#pragma warning disable 649
using System;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

namespace Komodo.AssetImport
{
    public class ModelDownloaderAndLoader : MonoBehaviour
    {
        //Reference for the latest loaded GameObject
        protected GameObject loadedGameObject;
        public string urlBase = "https://vrcat-assets.s3.us-east-2.amazonaws.com/";

        public bool logging = false;

        /**
        * Returns an array: first value is the GUID; second value is the file name and extension.
        */
        private string[] getModelParameters(string url) {
            string urlSuffix = url.Substring(urlBase.Length);
            string[] modelParams = urlSuffix.Split('/');
            return modelParams;
        }
        
        /** 
        * Creates a directory to store the model in and then passes model data onto a download coroutine.
        */
        public IEnumerator GetFileFromURL(ModelDataTemplate.ModelImportData modelData, Text progressDisplay, int index, System.Action<GameObject> callback)
        {
            //Gets guid and filename and extension       
            string[] modelParams = getModelParameters(modelData.url); 
            var guid = modelParams[0];
            var fileNameAndExtension = modelParams[1];

            //Create a unique directory based on the guid
            var modelDirectoryLocation = $"{Application.persistentDataPath}/{guid}";

            //Debug.Log($"Storing model at {modelDirectoryLocation}");

            Directory.CreateDirectory(modelDirectoryLocation);

            var modelFileLocation = $"{modelDirectoryLocation}/{fileNameAndExtension}";

            if (!File.Exists(modelFileLocation)) {
                yield return StartCoroutine(DownloadFile(modelData, progressDisplay, index, modelFileLocation, callback));
                yield break;
            }

            FileInfo modelInfo = new FileInfo(modelFileLocation);

            Debug.Log($"{modelData.name} cached. Loading immediately.");
            
            progressDisplay.text = $"{modelData.name} cached. Loading immediately.";

            ulong modelSize = (ulong) modelInfo.Length;

            TryLoadLocalFile(modelFileLocation, modelData.name, modelSize, callback);
        }

        /** 
        * Downloads a file to a local path, then loads the file
        */
        private IEnumerator DownloadFile(ModelDataTemplate.ModelImportData modelData, Text progressDisplay, int index, string localPathAndFilename, System.Action<GameObject> callback = null)
        {
            UnityWebRequest fileDownloader = UnityWebRequest.Get(modelData.url);

            //get size of model first to allocate what is needed
            long modelSize = 0;
            yield return StartCoroutine(GetFileSize(modelData.url, (size) =>
                {
                    modelSize = size;
                })
            );

            //set our model download settings
            fileDownloader.method = UnityWebRequest.kHttpVerbGET;
            using (var dh = new DownloadHandlerFile(localPathAndFilename)) 
            {
                dh.removeFileOnAbort = true;
                fileDownloader.downloadHandler = dh;
                fileDownloader.SendWebRequest();

                while (!fileDownloader.isDone)
                {
                    progressDisplay.text = $"Downloading {modelData.name}: {fileDownloader.downloadProgress.ToString("P")}";

                    yield return null;
                }
            }

            System.GC.Collect();

            //Debug.Log($"Successfully downloaded model {modelData.name}, size {fileDownloader.downloadedBytes} bytes.");

            if (fileDownloader.result == UnityWebRequest.Result.ConnectionError || fileDownloader.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(fileDownloader.error);
            }

            //Debug.Log($"Successfully downloaded asset {assetData.name}, size {fileDownloader.downloadedBytes} bytes.");

            ulong downloadedSize = fileDownloader.downloadedBytes;

            fileDownloader = null;

            TryLoadLocalFile(localPathAndFilename, modelData.name, downloadedSize, callback);
        }

        public void TryLoadLocalFile (string localPathAndFileName, string name, ulong downloadedSize, System.Action<GameObject> callback) {
            WebGLMemoryStats.LogMoreStats("Checking memory before trying to load model...");

            bool canLoad = WebGLMemoryStats.HasEnoughMemoryToLoadBytes((long) downloadedSize);

            if (!canLoad)
            {
                Debug.LogError($"Size of {name}: {WebGLMemoryStats.ToRoundedMB(downloadedSize, 2)}MB.\nSize is too large.");

                //TODO(Brandon):
                // delete local file? to free it from memory? or let OS GC do that?
                // download placeholder / error file then call callback on it
                // OR call callback with a placeholder error file
                // create a button that lets you TRY to reload it?
                callback(new GameObject("Error: Model Too Large"));
                return;
            }
            
            Debug.Log($"Size of {name}: {WebGLMemoryStats.ToRoundedMB(downloadedSize, 2)}MB.\nProceeding with loading.");
            
            LoadLocalFile(localPathAndFileName, callback);
        }

        /**
        * Use an inherited class to replace this function.
        * It should load a local 3D model file from disk into memory as a  
        * GameObject and then call the passed callback on that GameObject.
        * If the callback is null, it should do nothing.
        */
        public virtual void LoadLocalFile(string localFilename, System.Action<GameObject> callback) { }

        public static IEnumerator GetFileSize(string url, Action<long> callback)
        {
            UnityWebRequest request = UnityWebRequest.Head(url);
            yield return request.SendWebRequest();
            string size = request.GetResponseHeader("Content-Length");

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error while getting length: " + request.error);
                callback?.Invoke(-1);
            }
            else
                callback?.Invoke(Convert.ToInt64(size));
        }
    }
}
#pragma warning restore 649


