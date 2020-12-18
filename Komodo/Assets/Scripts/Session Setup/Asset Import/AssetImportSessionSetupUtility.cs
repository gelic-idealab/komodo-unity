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


using System.Collections.Generic;
using UnityEngine;


//Provides funcions to set up gameobject with colliders and setup references for network interaction
public class AssetImportSessionSetupUtility 
{
    /// <summary>
    /// Setup imported objects with colliders, register it to be used accross the network and set properties from the data received and the setup flags from AssetImportSetupSettings
    /// </summary>
    /// <param name="index"> for UI Button referencing</param>
    /// <param name="importedObject_Data"> custom data received by the network</param>
    /// <param name="loadedObject"> our loaded object</param>
    /// <param name="setUpFlags"> setup instructions</param>
    /// <returns></returns>
    public static GameObject SetupGameObjects(int index, AssetDataTemplate.AssetImportData importedObject_Data, GameObject loadedObject, AssetImportSetupSettings setUpFlags = null)
    {
        if (setUpFlags == null)
        {
            setUpFlags = ScriptableObject.CreateInstance<AssetImportSetupSettings>();
            setUpFlags.defaultSizeToLoadGO = 2;
            setUpFlags.setUpColliders = true;
            setUpFlags.setupNetRegisterGO = true;
            setUpFlags.environmentHeight = 0f;
        }

        //Create a rootParent for our asset

        Transform newGOParent = new GameObject(index.ToString()).transform;

        #region GameObject Network Link Setup
        //set up reference to use with network
        if (setUpFlags.setupNetRegisterGO)
            ClientSpawnManager.Instance.LinkNewNetworkObject(newGOParent.gameObject, index, importedObject_Data.id);
        #endregion

        //provide appropriate tag to enable it to be grabbed
        newGOParent.gameObject.gameObject.tag = "Interactable";
      
        newGOParent.gameObject.SetActive(false);

        #region BoundingBox and Collider Setup

        Bounds bounds = new Bounds();

        if (setUpFlags.setUpColliders)
        {
            //clear subobjectlist for new object processiong
            List<Bounds> subObjectBounds = new List<Bounds>();

            //reset rotation to avoid align axis bounding errors
            loadedObject.transform.rotation = Quaternion.identity;

            //obtain all bounds from skinned mesh renderer and skinned mesh remderer
            CombineMesh(loadedObject.transform, subObjectBounds);

            bounds = new Bounds(subObjectBounds[0].center, Vector3.one * 0.02f);

            //set bounds from all subobjects
            for (int i = 0; i < subObjectBounds.Count; i++)
            {
                bounds.Encapsulate(new Bounds(subObjectBounds[i].center, subObjectBounds[i].size));
            }

            //set collider properties
            var wholeCollider = newGOParent.gameObject.AddComponent<BoxCollider>();
         
            //center the collider to our new parent and send proper size of it based on the renderers picked up
            wholeCollider.center = Vector3.zero;// bounds.center;
            wholeCollider.size = bounds.size;

            //set parent to be the center of our loaded object to show correct deformations with scalling 
            newGOParent.transform.position = bounds.center;
            
            loadedObject.transform.SetParent(newGOParent.transform, true);
         

            newGOParent.transform.position = Vector3.up * setUpFlags.environmentHeight;

            //CHECK IF OBJECT IS TO BIG TO DOWNSCALE TO DEFAULT
            while (bounds.extents.x > setUpFlags.defaultSizeToLoadGO || bounds.extents.y > setUpFlags.defaultSizeToLoadGO || bounds.extents.z > setUpFlags.defaultSizeToLoadGO || bounds.extents.x < -setUpFlags.defaultSizeToLoadGO || bounds.extents.y < -setUpFlags.defaultSizeToLoadGO || bounds.extents.z < -setUpFlags.defaultSizeToLoadGO)
            {
                newGOParent.transform.localScale *= 0.9f;
                bounds.extents *= 0.9f;
            }
            
            //animated objects get there whole collider set by default, and we set it to be affected by physics
            var animationComponent = loadedObject.GetComponent<Animation>();

            if (animationComponent)
            {
                loadedObject.GetComponent<Animation>().animatePhysics = true;
            }
         


            //turn off whole colliders for those set to be deconstructive
            if (!importedObject_Data.isWholeObject)
            {
                //activate to allow GO setup to happen
                newGOParent.gameObject.SetActive(true);

                //a dictionary to keep new parent and child references to set correct pivot parents after child iteration
                Dictionary<Transform, Transform> childAndNewParentPivot_Dictionary = new Dictionary<Transform, Transform>();
                SetUpRigidBodyBAndColliders(loadedObject.transform, index, ref childAndNewParentPivot_Dictionary);

                //since we are creating new parent pivots dueing the child iteration process we have to set parents after the fact
                foreach (KeyValuePair<Transform, Transform> item in childAndNewParentPivot_Dictionary)
                    SetParentingAfterChildIteration(item.Key, item.Value);

                //if we do have animation in this object we turn on its whole collider to use for reference
                if (animationComponent != null)
                    wholeCollider.enabled = true;
                else

                    wholeCollider.enabled = false;
            }

      

            newGOParent.gameObject.SetActive(false);
        }
        #endregion

        //set custom properties
        newGOParent.transform.localScale *= importedObject_Data.scale;
     
        //Check for our highest extent to know how much to offset it up (to lift it up in relation to its scale)
        var lowestPoint = Mathf.Infinity;
        if (lowestPoint > bounds.extents.y)
        {
            lowestPoint = bounds.extents.y;
        }

        newGOParent.transform.position += Vector3.up * lowestPoint;

        newGOParent.rotation = Quaternion.Euler(importedObject_Data.euler_rotation);

        return newGOParent.gameObject;
    }

