using System.Collections.Generic;
using Komodo.AssetImport;

namespace Komodo.Runtime
{
    /// <summary>
    /// The struct that contains detailed information about a session.
    /// </summary>
    [System.Serializable]
    public struct SessionDetails
    {
        /// <summary>
        /// A list that contains data of model that will be imported. 
        /// </summary>
        public List<ModelDataTemplate.ModelImportData> assets;
        
        /// <summary>
        /// The build version.
        /// </summary>
        public string build;

        /// <summary>
        /// The course ID.
        /// </summary>
        public int course_id;

        /// <summary>
        /// Location of the session.
        /// </summary>
        public string create_at;

        /// <summary>
        /// Description of the session.
        /// </summary>
        public string description;

        /// <summary>
        /// The end time of the session.
        /// </summary>
        public string end_time;

        /// <summary>
        /// The ID of the session.
        /// </summary>
        public int session_id;

        /// <summary>
        /// The name of the session.
        /// </summary>
        public string session_name;

        /// <summary>
        /// The start time of the session.
        /// </summary>
        public string start_time;

        /// <summary>
        /// A list of users in the session.
        /// </summary>
        public List<User> users;
    }
}