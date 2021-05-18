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
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Komodo.AssetImport;

namespace Komodo.Runtime
{
    //Provides funcions to set up gameobject with colliders and setup references for network interaction
    public class ModelImportPostProcessor
    {
        /// <summary>
        /// Set up imported objects with colliders, register them to be used accross the network, and set properties from the data received and the setup flags from AssetImportSetupSettings
        /// </summary>
        /// <param name="menuButtonIndex"> index of Menu Button. </param>
        /// <param name="modelData"> custom data received by the network</param>
        /// <param name="loadedObject"> our loaded object</param>
        /// <param name="setupFlags"> setup instructions</param>
        /// <returns></returns>
        public static GameObject SetUpGameObject(int menuButtonIndex, ModelDataTemplate.ModelImportData modelData, GameObject loadedObject, ModelImportSettings setupFlags = null)
        {
            const float defaultFitToScale = 2;
            const bool defaultDoSetUpColliders = true;
            const bool defaultIsNetworked = true;
            const float defaultModelSpawnHeight = 0.0f;

            if (loadedObject == null)
            {
                throw new System.Exception("Failed to import an model at runtime because the loaded object was null. Please ensure your custom runtime importer properly returns a valid GameObject.");
            }

            if (setupFlags == null)
            {
                setupFlags = ScriptableObject.CreateInstance<ModelImportSettings>();
                setupFlags.fitToScale = defaultFitToScale;
                setupFlags.doSetUpColliders = defaultDoSetUpColliders;
                setupFlags.isNetworked = defaultIsNetworked;
                setupFlags.spawnHeight = defaultModelSpawnHeight;
            }

            //parent of model in list
            Transform newParent = new GameObject(menuButtonIndex.ToString()).transform;

            NetworkedGameObject nRGO = default;
            if (setupFlags.isNetworked)
            {
                //set up reference to use with network
                nRGO = ClientSpawnManager.Instance.CreateNetworkedGameObject(newParent.gameObject, menuButtonIndex, modelData.id);

            }

            //provide appropriate tag to enable it to be grabbed
            newParent.gameObject.gameObject.tag = TagList.interactable;

            newParent.gameObject.SetActive(false);

            Bounds bounds = new Bounds();

            if (setupFlags.doSetUpColliders)
            {
                SetUpColliders(loadedObject, bounds, newParent, modelData, setupFlags, menuButtonIndex);

                //turn off whole colliders for non-whole objects
                if (!modelData.isWholeObject)
                {
                    SetUpSubObjects(newParent, loadedObject, menuButtonIndex);
                }

                SetUpAnimation(newParent, loadedObject, modelData.isWholeObject);
                AdjustHeight(newParent, setupFlags);
                AdjustScale(newParent, bounds, setupFlags);
            }

            AdjustPose(newParent, modelData, bounds);

            //Initialize fields for ECS
            ConvertObjectsToEntities(nRGO, newParent, menuButtonIndex);

            newParent.gameObject.SetActive(false);

            return newParent.gameObject;
        }

        public static void AdjustPose(Transform newParent, ModelDataTemplate.ModelImportData modelData, Bounds bounds)
        {
            //set custom properties
            newParent.transform.localScale *= modelData.scale;

            //Check for our highest extent to know how much to offset it up (to lift it up in relation to its scale)
            var lowestPoint = Mathf.Infinity;
            if (lowestPoint > bounds.extents.y)
            {
                lowestPoint = bounds.extents.y;
            }

            newParent.transform.position += Vector3.up * lowestPoint;

            newParent.rotation = Quaternion.Euler(modelData.euler_rotation);
        }

        public static void ConvertObjectsToEntities(NetworkedGameObject netObject, Transform newParent, int menuButtonIndex)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityCommandBuffer ecbs = entityManager.World.GetOrCreateSystem<EntityCommandBufferSystem>().CreateCommandBuffer();

