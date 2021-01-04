using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Siccity.GLTFUtility {
	/// <summary> Defines which shaders to use in the gltf import process </summary>
	[Serializable]
	public class ShaderSettings {
		[SerializeField] private Shader metallic;
		public Shader Metallic { get { return metallic != null ? metallic : GetDefaultMetallic(); } }

		[SerializeField] private Shader metallicBlend;
		public Shader MetallicBlend { get { return metallicBlend != null ? metallicBlend : GetDefaultMetallicBlend(); } }

		[SerializeField] private Shader specular;
		public Shader Specular { get { return specular != null ? specular : GetDefaultSpecular(); } }

		[SerializeField] private Shader specularBlend;
		public Shader SpecularBlend { get { return specularBlend != null ? specularBlend : GetDefaultSpecularBlend(); } }

		/// <summary> Caches default shaders so that async import won't try to search for them while on a separate thread </summary>
		public void CacheDefaultShaders() {
			metallic = Metallic;
			metallicBlend = MetallicBlend;
			specular = Specular;
			specularBlend = SpecularBlend;
		}
		//using default built in material not Universal Rendering Pipeline wich need to be added manually through editor project settings available shaders to be picked up in build
		public Shader GetDefaultMetallic() => Shader.Find("GLTFUtility/Standard (Metallic)");
		

		public Shader GetDefaultMetallicBlend() =>	Shader.Find("GLTFUtility/Standard Transparent (Metallic)");
		

		public Shader GetDefaultSpecular() => Shader.Find("GLTFUtility/Standard (Specular)");
		

		public Shader GetDefaultSpecularBlend() => Shader.Find("GLTFUtility/Standard Transparent (Specular)");
		
	}
}