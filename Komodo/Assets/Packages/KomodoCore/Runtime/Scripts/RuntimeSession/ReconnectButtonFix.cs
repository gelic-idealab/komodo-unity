using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

namespace Komodo.Runtime
{
    public class ReconnectButtonFix : MonoBehaviour
    {
        void Start()
        {
            NetworkUpdateHandler netHandler = NetworkUpdateHandler.Instance;

            KomodoEventManager.StartListening("network.reconnect", () =>
            {
                netHandler.Reconnect();
            });
        }
    }
}