            using (BlobAssetStore blobAssetStore = new BlobAssetStore())
            {

                var entity = Entity.Null;

                if (netObject && netObject.Entity != Entity.Null)
                {
                    entity = netObject.Entity;
                }
                else
                {
                    entity = entityManager.CreateEntity();
                }

#if ECS
            entityManager.SetName(entity, newParent.gameObject.name);
#endif

                ClientSpawnManager.Instance.topLevelEntityList.Add(entity);

                var buff = ecbs.AddBuffer<LinkedEntityGroup>(entity);

                SetEntityReferences(entityManager, newParent.transform, buff, menuButtonIndex, entity, true);

                //to be in par with gameobject representation current state 
                entityManager.SetEnabled(entity, false);

                //play back our structural changes after adding them to our command buffer
                ecbs.ShouldPlayback = false;

                ecbs.Playback(entityManager);

            }
        }

        public static void SetUpColliders(GameObject loadedObject, Bounds bounds, Transform newParent, ModelDataTemplate.ModelImportData modelData, ModelImportSettings setupFlags, int menuButtonIndex)
        {
            //clear subobjectlist for new object processiong
            List<Bounds> subObjectBounds = new List<Bounds>();

            //reset rotation to avoid align axis bounding errors
            loadedObject.transform.rotation = Quaternion.identity;

            //obtain all bounds from skinned mesh renderer and skinned mesh remderer
            CombineMesh(loadedObject.transform, subObjectBounds);

            if (subObjectBounds.Count > 0)
            {
                bounds = new Bounds(subObjectBounds[0].center, Vector3.one * 0.02f);
            }

            //set bounds from all sub-objects
            for (int i = 0; i < subObjectBounds.Count; i++)
            {
                bounds.Encapsulate(new Bounds(subObjectBounds[i].center, subObjectBounds[i].size));
            }

            //set collider properties
            var wholeCollider = newParent.gameObject.AddComponent<BoxCollider>();

            //center the collider to our new parent and send proper size of it based on the renderers picked up
            wholeCollider.center = Vector3.zero;// bounds.center;
            wholeCollider.size = bounds.size;

            //set parent to be the center of our loaded object to show correct deformations with scalling 
            newParent.transform.position = bounds.center;

            loadedObject.transform.SetParent(newParent.transform, true);
        }

        public static void AdjustHeight(Transform newParent, ModelImportSettings setupFlags)
        {
            newParent.transform.position = Vector3.up * setupFlags.spawnHeight;
        }

        public static void AdjustScale(Transform newParent, Bounds bounds, ModelImportSettings setupFlags)
        {
            //If the object's length, width, or height exceeds fitToScale, iteratively shrink the object.
            //TODO: change this to an immediate rescale based on ratios, rather than an iterative one. 
            while (bounds.extents.x > setupFlags.fitToScale || bounds.extents.y > setupFlags.fitToScale || bounds.extents.z > setupFlags.fitToScale || bounds.extents.x < -setupFlags.fitToScale || bounds.extents.y < -setupFlags.fitToScale || bounds.extents.z < -setupFlags.fitToScale)
            {
                newParent.transform.localScale *= 0.9f;
                bounds.extents *= 0.9f;
            }
        }

        public static void SetUpAnimation(Transform newParent, GameObject loadedObject, bool isWholeObject)
        {

            //animated objects get their whole collider set by default, and we set it to be affected by physics
            var animationComponent = loadedObject.GetComponent<Animation>();

            var wholeCollider = newParent.gameObject.GetComponent<BoxCollider>();

            if (!isWholeObject)
            {
                //if we do have animation in this object we turn on its whole collider to use for reference
                if (animationComponent != null)
                {
                    wholeCollider.enabled = true;
                }
                else
                {
                    wholeCollider.enabled = false;
                }
            }

            if (animationComponent)
            {
                loadedObject.GetComponent<Animation>().animatePhysics = true;
            }
        }

