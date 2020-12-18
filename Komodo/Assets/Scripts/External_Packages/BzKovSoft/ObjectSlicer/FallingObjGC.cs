using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
    public class FallingObjGC : MonoBehaviour
    {
#pragma warning disable 0649
		[SerializeField]
		bool _enableLog = true;
		[SerializeField]
		int _delaySec = 10;
		[SerializeField]
        float _minPosY = -10f;
#pragma warning restore 0649
		float _nextTime = 0f;
        
        void Update()
        {
            if (Time.time < _nextTime)
                return;

            _nextTime = Time.time + _delaySec;
            
            var objects = Resources.FindObjectsOfTypeAll(typeof(BzSliceableBase));
            
            for (int i = 0; i < objects.Length; i++)
            {
                var go =((BzSliceableBase)objects[i]).gameObject;
                if (go.transform.position.y < _minPosY)
                {
                    if (_enableLog)
                        Debug.Log("Destroyed by GC: " + go.name);

                    UnityEngine.Object.Destroy(go);
                }
            }
        }
    }
}