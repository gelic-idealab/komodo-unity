//https://forum.unity.com/threads/how-to-store-generic-delegates.372530/

using Komodo.Utilities;
using System;
using System.Collections.Generic;

namespace Komodo.Runtime
{

    public class GlobalMessageManager : SingletonComponent<GlobalMessageManager>
    {
      
        public static GlobalMessageManager Instance
        {
            get { return ((GlobalMessageManager)_Instance); }
            set { _Instance = value; }
        }

        public  IDictionary<string, List<Action<string>>> subscribers = new Dictionary<string, List<Action<string>>>();

        public List<string> registeredMessages = new List<string>();

        public void Subscribe(string messageTypeName, Action<string> callback) 
        {
            // var type = typeof(T);

            if (subscribers.ContainsKey(messageTypeName))
            {
                subscribers[messageTypeName].Add(callback);
            }
            else
            {
                registeredMessages.Add(messageTypeName);

                subscribers[messageTypeName] = new List<Action<string>>();
                subscribers[messageTypeName].Add(callback);
            }
        }


        //public void Send(string messageTypeName, string param) 
        //{
        //    //var type = typeof(T);

        //    if (subscribers.ContainsKey(messageTypeName))
        //    {
        //        List<Action<string>> callbacks = subscribers[messageTypeName];

        //        for (int i = 0; i < callbacks.Count; i++)
        //        {
        //            Action<string> callback = (Action<string>)callbacks[i];

        //            callback(param);
        //        }
        //    }
        //}

        public void UnSubscribe(string messageTypeName, Action<string> callback) 
        {
            //var type = typeof(T);

            if (subscribers.ContainsKey(messageTypeName))
            {

                List<Action<string>> callbacks = subscribers[messageTypeName];

                for (int i = 0; i < callbacks.Count; i++)
                {

                    Action<string> tmpCallback = (Action<string>)callbacks[i];

                    if (tmpCallback == callback)
                    {

                        callbacks.RemoveAt(i);

                        break;
                    }
                }
            }
        }



    }

}