        public static void SetUpSubObjects(Transform newParent, GameObject loadedObject, int menuButtonIndex)
        {
            //activate to allow GO setup to happen
            newParent.gameObject.SetActive(true);

            //a dictionary to keep new parent and child references to set correct pivot parents after child iteration
            Dictionary<Transform, Transform> childToNewParentPivot = new Dictionary<Transform, Transform>();
            AddRigidBodiesAndColliders(loadedObject.transform, menuButtonIndex, ref childToNewParentPivot);

            //since we are creating new parent pivots during the child iteration process we have to set parents after the fact
            foreach (KeyValuePair<Transform, Transform> item in childToNewParentPivot)
            {
                SetParentAsPivot(item.Key, item.Value);
            }
        }

        static void SetEntityReferences(EntityManager entityManager, Transform transform, DynamicBuffer<LinkedEntityGroup> linkedEntityGroupList, int buttonIndex, Entity parentEntity, bool isFirstInstance)
        {
            EntityCommandBuffer ecbs = entityManager.World.GetOrCreateSystem<EntityCommandBufferSystem>().CreateCommandBuffer();

            var netREg = transform.GetComponent<NetworkedGameObject>();

            if (netREg)
            {
                var entityCreated = Entity.Null;

                if (isFirstInstance)
                {
                    isFirstInstance = false;
                    entityCreated = parentEntity;
                }
                else
                {
                    if (netREg.Entity == Entity.Null)
                        entityCreated = entityManager.CreateEntity();
                    else
                        entityCreated = netREg.Entity;


                    //entityCreated = entityManager.CreateEntity();
                    ecbs.AddComponent(entityCreated, new Parent { Value = parentEntity });
                }

                linkedEntityGroupList.Add(entityCreated);
                netREg.Entity = entityCreated;//associatedEntity;

                //add identification data
                if (!entityManager.HasComponent<NetworkEntityIdentificationComponentData>(netREg.Entity))
                    ecbs.AddComponent(netREg.Entity, new NetworkEntityIdentificationComponentData
                    {
                        entityID = netREg.thisEntityID,
                        clientID = NetworkUpdateHandler.Instance.client_id,
                        sessionID = NetworkUpdateHandler.Instance.session_id,
                        current_Entity_Type = Entity_Type.objects

                    });

                ecbs.AddSharedComponent(netREg.Entity, new ButtonIDSharedComponentData { buttonID = buttonIndex });
            }


            int numChildren = transform.childCount;

            for (int i = 0; i < numChildren; ++i)
            {
                SetEntityReferences(entityManager, transform.GetChild(i), linkedEntityGroupList, buttonIndex, parentEntity, false);
            }

            ecbs.Playback(entityManager);

            ecbs.ShouldPlayback = false;
        }

        //build up the bounds from renderers that make up asset
        public static void CombineMesh(Transform transform, List<Bounds> combinedMeshList)
        {
            //parent check
            if (ObtainBoundsFromRenderers(transform.gameObject, out Bounds mf1))
                combinedMeshList.Add(mf1);
            //search each child object for components
            foreach (Transform child in transform)
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
            MeshRenderer renderer = null;
            SkinnedMeshRenderer skinnedRenderer = null;

            renderer = gameObjectToCheck.GetComponent<MeshRenderer>();
            skinnedRenderer = gameObjectToCheck.GetComponent<SkinnedMeshRenderer>();

            if (renderer != null)
            {
                bounds = renderer.bounds;
                return true;
            }
            else
            {
                if (skinnedRenderer != null)
                {
                    bounds = skinnedRenderer.bounds;
                    return true;
                }
                else
                {
                    bounds = default;
                    return false;
                }
            }
        }

