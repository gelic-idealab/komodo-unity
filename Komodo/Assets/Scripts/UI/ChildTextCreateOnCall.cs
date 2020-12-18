using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ChildTextCreateOnCall : MonoBehaviour
{
    public Transform transformToAddTextUnder;
    public GameObject textProfile;

    Dictionary<string, GameObject> clientIDsToLabelGO = new Dictionary<string, GameObject>();

    public void CreateTextFromString(string clientID)
    {
      
        if (!clientIDsToLabelGO.ContainsKey(clientID))
        {
            //wait to create text until position is situated
            var newObj = GameObject.Instantiate(textProfile);
            clientIDsToLabelGO.Add(clientID, newObj);

            var newText = newObj.GetComponent<Text>();
            clientIDsToLabelGO[clientID] = newObj;

            newText.text = clientID;
            newObj.transform.SetParent(transformToAddTextUnder, false);
        }
        else
            Debug.Log("CLIENT LABEL + " + clientID + " Already exist");
    }

   
    //wait to load
    //public async void CreateText(string clientID)
    //{
    //    while (!ClientSpawnManager.Instance.isURL_Loading_Finished)
    //        await Task.Delay(001);

    //    var newObj = GameObject.Instantiate(textProfile);
    //    var newText = newObj.GetComponent<Text>();

    //    clientIDsToLabelGO.Add(clientID, newObj);

    //    newText.text = clientID;
    //    newObj.transform.SetParent(transformToAddTextUnder, false);

    //}



    public void DeleteTextFromString(string clientID)
    {
       // Debug.Log(1);
        DeleteClientID_Await(clientID);
    }

    public async void DeleteClientID_Await(string clientID)
    {
        //  Debug.Log(1);
        //    if (ClientSpawnManager.Instance._client_ID_List.Contains(clientID))
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
