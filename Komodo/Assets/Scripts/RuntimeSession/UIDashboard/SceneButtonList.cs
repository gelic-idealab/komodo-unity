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

namespace Komodo.Runtime
{
    public class SceneButtonList : ButtonList
    {
        public SceneList sceneList;
        [HideInInspector] public List<string> scene_Additives_Loaded = new List<string>();

        //store our generated buttons
        List<Button> sceneButtons = new List<Button>();

        private EntityManager entityManager;

        public override IEnumerator Start()
        {
            //check if we should set up scenes
            if (!SceneManagerExtensions.IsAlive)
            {
                gameObject.SetActive(false);
                yield break;
            }
            else
            {
                gameObject.SetActive(true);
                StartCoroutine(base.Start());
            }
            yield return null;
        }

        protected override void InitializeButtons()
        {
            //if we do not detect a scenemanager in scene we do not 
            if (!transformToPlaceButtonUnder)
            transformToPlaceButtonUnder = transform;

            for (int i = 0; i < sceneList.references.Count; i++)
            {
                GameObject temp = Instantiate(buttonTemplate, transformToPlaceButtonUnder);

                Button tempButton = temp.GetComponentInChildren<Button>(true);

                SceneManagerExtensions.Instance.sceneButtonRegister_List.Add(tempButton);

                SetSceneButtonDelegate(tempButton, sceneList.references[i]);
                Text tempText = temp.GetComponentInChildren<Text>(true);

                tempText.text = sceneList.references[i].name;// scene_list[i].name;//scenes[i].name;

                // buttonLinks.Add(temp);
                sceneButtons.Add(tempButton);

            }
        }

        protected override void NotifyIsReady()
        {
            base.NotifyIsReady();

            if (UIManager.IsAlive)
                UIManager.Instance.isSceneButtonListReady = true;
        }

        public void SetSceneButtonDelegate(Button button, SceneReference sceneRef)
        {


            button.onClick.AddListener(delegate
            {
                foreach (Button b in sceneButtons)
                {
                    b.interactable = true;
                };
            });

            button.onClick.AddListener(delegate
            {
                SceneManagerExtensions.Instance.OnSelectSceneReferenceButton(sceneRef, button);
            });


        }

    }
}
