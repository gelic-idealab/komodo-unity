using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.AssetImport
{
    public static class TextureMemoryMeasurer
    {
        static uint AdjustSizeForMipmapping (uint rawSize, bool isMipmapOn)
        {
            if (isMipmapOn) 
            {
                return (uint) (1.334d * (double) rawSize);
            }

            return rawSize;
        }

        public static uint EstimateSize (uint width, uint height, uint bitDepth, uint samples, bool isMipmapOn) 
        {
            uint bitsPerPixel = bitDepth * samples;
            uint bytesPerPixel = bitsPerPixel / 8;
            uint rawSize = width * height * bytesPerPixel;
            uint sizeInRAM = AdjustSizeForMipmapping(rawSize, isMipmapOn);
            uint sizeInVRAM = sizeInRAM;

            return sizeInRAM + sizeInVRAM;
        }

        public static uint EstimateBitsPerTexel (uint width, uint height, uint bitDepth, uint samples, bool isMipmapOn)
        {
            uint size = EstimateSize(width, height, bitDepth, samples, isMipmapOn);
            uint bitsPerTexel = size / width / height;
            return bitsPerTexel;
        }
    }
}
