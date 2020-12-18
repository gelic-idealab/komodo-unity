
using UnityEngine;

public static class UnityExtensionMethods
{
    /// <summary>
    /// Determines whether the quaternion is safe for interpolation or use with transform.rotation.
    /// </summary>
    /// <returns><c>false</c> if using the quaternion in Quaternion.Lerp() will result in an error (eg. NaN values or zero-length quaternion).</returns>
    /// <param name="quaternion">Quaternion.</param>
    public static bool IsValid(this Quaternion quaternion)
    {
        bool isNaN = float.IsNaN(quaternion.x + quaternion.y + quaternion.z + quaternion.w);

        bool isZero = quaternion.x == 0 && quaternion.y == 0 && quaternion.z == 0 && quaternion.w == 0;


       // bool twoZeros = false;
       //two se
        //if (quaternion.x == 0 && quaternion.y == 0)
        //    quaternion.x = 0.001f;
        //if (quaternion.y == 0 && quaternion.z == 0)
        //    quaternion.y = 0.001f;
        //if (quaternion.x == 0 && quaternion.z == 0)
        //    quaternion.z = 0.001f;



        //int zeroVectors2 = 0;

        //if (quaternion.x == 0)
        //    zeroVectors2++;
        //if (quaternion.y == 0)
        //    zeroVectors2++;
        //if (quaternion.z == 0)
        //    zeroVectors2++;
        //if (zeroVectors2 > 1)
        //    twoZeros = true;


        return !(isNaN || isZero );
    }
    public static bool IsValid(this Vector4 vector4)
    {
        bool isNaN = float.IsNaN(vector4.x + vector4.y + vector4.z + vector4.w);

        bool isZero = vector4.x == 0 && vector4.y == 0 && vector4.z == 0 && vector4.w == 0;

        //if (vector4.x == 0 && vector4.y == 0)
        //    vector4.x = 0.001f;
        //if (vector4.y == 0 && vector4.z == 0)
        //    vector4.y = 0.001f;
        //if (vector4.x == 0 && vector4.z == 0)
        //    vector4.z = 0.001f;
        //bool twoZeros = false;

        //int zeroVectors2 = 0;
        //if (vector4.x == 0)
        //    zeroVectors2++;
        //if (vector4.y == 0)
        //    zeroVectors2++;
        //if (vector4.z == 0)
        //    zeroVectors2++;
        //if (zeroVectors2 > 1)
        //    twoZeros = true;


        return !(isNaN || isZero);
    }

    public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
    }
}
