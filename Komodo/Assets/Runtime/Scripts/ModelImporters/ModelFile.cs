using System;

namespace Komodo.AssetImport
{
    public class ModelFile 
    {
        public string location;
        public string name;
        public ulong size;

        public ModelFile(string location, string name, ulong size) {
            this.location = location;
            this.name = name;
            this.size = size;
        }
    }
}