﻿// University of Illinois/NCSA
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
    /// <summary>
    /// This is a template for generating a list of buttons.
    /// </summary>
    public class ButtonList : MonoBehaviour
    {
        /// <summary>
        /// define the prefab to use to generate our list of buttons
        /// </summary>
        public GameObject buttonTemplate;

        /// <summary>
        /// define were to place our generated buttons under
        /// </summary>
        public Transform transformToPlaceButtonUnder;

        private EntityManager entityManager;
        public virtual IEnumerator Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            InitializeButtons();

            NotifyIsReady();

            yield return null;

        }

        /// <summary>
        /// A virtual overridable method for initializing buttons.
        /// </summary>
        protected virtual void InitializeButtons()
        {

        }

        /// <summary>
        /// A virtual overridable method for notifying if buttons are ready.
        /// </summary>
        protected virtual void NotifyIsReady()
        {

        }

    }
}