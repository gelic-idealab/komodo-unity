using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    //For handling different type of text between clients
    public enum STRINGTYPE
    {
        TUTORIAL,
        CLIENT_NAME,
        SPEECH_TO_TEXT,
    }

    public struct SpeechToTextSnippet
    {
        public int target;
        public int stringType;
        public string text;
    }

    public struct SpeechToText
    {
        public int session_id;
        public int client_id;
        public string text;
        public string type;
        public int ts;
    }
}
