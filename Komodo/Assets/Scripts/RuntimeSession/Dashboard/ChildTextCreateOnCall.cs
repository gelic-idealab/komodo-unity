using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    public class ChildTextCreateOnCall : MonoBehaviour
    {
        public Transform transformToAddTextUnder;
        public GameObject textProfile;

        Dictionary<string, GameObject> clientIDsToLabelGO = new Dictionary<string, GameObject>();

        public void CreateTextFromString(string clientTextLabel, int clientID)
        {
            if (!clientIDsToLabelGO.ContainsKey(clientTextLabel))
            {
                //wait to create text until position is situated
                var newObj = Instantiate(textProfile);
                clientIDsToLabelGO.Add(clientTextLabel, newObj);

                var newText = newObj.GetComponentInChildren<Text>(true);

                clientIDsToLabelGO[clientTextLabel] = newObj;

                newText.text = clientTextLabel;
                newObj.transform.SetParent(transformToAddTextUnder, false);
            }
            else
                Debug.Log("CLIENT LABEL + " + clientTextLabel + " Already exist");
        }

        public void DeleteTextFromString(string clientID)
        {
            DeleteClientID_Await(clientID);
        }

        public async void DeleteClientID_Await(string clientID)
        {
            if (clientIDsToLabelGO.ContainsKey(clientID))
            {
                while (clientIDsToLabelGO[clientID] == null)
                    await Task.Delay(1);

                Destroy(clientIDsToLabelGO[clientID]);
                clientIDsToLabelGO.Remove(clientID);

            }
            else
                Debug.Log("Client Does not exist");

        }

    }
}
