using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace Komodo.Editor
{
    //copied from webxrmenu.cs
    public class KomodoEditorMenu : UnityEditor.EditorWindow
    {
        public TextAsset packageReference;

//        public static string packageDir = ;

        [MenuItem("Window/Komodo/Copy WebGLTemplates")]
        static void CopyWebGLTemplates()
        {
            if (!EditorUtility.DisplayDialog("Copy WebGLTemplates", "This action might override your WebGLTemplates folders and files. Make sure to have a backup", "Continue", "Cancel"))
            {
                return;
            }
            // Ugly hack to get package path by asset reference
            KomodoEditorMenu KwebXRMenu = (KomodoEditorMenu)ScriptableObject.CreateInstance("KomodoEditorMenu");

            //custom to find package, since for some reason it looses track of package reference in the editor when adding package to project
            var script = MonoScript.FromScriptableObject(KwebXRMenu);
            var path = Directory.GetParent(AssetDatabase.GetAssetPath(script));
            string packageAssetFullPath = path.FullName;// AssetDatabase.GetAssetPath(script);//Path.GetFullPath(AssetDatabase.GetAssetPath(KwebXRMenu.packageReference));


            DestroyImmediate(KwebXRMenu);
            string packagePath = Path.GetDirectoryName(packageAssetFullPath);

            if (packagePath == null)
            {
                Debug.LogError("Copy failed, could not find package");
                return;
            }
            CopyFolder(Path.Combine(packagePath, "Hidden~"), Application.dataPath);
            AssetDatabase.Refresh();
        }

        // modified version of https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        private static void CopyFolder(string sourceFolderName, string destFolderName)
        {
            DirectoryInfo directory = new DirectoryInfo(sourceFolderName);

            DirectoryInfo[] directories = directory.GetDirectories();
            if (!Directory.Exists(destFolderName))
            {
                Directory.CreateDirectory(destFolderName);
            }

            FileInfo[] files = directory.GetFiles();
            // In the source repository, it'll throw an error,
            // as it'll try to copy from the same file, to the same file (symlink)
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destFolderName, file.Name);
                try
                {
                    file.CopyTo(temppath, true);
                }
                catch (IOException exception)
                {
                    Debug.LogError(exception.Message);
                }
            }

            foreach (DirectoryInfo subFolder in directories)
            {
                string temppath = Path.Combine(destFolderName, subFolder.Name);
                CopyFolder(subFolder.FullName, temppath);
            }
        }
    }
}
