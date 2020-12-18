using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SetUp_Button_ToolOptions : MonoBehaviour
{
    public GameObject _buttonTemplate;

    public Transform transformToPlaceButtonUnder;

    public Transform toolParent;

    public GameObject[] manipulationToolList;

    List<Button> buttonLinks;
    IEnumerator Start()
    {
        toolParent = GameObject.FindGameObjectWithTag("Tool").transform;
        //foreach (var item in toolParent)
        //{

        //}
     //   manipulationToolList = GameObject.FindGameObjectsWithTag("Tool");
 
            if (!transformToPlaceButtonUnder)
                transformToPlaceButtonUnder = transform;

            buttonLinks = new List<Button>();

        for (int i = 0; i < toolParent.childCount; i++)
        {
            GameObject temp = Instantiate(_buttonTemplate, transformToPlaceButtonUnder);

            Button tempButton = temp.GetComponentInChildren<Button>(true);

            SetButtonToolActive(tempButton, i);
            Text tempText = temp.GetComponentInChildren<Text>(true);
            tempText.text = toolParent.GetChild(i).name;

            //  temp.SetActive(false);
            buttonLinks.Add(tempButton);
        }
        //for (int i = 0; i < manipulationToolList.Length; i++)
        //{
        //    GameObject temp = Instantiate(_buttonTemplate, transformToPlaceButtonUnder);

        //    Button tempButton = temp.GetComponentInChildren<Button>(true);

        //    SetButtonToolActive(tempButton, i);
        //    Text tempText = temp.GetComponentInChildren<Text>(true);
        //    tempText.text = manipulationToolList[i].name;

        //    //  temp.SetActive(false);
        //    buttonLinks.Add(tempButton);
        //}

        SetAllToDeActivate();
            yield return null;
          //  yield return new WaitUntil(() => ClientSpawnManager.Instance.isURL_LoadingFinished);

        
    }

    public void SetButtonToolActive(Button button, int index)
    {
        button.onClick.AddListener(delegate {

            EventSystem.current.SetSelectedGameObject(null);
            //Remove from Pressed
            foreach (var but in buttonLinks)
            {
                but.interactable = true;
              //  EventSystem.current.SetSelectedGameObject(null);
            }
            button.interactable = false;
            //EventSystem.current.SetSelectedGameObject(button.gameObject);
            //button.Select();

            SetAllToDeActivate();

            toolParent.GetChild(index).gameObject.SetActive(true);
          //  manipulationToolList[index].SetActive(true);



        });
    }
    public void SetAllToDeActivate()
    {
        foreach (Transform tool in toolParent)
        {
            tool.gameObject.SetActive(false);
        }
        //foreach (var tool in manipulationToolList)
        //{
        //    tool.SetActive(false);
        //}
    }


}
