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

public class ModelButtonList : ButtonList
{
    public ModelDataTemplate modelData;

    public Color activeColor = new Color(255, 0, 255, 1);
    public Color inactiveColor = new Color(255, 0, 255, 0.5f);

    private EntityManager entityManager;
    public override IEnumerator Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        yield return new WaitUntil(() => GameStateManager.Instance.isAssetImportFinished);
        
        InitializeButtons();

        NotifyIsReady();

    }

    protected override void InitializeButtons () {
        if (!transformToPlaceButtonUnder)
            transformToPlaceButtonUnder = transform;

        //  List<GameObject> buttonLinks = new List<GameObject>();

        for (int i = 0; i < modelData.models.Count; i++)
        {
            GameObject temp = Instantiate(buttonTemplate, transformToPlaceButtonUnder);

            Button tempButton = temp.GetComponentInChildren<Button>(true);
            UIManager.Instance.modelVisibilityButtonList.Add(tempButton);

            Toggle tempLockToggle = temp.GetComponentInChildren<Toggle>();

            UIManager.Instance.modelLockButtonList.Add(tempLockToggle);


            SetButtonDelegate(tempButton, i, tempLockToggle);
            Text tempText = temp.GetComponentInChildren<Text>(true);
            tempText.text = modelData.models[i].name;

            //  buttonLinks.Add(temp);
        }
    }
    
    protected override void NotifyIsReady()
    {
        base.NotifyIsReady();
        UIManager.Instance.isModelButtonListReady = true;
    }

    public void SetButtonDelegate(Button button, int index, Toggle toggleLock)
    {
        toggleLock.onValueChanged.AddListener((bool lockState) => {         
            OnSelectModelLock(lockState, toggleLock, index, true); 
        });

        //set up model spawning mechanism
        button.onClick.AddListener(delegate
        {
            var modelEntity = ClientSpawnManager.Instance.GetEntity(index);
            var isAssetActive = entityManager.GetEnabled(modelEntity);
            entityManager.SetEnabled(modelEntity, !isAssetActive);
            button.SetButtonColor(!isAssetActive, activeColor, inactiveColor);
            if (isAssetActive)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
            else
            {
                //get rid of selected object after deselecting it
                EventSystem.current.SetSelectedGameObject(null);
            }
            UIManager.Instance.ToggleModelVisibility(index, !isAssetActive);

        });
    }

    public void OnSelectModelLock(bool currentLockStatus, Toggle toggleButton, int index, bool callToNetwork)
    {
        foreach (NetworkedGameObject item in ClientSpawnManager.Instance.GetNetworkedSubObjectList(index))
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

            int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(ClientSpawnManager.Instance.GetNetworkedSubObjectList(index)[0].Entity).entityID;

            NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
            {
                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
                targetEntity_id = entityID,
                interactionType = lockState,

            });

        }
    }

}
