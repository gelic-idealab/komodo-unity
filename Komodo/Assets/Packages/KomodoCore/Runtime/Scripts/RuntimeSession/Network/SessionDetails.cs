using System.Collections.Generic;
using Komodo.AssetImport;

namespace Komodo.Runtime
{
    [System.Serializable]
    public struct SessionDetails
    {
        public List<ModelDataTemplate.ModelImportData> assets;
        public string build;
        public int course_id;
        public string create_at;
        public string description;
        public string end_time;
        public int session_id;
        public string session_name;
        public string start_time;
        public List<User> users;
    }
}