using System;
using System.IO;
using UnityEngine;
using Komodo.AssetImport;

namespace GLTFast.Loading {

    public class KomodoDownloadProvider : IDownloadProvider {
        public IDownload Request(Uri filename) {
            var req = new KomodoDownload(filename);
            return req;
        }

        public ITextureDownload RequestTexture(Uri filename) {
            var req = new KomodoTextureDownload(filename);
            return req;
        }
    }

    public class KomodoDownload : IDownload {

        protected byte[] fileData;

        public KomodoDownload() {}

        public KomodoDownload(Uri filename) {
            Init(filename);
        }

        protected void Init(Uri filename) {
            //Debug.Log($"trying to load {filename.LocalPath}");
            try {
                fileData = File.ReadAllBytes(filename.LocalPath);
            }
            catch (System.Exception e) {
                Debug.LogError($"Error trying to read all bytes of {filename.LocalPath}. Skipping and returning an empty byte array.");

                Debug.LogError($"{e.Message}");

                fileData = new byte[1];
            }
        }

        //Some stuff to try and make GLTFast happy.
        public object Current { get { return new object(); } } //TODO fix this
        public bool MoveNext() { return false; } //TODO check this
        public void Reset() {}
        public bool success => true; //we have already downloaded the file with our AssetDownloaderAndLoader routine. 

        public string error { get { return "[Required text to fulfill defintion of IDownloadProvider]"; } }
        public byte[] data 
        { 
            get 
            { 

                return fileData; 
            } 
        }
        public string text { get { return "[Required text to fulfill defintion of IDownloadProvider]"; } }
        public bool? isBinary
        {
            get
            {
                if (success)
                {
                    return true;
                    //GLB supported only for now.
                    //TODO(Brandon): add GLTF support
                }
                return null;
            }
        }
    }

    public class KomodoTextureDownload : KomodoDownload, ITextureDownload {

        public KomodoTextureDownload():base() {}
        public KomodoTextureDownload(Uri url):base(url) {}

        public KomodoTextureDownload(Uri url, bool nonReadable) {
            Init(url,nonReadable);
        }

        protected void Init(Uri url, bool nonReadable) {
        }

        //TODO fix this
        public Texture2D texture {
            get {
                return new Texture2D(1, 1, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
            }
        }
    }
}