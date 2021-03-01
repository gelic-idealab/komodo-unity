//https://forum.unity.com/threads/how-to-store-generic-delegates.372530/

using Komodo.Utilities;
using System;
using System.Collections.Generic;


public class GlobalMessageManager : SingletonComponent<GlobalMessageManager> 
{
    private static IDictionary<string, List<object>> subscribers = new Dictionary<string, List<object>>();

    public static void Subscribe<T>(string messageTypeName, Action<T> callback) where T : struct
    {
       // var type = typeof(T);

        if (subscribers.ContainsKey(messageTypeName))
        {
            subscribers[messageTypeName].Add(callback);
        }
        else
        {
            subscribers[messageTypeName] = new List<object>();
            subscribers[messageTypeName].Add(callback);
        }
    }

 
    public static void Send<T>(string messageTypeName, T param) where T : struct
    {
        //var type = typeof(T);

        if (subscribers.ContainsKey(messageTypeName))
        {
            List<object> callbacks = subscribers[messageTypeName];

            for (int i = 0; i < callbacks.Count; i++)
            {
                Action<T> callback = (Action<T>)callbacks[i];

                callback(param);
            }
        }
    }

    public static void UnSubscribe<T>(string messageTypeName, Action<T> callback) where T : struct
    {
        //var type = typeof(T);

        if (subscribers.ContainsKey(messageTypeName))
        {

            List<object> callbacks = subscribers[messageTypeName];

            for (int i = 0; i < callbacks.Count; i++)
            {

                Action<T> tmpCallback = (Action<T>)callbacks[i];

                if (tmpCallback == callback)
                {

                    callbacks.RemoveAt(i);

                    break;
                }
            }
        }
    }



}
