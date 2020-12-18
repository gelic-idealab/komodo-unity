using UnityEngine;

namespace BzKovSoft.ObjectSlicerSamples
{
	public class AdderSliceableAsync : MonoBehaviour
	{
		void Start()
		{
			var rigids = GetComponentsInChildren<Rigidbody>();

			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];
				var go = rigid.gameObject;

				if (go == gameObject)
					continue;

				if (go.GetComponent<KnifeSliceableAsync>() != null)
					continue;

				go.AddComponent<KnifeSliceableAsync>();
			}
		}
	}
}