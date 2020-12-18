using Siccity.GLTFUtility;
using UnityEngine;

public class SiccityGLTFLoader : AssetDownloaderAndLoader
{
    public override GameObject LoadLocalFile(string localFilename)
    {
        return Importer.LoadFromFile(localFilename);
    }
}
