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
using Unity.Entities;
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
    [HideInInspector] public List<string> scene_Additives_Loaded = new List<string>();

    //store our generated buttons
    List<Button> sceneButtons = new List<Button>();

    public static int totalSetupButtonList;
    public static int listDone;

    private EntityManager entityManager;
    public IEnumerator Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //gather our total list instances that need to be setup to know when we are finished setting up our UI
        ++totalSetupButtonList;

        yield return new WaitUntil(() => GameStateManager.Instance.isAssetLoading_Finished);
        

        if (isURLButtonList)
        {
            if (!transformToPlaceButtonUnder)
                transformToPlaceButtonUnder = transform;

          //  List<GameObject> buttonLinks = new List<GameObject>();

            for (int i = 0; i < importAsset_Data_Container.dataList.Count; i++)
            {
                GameObject temp = Instantiate(buttonTemplate, transformToPlaceButtonUnder);

                Button tempButton = temp.GetComponentInChildren<Button>(true);
                UIManager.Instance.assetButtonRegister_List.Add(tempButton);

                Toggle tempLockToggle = temp.GetComponentInChildren<Toggle>();

                UIManager.Instance.assetLockToggleRegister_List.Add(tempLockToggle);


               SetButtonDelegateURL(tempButton, i, tempLockToggle);
                Text tempText = temp.GetComponentInChildren<Text>(true);
                tempText.text = importAsset_Data_Container.dataList[i].name;

              //  buttonLinks.Add(temp);
            }
           
        }
        else if(isEnvironmentButtonList){

            if (!transformToPlaceButtonUnder)
                transformToPlaceButtonUnder = transform;

   //         List<GameObject> buttonLinks = new List<GameObject>();

            for (int i = 0; i < sceneList.sceneReferenceList.Count; i++)
            {
                GameObject temp = Instantiate(buttonTemplate, transformToPlaceButtonUnder);

                Button tempButton = temp.GetComponentInChildren<Button>(true);

                SceneManagerExtensions.Instance.sceneButtonRegister_List.Add(tempButton);

                SetButtonDelegate_Scene(tempButton, sceneList.sceneReferenceList[i]);
                Text tempText = temp.GetComponentInChildren<Text>(true);

                tempText.text = sceneList.sceneReferenceList[i].name;// scene_list[i].name;//scenes[i].name;

               // buttonLinks.Add(temp);
                sceneButtons.Add(tempButton);

            }
        }

        //we are finished processing all of our list
        ++listDone;

        if (totalSetupButtonList == listDone)
            GameStateManager.Instance.isUISetup_Finished = true;

    }


    public void SetButtonDelegateURL(Button button, int index, Toggle toggleLock)
    {

        toggleLock.onValueChanged.AddListener((bool lockState) => { CallBackOnAssetLockSelect(lockState, toggleLock, index, true); }
        );

            //setup asset spawning mechanism
            button.onClick.AddListener(delegate
            {
                var isAssetActive = entityManager.GetEnabled(ClientSpawnManager.Instance.topLevelEntityList[index]);
                entityManager.SetEnabled(ClientSpawnManager.Instance.topLevelEntityList[index], !isAssetActive);// ClientSpawnManager.Instance.renderAssetFlag[index];
                button.SetButtonStateColor(Color.green, !isAssetActive);

                if (isAssetActive)
                {
                  //  entityManager.SetEnabled(ClientSpawnManager.Instance.topLevelEntityList[index], true) ;
                  ////  ClientSpawnManager.Instance.renderAssetFlag[index] = true;
                  //  //set our color to selected state
                  //  button.SetButtonStateColor(Color.green, true);

                    EventSystem.current.SetSelectedGameObject(button.gameObject);
                }
                else
                {
                    // ClientSpawnManager.Instance.renderAssetFlag[index] = false;
                    //entityManager.SetEnabled(ClientSpawnManager.Instance.topLevelEntityList[index], false);
                    ////regress our color back to initial state
                    //button.SetButtonStateColor(Color.white, false);

                    //get rid of selected object after deselecting it
                    EventSystem.current.SetSelectedGameObject(null);

                }

                //  isAssetActive = entityManager.GetEnabled(ClientSpawnManager.Instance.topLevelEntityList[index]);
                UIManager.Instance.On_Button_RenderAsset(index, !isAssetActive);

            });
    }

    public void CallBackOnAssetLockSelect(bool currentLockStatus, Toggle toggleButton, int index, bool callToNetwork)
    {
        foreach (NetworkAssociatedGameObject item in ClientSpawnManager.Instance.decomposedAssetReferences_Dict[index])
        {
           
                if (currentLockStatus)
                {
                    if (!entityManager.HasComponent<TransformLockTag>(item.Entity))
                        entityManager.AddComponentData(item.Entity, new TransformLockTag{ });

                }
                else
                {
                    if (entityManager.HasComponent<TransformLockTag>(item.Entity))
                        entityManager.RemoveComponent<TransformLockTag>(item.Entity);
                }
        }

        toggleButton.graphic.transform.parent.gameObject.SetActive(currentLockStatus);

        if (callToNetwork)
        {
            int lockState = 0;

            //SETUP and send network lockstate
            if (currentLockStatus)
                lockState = (int)INTERACTIONS.LOCK;
            else
                lockState = (int)INTERACTIONS.UNLOCK;

            int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(ClientSpawnManager.Instance.decomposedAssetReferences_Dict[index][0].Entity).entityID;

            NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
            {
                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
                targetEntity_id = entityID,
                interactionType = lockState,

            });

        }
    }


    public void SetButtonDelegate_Scene(Button button, SceneReference sceneRef)
    {
       

        button.onClick.AddListener(delegate {
            foreach (Button but in sceneButtons)
            {
                but.interactable = true;
            };
        });

        button.onClick.AddListener(delegate {
            SceneManagerExtensions.Instance.On_Select_Scene_Refence_Button(sceneRef, button);
        });

      
    }

}
