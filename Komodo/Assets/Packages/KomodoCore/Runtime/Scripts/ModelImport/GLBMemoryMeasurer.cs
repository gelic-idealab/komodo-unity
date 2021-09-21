using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Komodo.AssetImport
{
    public class PrimitiveInfo 
    {
        public uint triangles;
        public uint vertices;

        public PrimitiveInfo (uint triangles, uint vertices) {
            this.triangles = triangles;
            this.vertices = vertices;
        }
    }

    public class TextureInfo
    {
        public uint width;
        public uint height;
        public uint bitDepth;
        public uint samples;
        public bool isMipMapOn;

        public TextureInfo (uint width, uint height, uint bitDepth, uint samples, bool isMipMapOn) 
        {
            this.width = width;
            this.height = height;
            this.bitDepth = bitDepth;
            this.samples = samples;
            this.isMipMapOn = isMipMapOn;
        }
    }
    

    public static class GLBMemoryMeasurer
    {

        private static List<PrimitiveInfo> _primitives;

        private static List<TextureInfo> _textures = new List<TextureInfo>();

        public static void AddPrimitive (PrimitiveInfo primitive) 
        {
            _primitives.Add(primitive);
        }

        public static void AddTexture (TextureInfo texture) 
        {
            _textures.Add(texture);
        }

        public static void AddPrimitives (ModelRoot model)
        {
            if (model == null)
            {
                return;
            }
            var logicalMeshes = model.LogicalMeshes;
            for (int i = 0; i < logicalMeshes.Count; i += 1)
            {
                var primitives = logicalMeshes[i].Primitives;

                for (int j = 0; j < primitives.Count; j += 1)
                {
                    uint vertices = (uint) primitives[j].GetVertexAccessor("POSITION")?.AsVector3Array().Count;

                    var triangleIndices = primitives[j].GetTriangleIndices();

                    // Note(Brandon): for some reason, tris.Count() and tris.ToArray() are not working, so we have to count manually like this.
                    uint triangles = 0; 
                    foreach (var (ta, tb, tc) in triangleIndices) 
                    {
                        triangles += 1;
                    }

                    _primitives.Add(new PrimitiveInfo(triangles, vertices));
                    
                    //Debug.Log($"{i}.{j}: {vertices} vertices, {triangles} triangles");
                }
                //TODO do the same for textures
            }
        }

        public static void AddTextures(ModelRoot model)
        {
            if (model == null) 
            {
                return;
            }
            const bool USE_MIPMAP = true;
            const uint PNG_SAMPLES = 4;
            const uint JPEG_SAMPLES = 3;

            var images = model.LogicalImages;
            for (int i = 0; i < images.Count; i += 1)
            {
                using (var stream = images[i].Content.Open())
                {
                    IImageFormat format;
                    IImageInfo info = SixLabors.ImageSharp.Image.Identify(stream, out format);
                    if (info == null)
                    {
                        Debug.LogError($"Could not get image info for image {i}. Texture size will be underestimated.");
                        return;
                    }

                    uint samples = PNG_SAMPLES; //choose worst-case

                    if (format.DefaultMimeType == "image/jpeg") {
                        samples = JPEG_SAMPLES;
                    }

                    if (format.DefaultMimeType == "image/png") {
                        samples = PNG_SAMPLES;
                    }

                    AddTexture(
                        new TextureInfo(
                            (uint) info.Width, 
                            (uint) info.Height, 
                            (uint) info.PixelType.BitsPerPixel / samples, 
                            samples, 
                            USE_MIPMAP
                        )
                    );
                }
            }
        }

        private static void _ClearPrimitivesList () {
            _primitives = new List<PrimitiveInfo>();
        }

        private static void _ClearTexturesList () {
            _textures = new List<TextureInfo>();
        }

        public static void SetModel (string path) {
            //WebGLMemoryStats.LogMoreStats("GLBMemoryMeasurer.SetModel.Load BEFORE");

            ModelRoot model = null;
            try {
                model = SharpGLTF.Schema2.ModelRoot.Load(path);
            } catch (System.Exception e) {
                Debug.LogWarning(e.Message + ". Proceeding anyways, assuming model is 0MB.");
            }
            //WebGLMemoryStats.LogMoreStats("GLBMemoryMeasurer.SetModel.Load AFTER");

            _ClearPrimitivesList();
            AddPrimitives(model);

            _ClearTexturesList();
            AddTextures(model);
        }

        public static uint EstimateSize () 
        {
            uint totalSize = 0;

            uint totalTriangles = 0;
            uint totalVertices = 0;

            //WebGLMemoryStats.LogMoreStats("GLBMemoryMeasurer.EstimateSize Meshes BEFORE");
            for (int i = 0; i < _primitives.Count; i += 1)
            {
                uint size = MeshMemoryMeasurer.EstimateSize(_primitives[i].triangles, _primitives[i].vertices);

                totalSize += size;

                totalTriangles += _primitives[i].triangles;
                totalVertices += _primitives[i].vertices;
            }
            //WebGLMemoryStats.LogMoreStats("GLBMemoryMeasurer.EstimateSize Meshes AFTER");

            Debug.Log($"Estimated size (meshes): {WebGLMemoryStats.ToRoundedMB(totalSize, 2)} MB.                  ({totalVertices} vertices, {totalTriangles} triangles)");

            for (int i = 0; i < _textures.Count; i += 1)
            {
                //WebGLMemoryStats.LogMoreStats("GLBMemoryMeasurer.EstimateSize Textures BEFORE");
                
                uint size = TextureMemoryMeasurer.EstimateSize(_textures[i].width, _textures[i].height, _textures[i].bitDepth, _textures[i].samples, _textures[i].isMipMapOn);
                
                //WebGLMemoryStats.LogMoreStats("GLBMemoryMeasurer.EstimateSize Textures AFTER");

                totalSize += size;

                Debug.Log($"Estimated texture #{i} size: {WebGLMemoryStats.ToRoundedMB(size, 2)} MB.                  {_textures[i].width}x{_textures[i].height} / {_textures[i].bitDepth}-bit depth / {_textures[i].samples} samples / mipmap = {_textures[i].isMipMapOn}.");
            }

            //Debug.Log($"Estimated size (total): {WebGLMemoryStats.ToRoundedMB(totalSize, 2)} MB.");

            return totalSize;
        }
    }
}