        public static Transform CheckForIndividualFiltersAndSkins(GameObject gameObjectToCheck, int menuButtonIndex = -1, bool isNetworked = true)
        {
            bool hasMeshFilter = false;
            MeshFilter filter = default;

            bool hasSkinnedRenderer = false;
            SkinnedMeshRenderer skinnedRenderer = default;

            //Access what components are available
            filter = gameObjectToCheck.GetComponent<MeshFilter>();
            if (filter != null) hasMeshFilter = true;
            skinnedRenderer = gameObjectToCheck.GetComponent<SkinnedMeshRenderer>();
            if (skinnedRenderer != null) hasSkinnedRenderer = true;
            var animationComponent = gameObjectToCheck.GetComponent<Animation>();
            if (animationComponent) animationComponent.animatePhysics = true;

            //to avoid selecting different anim references between checking
            if (hasMeshFilter || hasSkinnedRenderer)
            {
                //  create new parent to be a pivot of object to deform correctly when grabbing
                Transform newParent = new GameObject().transform;

                //set new parent with sibling pos and scale
                newParent.SetParent(gameObjectToCheck.transform.parent, true);
                newParent.localPosition = gameObjectToCheck.transform.localPosition;
                newParent.localScale = gameObjectToCheck.transform.localScale;

                if (hasMeshFilter)
                {
                    //set the parent as a child to set pivot in correct location
                    newParent.SetParent(gameObjectToCheck.transform, true);

                    newParent.transform.localRotation = Quaternion.identity;

                    //set grab tag
                    newParent.gameObject.tag = TagList.interactable;

                    BoxCollider tempCollider = newParent.gameObject.AddComponent<BoxCollider>();

                    tempCollider.size = filter.mesh.bounds.size;
                    newParent.localPosition = filter.mesh.bounds.center;

                    if (isNetworked) ClientSpawnManager.Instance.CreateNetworkedGameObject(newParent.gameObject, menuButtonIndex);
                }
                else if (hasSkinnedRenderer) // NOTE(david): animated objects are considered to have skinned mesh renderers and given whole collider
                {
                    //set anim objects as one whole object
                    if (skinnedRenderer.rootBone)
                    {

                    }
                }
                return newParent;
            }
            return null;
        }

        /// <summary>
        /// Recursively set up rigid bodies and colliders for the given transform and all children.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="menuButtonIndex"></param>
        /// <param name="childToNewParentPivot"></param>
        static void AddRigidBodiesAndColliders(Transform transform, int menuButtonIndex, ref Dictionary<Transform, Transform> childToNewParentPivot)
        {
            //check that we are not looking at a transform that we already looked at
            if (!childToNewParentPivot.ContainsValue(transform))
            {
                //Check parent first for renderers 
                Transform newPO2 = CheckForIndividualFiltersAndSkins(transform.gameObject, menuButtonIndex);

                if (newPO2 != null)
                    childToNewParentPivot.Add(newPO2, transform);
            }

            //check children for renderers
            foreach (Transform child in transform)
            {
                Transform newParent = CheckForIndividualFiltersAndSkins(child.gameObject, menuButtonIndex);

                //if we made a new pivot parent add it to the dictionary to iterate after completing iteraction for the specific model
                if (newParent != null) childToNewParentPivot.Add(newParent, child);

                //deeper children search //resets our dic
                if (child.childCount > 0) AddRigidBodiesAndColliders(child, menuButtonIndex, ref childToNewParentPivot);
            }
        }

        /// <summary>
        /// Since we cannot add parents while we iterate through children to set their colliders and netobjects,
        /// we have to do this after the fact.
        /// </summary>
        /// <param name="pivot"></param>
        /// <param name="child"></param>
        public static void SetParentAsPivot(Transform pivot, Transform child)
        {
            //grab the original parent before changing it
            Transform previousParent = child.parent;

            //remove new parent child reference (used to obtain center pivot)
            child.DetachChildren();

            //now attach child to new parent to have pivot point parent
            child.transform.SetParent(pivot, true);

            //set pivot to be parented to the original child parent
            pivot.transform.SetParent(previousParent);
        }

    }
}
