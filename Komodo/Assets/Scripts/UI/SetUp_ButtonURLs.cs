// University of Illinois/NCSA
// Open Source License
// http://otm.illinois.edu/disclose-protect/illinois-open-source-license

// Copyright (c) 2020 Grainger Engineering Library Information Center.  All rights reserved.

// Developed by: IDEA Lab
//               Grainger Engineering Library Information Center - University of Illinois Urbana-Champaign
//               https://library.illinois.edu/enx

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal with
// the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to
// do so, subject to the following conditions:
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimers.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimers in the documentation
//   and/or other materials provided with the distribution.
// * Neither the names of IDEA Lab, Grainger Engineering Library Information Center,
//   nor the names of its contributors may be used to endorse or promote products
//   derived from this Software without specific prior written permission.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE
// SOFTWARE.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetUp_ButtonURLs : MonoBehaviour
{
    //define the prefab to use to generate our list of buttons
    public GameObject buttonTemplate;

    //define were to place our generated buttons under
    public Transform transformToPlaceButtonUnder;

    //to allow two different button setup asset and scenes
    public bool isURLButtonList = true;
    public AssetDataTemplate importAsset_Data_Container;

    public bool isEnvironmentButtonList = false;
    public SceneList sceneList;

    //store our generated buttons
    List<Button> allButtons = new List<Button>();

    public static int totalSetupButtonList;
    public static int listDone;
   
    
    public IEnumerator Start()
    {
        ++totalSetupButtonList;

        yield return new WaitUntil(() => GameStateManager.Instance.isAssetLoading_Finished);
        

        if (isURLButtonList)
        {
            if (!transformToPlaceButtonUnder)
                transformToPlaceButtonUnder = transform;

            List<GameObject> buttonLinks = new List<GameObject>();

            for (int i = 0; i < importAsset_Data_Container.dataList.Count; i++)
            {
                GameObject temp = Instantiate(buttonTemplate, transformToPlaceButtonUnder);

                Button tempButton = temp.GetComponentInChildren<Button>(true);
                ClientSpawnManager.Instance.assetButtonRegister_List.Add(tempButton);
                ClientSpawnManager.Instance.renderAssetFlag.Add(false);

                Toggle tempLockToggle = temp.GetComponentInChildren<Toggle>();

                ClientSpawnManager.Instance.assetLockToggleRegister_List.Add(tempLockToggle);


               SetButtonDelegateURL(tempButton, i, tempLockToggle);
                Text tempText = temp.GetComponentInChildren<Text>(true);
                tempText.text = importAsset_Data_Container.dataList[i].name;

                buttonLinks.Add(temp);
            }
           
        }
        else if(isEnvironmentButtonList){

            if (!transformToPlaceButtonUnder)
                transformToPlaceButtonUnder = transform;

            List<GameObject> buttonLinks = new List<GameObject>();

            for (int i = 0; i < sceneList.sceneReferenceList.Count; i++)
            {
                GameObject temp = Instantiate(buttonTemplate, transformToPlaceButtonUnder);

                Button tempButton = temp.GetComponentInChildren<Button>(true);

                ClientSpawnManager.Instance.sceneButtonRegister_List.Add(tempButton);

                SetButtonDelegate_Scene(tempButton, sceneList.sceneReferenceList[i]);
                Text tempText = temp.GetComponentInChildren<Text>(true);

                tempText.text = sceneList.sceneReferenceList[i].name;// scene_list[i].name;//scenes[i].name;

                buttonLinks.Add(temp);
                allButtons.Add(tempButton);

            }
        }

        ++listDone;

        if (totalSetupButtonList == listDone)
            GameStateManager.Instance.isUISetup_Finished = true;

    }


    public void SetButtonDelegateURL(Button button, int index, Toggle toggleLock)
    {

        toggleLock.onValueChanged.AddListener((bool lockState) => { CallBackOnAssetLockSelect(lockState, toggleLock, index, true); }
        );
        
        //setup asset spawning mechanism
        button.onClick.AddListener(delegate {

            var isAssetActive = ClientSpawnManager.Instance.renderAssetFlag[index];

            if (!isAssetActive)
            {
                ClientSpawnManager.Instance.renderAssetFlag[index] = true;

                //set our color to selected state
                button.SetButtonStateColor(Color.green, true);

                EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
            else
            {
                ClientSpawnManager.Instance.renderAssetFlag[index] = false;

                //regress our color back to initial state
                button.SetButtonStateColor(Color.white, false);

                //get rid of selected object after deselecting it
                EventSystem.current.SetSelectedGameObject(null);

            }
          

            ClientSpawnManager.Instance.On_Button_RenderAsset(index, ClientSpawnManager.Instance.renderAssetFlag[index]);

        });
    }

    public void CallBackOnAssetLockSelect(bool currentLockStatus, Toggle toggleButton, int index, bool callToNetwork)
    {
        foreach (Net_Register_GameObject item in ClientSpawnManager.Instance.decomposedAssetReferences_Dict[index])
            item.entity_data.isCurrentlyGrabbed = currentLockStatus;


        toggleButton.graphic.transform.parent.gameObject.SetActive(currentLockStatus);
        //  toggleButton.graphic.enabled = currentLockStatus;//.isOn = currentLockStatus;

        if (callToNetwork)
        {
            int lockState = 0;

            //SETUP and send network lockstate
            if (currentLockStatus)
                lockState = (int)INTERACTIONS.LOCK;
            else
                lockState = (int)INTERACTIONS.UNLOCK;

            NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
            {
                sourceEntity_id = index,
                targetEntity_id = ClientSpawnManager.Instance.decomposedAssetReferences_Dict[index][0].entity_data.entityID,
                interactionType = lockState,

            });

        }
    }


    public void SetButtonDelegate_Scene(Button button, SceneReference sceneRef)
    {
       

        button.onClick.AddListener(delegate {
            foreach (Button but in allButtons)
            {
                but.interactable = true;
            };
        });

        button.onClick.AddListener(delegate {
            ClientSpawnManager.Instance.On_Select_Scene_Refence_Button(sceneRef, button);
        });

      
    }

}
