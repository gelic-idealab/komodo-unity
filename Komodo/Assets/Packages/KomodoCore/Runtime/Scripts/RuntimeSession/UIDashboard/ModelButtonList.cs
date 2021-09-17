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
using Komodo.AssetImport;

namespace Komodo.Runtime
{
    public class ModelButtonList : ButtonList
    {
        public ModelDataTemplate modelData;

        public Color activeColor = new Color(255, 0, 255, 1);

        public Color inactiveColor = new Color(255, 0, 255, 0.5f);

        private EntityManager entityManager;
        public override IEnumerator Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            //check if we should set up models
            if (!ModelImportInitializer.IsAlive)
            {
                gameObject.SetActive(false);
                yield break;
            }
            else
            {
                gameObject.SetActive(true);
          //      StartCoroutine(base.Start());
            }
        //    yield return null;

            yield return new WaitUntil(() => GameStateManager.Instance.isAssetImportFinished);

            InitializeButtons();

            NotifyIsReady();
        }

        protected override void InitializeButtons()
        {
            if (!transformToPlaceButtonUnder) {
                transformToPlaceButtonUnder = transform;
            }

            for (int i = 0; i < modelData.models.Count; i++)
            {
                if (UIManager.IsAlive)
                {
                    GameObject temp = Instantiate(buttonTemplate, transformToPlaceButtonUnder);

                    Button tempButton = temp.GetComponentInChildren<Button>(true);

                    UIManager.Instance.modelVisibilityButtonList.Add(tempButton);

                    LockToggle lockToggle = temp.GetComponentInChildren<LockToggle>();

                    lockToggle.Initialize(i);

                    UIManager.Instance.modelLockToggleList.Add(lockToggle);

                    //set button active color
                    tempButton.image.color = activeColor;

                    InitializeListeners(tempButton, i, lockToggle);

                    Text tempText = temp.GetComponentInChildren<Text>(true);

                    tempText.text = modelData.models[i].name;
                }
            }
        }

        protected override void NotifyIsReady()
        {
            base.NotifyIsReady();

            if (UIManager.IsAlive)
            {
                UIManager.Instance.isModelButtonListReady = true;
            }
        }

        public void InitializeListeners(Button visibilityButton, int index, LockToggle lockToggle)
        {
            //set up model show / hide mechanism
            visibilityButton.onClick.AddListener(delegate
            {
                GameObject currentObj = ClientSpawnManager.Instance.GetNetworkedGameObject(index).gameObject;

                var isModelVisible = currentObj.activeInHierarchy;

                visibilityButton.SetButtonColor(!isModelVisible, activeColor, inactiveColor);

                if (isModelVisible)
                {
                    EventSystem.current.SetSelectedGameObject(visibilityButton.gameObject);
                }
                else
                {
                //get rid of selected object after deselecting it
                EventSystem.current.SetSelectedGameObject(null);
                }
                if (UIManager.IsAlive)
                {
                    UIManager.Instance.ToggleModelVisibility(index, !isModelVisible);
                }
            });

            //reset state
            visibilityButton.SetButtonColor(false, activeColor, inactiveColor);
        }
    }
}