    //build up the bounds from renderers that make up asset
    public static void CombineMesh(Transform trans, List<Bounds> combinedMeshList)
    {

        //parent check
        if (ObtainBoundsFromRenderers(trans.gameObject, out Bounds mf1))
            combinedMeshList.Add(mf1);


        //search each child object for components
        foreach (Transform child in trans)
        {

            //if our object has bounds we mark it
            if (ObtainBoundsFromRenderers(child.gameObject, out Bounds mf))
                combinedMeshList.Add(mf);


            //deeper children search
            if (child.childCount > 0)
                CombineMesh(child, combinedMeshList);
        }
    }

    public static bool ObtainBoundsFromRenderers(GameObject gameObjectToCheck, out Bounds bounds)
    {
        MeshRenderer meshRenderer = null;
        SkinnedMeshRenderer smr = null;

        meshRenderer = gameObjectToCheck.GetComponent<MeshRenderer>();
        smr = gameObjectToCheck.GetComponent<SkinnedMeshRenderer>();

        

        if (meshRenderer != null)
        {
            bounds = meshRenderer.bounds;
            return true;//mf.bounds;
        }
        else
        {
            if (smr != null)
            {
                bounds = smr.bounds;
                return true;//smr.bounds;
            }
            else
            {
                bounds = default;
                return false;
            }
                
        }

    }

    public static Transform CheckForIndividualFilterAndSkinn(GameObject gameObjectToCheck, int indexForUISelection = -1, bool setNetwork = true)
    {
        bool hasMeshFilter = false;
        MeshFilter meshF = default;

        bool hasSkinnedMR = false;
        SkinnedMeshRenderer skinned = default;

        //Access what components are available
        meshF = gameObjectToCheck.GetComponent<MeshFilter>();
        if (meshF != null)
            hasMeshFilter = true;

        skinned = gameObjectToCheck.GetComponent<SkinnedMeshRenderer>();
        if (skinned != null)
            hasSkinnedMR = true;

        var animationComponent = gameObjectToCheck.GetComponent<Animation>();
        if (animationComponent)
            animationComponent.animatePhysics = true;
       
        //to avoid selecting different anim references between checking
        if (hasMeshFilter || hasSkinnedMR)
        {
            //  create new parent to be a pivot of object to deform correctly when grabbing
            Transform newGOParent = new GameObject().transform;

            //set new parent with sibling pos and scale
            newGOParent.SetParent(gameObjectToCheck.transform.parent, true);
            newGOParent.localPosition = gameObjectToCheck.transform.localPosition;
            newGOParent.localScale = gameObjectToCheck.transform.localScale;

            if (hasMeshFilter)
            {
                //set the parent as a child to set pivot in correct location
                newGOParent.SetParent(gameObjectToCheck.transform, true);

                newGOParent.transform.localRotation = Quaternion.identity;

                //set grab tag
                newGOParent.gameObject.tag = "Interactable";

                BoxCollider tempCollider = newGOParent.gameObject.AddComponent<BoxCollider>();

                tempCollider.size = meshF.mesh.bounds.size;
           //     tempCollider.center = newGOParent.localPosition;

                newGOParent.localPosition = meshF.mesh.bounds.center;

                if (setNetwork)
                    ClientSpawnManager.Instance.LinkNewNetworkObject(newGOParent.gameObject, indexForUISelection);
            }
            else if (hasSkinnedMR)
            {
                //set anim objects as one whole object
                if (skinned.rootBone)
                {


                }
            }
            return newGOParent;
        }

        return null;
    }

    /// <summary>
    /// Setup the given Transform hierarchy by looking for renderer components
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="uiIndex"></param>
    /// <param name="childAndNewParentPivot_Dictionary"></param>
    static void SetUpRigidBodyBAndColliders(Transform trans, int uiIndex, ref Dictionary<Transform, Transform> childAndNewParentPivot_Dictionary)
    {
        //check that we are not looking at a transform that we already looked at
        if (!childAndNewParentPivot_Dictionary.ContainsValue(trans))
        {
            //Check parent first for renderers 
            Transform newPO2 = CheckForIndividualFilterAndSkinn(trans.gameObject, uiIndex);

            if (newPO2 != null)
                childAndNewParentPivot_Dictionary.Add(newPO2, trans);
        }

        //check children for renderers
        foreach (Transform child in trans)
        {
          
                Transform newPO = CheckForIndividualFilterAndSkinn(child.gameObject, uiIndex);

                //if we made a new pivot parent add it to the dictionary to iterate after completing iteraction for the specific asset
                if (newPO != null)
                    childAndNewParentPivot_Dictionary.Add(newPO, child);

                //deeper children search //resets our dic
                if (child.childCount > 0)
                    SetUpRigidBodyBAndColliders(child, uiIndex, ref childAndNewParentPivot_Dictionary);
        }

    }

        /// <summary>
            /// Since we cannot add parents while we iterate through children to set their colliders and netobjects we have to do this after the fact.
            /// </summary>
            /// <param name="pivot"></param>
            /// <param name="child"></param>
            public static void SetParentingAfterChildIteration(Transform pivot, Transform child)
            {
      
                //grab the original parent before changing it
                Transform lastTransform = child.parent;

                //remove newPar child reference (used to obtain center pivot)
                child.DetachChildren();

                //now attach child to newParent to have pivot pivot point parent
                child.transform.SetParent(pivot, true);

                //set pivot to be parented to the original child parent
                pivot.transform.SetParent(lastTransform);
            }

}
