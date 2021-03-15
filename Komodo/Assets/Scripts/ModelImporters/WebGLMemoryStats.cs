using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;

namespace Komodo.AssetImport
{
    public static class WebGLMemoryStats
    {
        public static bool doLogStats = false;

        public static float numSecondsToWait = 5f;

        public static uint managedHeapUsed;

        public static uint managedHeapTotal;

        public static uint highwaterUsed;

        private static uint _memoryLimit;

        private static uint _padding = 15 * 1024 * 1024;

        private static uint mobileVsDesktopThreshold = 1024 * 1024 * 1024;

        private static uint mobileMemoryLimit = 256 * 1024 * 1024;

        private static uint desktopMemoryLimit = 0;

        [DllImport("__Internal")]
        public static extern uint GetTotalMemorySize();

        [DllImport("__Internal")]
        public static extern uint GetTotalStackSize();

        [DllImport("__Internal")]
        public static extern uint GetStaticMemorySize();

        [DllImport("__Internal")]
        private static extern uint GetDynamicMemorySize();

        [DllImport("__Internal")]
        private static extern uint GetUsedMemorySize();

        [DllImport("__Internal")]
        public static extern void LogMemoryStatsInMegabytes();

        [DllImport("__Internal")]
        private static extern uint GetJSHeapMaxSize(bool doLogStats);

        public static void LogStats()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            LogMemoryStatsInMegabytes();
# else 
            Debug.Log("Simulating LogStats(): " + "Highwater Used Memory: " + "unknown" + "MB.      Highwater Dynamic Memory: " + "unknown" + "MB.\n" + "Stack Memory: " + "unknown" + "MB.\n" + "Static Memory: " + "unknown" + "MB.\n");
#endif
        }

        public static void LogMoreStats(string message)
        {
            Debug.Log(GetMoreStats(message));
        }

        public static string GetMoreStats(string message)
        {
            return ($"{message}: Managed heap: {ToRoundedMB(Profiler.GetMonoUsedSizeLong(), 2)} / {ToRoundedMB(Profiler.GetMonoHeapSizeLong(), 2)}MB.    "
                + $"Highwater used: {ToRoundedMB(GetTotalUsedMemorySize(), 2)}MB.\n");
        }

        /*
        * Naive implementation: check the Max JS Heap Size. If it exists, we are on Chrome, and if its value is less than an empirical amount, we should mark ourselves as mobile. 
        */
        public static void ChooseMemoryLimitForDevice(bool forceMobileMemoryLimit)
        {
            uint heapMaxSize = WebGLMemoryStats.GetHeapMaxSize(true);

            Debug.Log("iFrame JS Heap Size Limit: " + ToRoundedMB(heapMaxSize, 2) + "MB");

            if (heapMaxSize == 0 && !forceMobileMemoryLimit)
            {
                _memoryLimit = 0;
                Debug.Log($"Set Memory Limit to {ToRoundedMB(_memoryLimit, 2)}MB.");
                return;
            }

            if (forceMobileMemoryLimit || heapMaxSize < mobileVsDesktopThreshold)
            {
                _memoryLimit = mobileMemoryLimit;
                Debug.Log($"Set Memory Limit to {ToRoundedMB(_memoryLimit, 2)}MB.");
                return;
            }

            _memoryLimit = desktopMemoryLimit;
            Debug.Log($"Set Memory Limit to {ToRoundedMB(_memoryLimit, 2)}MB.");
            return;
        }

        public static void InitMemoryLimit(uint size)
        {
            _memoryLimit = size;
        }

        public static uint GetMemoryLimit()
        {
            return _memoryLimit;
        }

        //   <   UNITY_HEAP <?>  < MGD <MGD_FREE> <MGD_USED> >  <?>   >
        public static bool HasEnoughMemoryToLoadBytes(long sizeToLoad)
        {
            if (_memoryLimit == 0)
            {
                Debug.LogWarning("Detected memory limit of 0. Allowing load of any size.");
                return true;
            }

            //This is the used and free memory in the managed heap. 
            long totalManagedHeapSize = Profiler.GetMonoHeapSizeLong();

            //Even if the object can fit in the managed heap, it still might grow by some amount.
            //Note(Brandon): I am not sure what this amount truly is. This is just a guess based on the data I've seen so far.
            long bumpedHeapSize = sizeToLoad + _padding + totalManagedHeapSize;

            if (sizeToLoad < GetFreeMemoryInManagedHeap())
            {
                if (bumpedHeapSize >= (long)_memoryLimit)
                {
                    Debug.LogWarning(
                    $"Bumped heap cannot fit. It would grow to {ToRoundedMB(bumpedHeapSize, 2)}MB, but is limited to {ToRoundedMB(_memoryLimit, 2)}MB.");
                    return false;
                }

                Debug.Log($"Bumped heap ({ToRoundedMB(bumpedHeapSize, 2)}MB) can fit. Loading.");
                return true;
            }

            //Otherwise, the managed heap will double in size.
            long doubledHeapSize = totalManagedHeapSize * 2 + _padding;

            if (doubledHeapSize >= (long)_memoryLimit)
            {
                Debug.LogWarning(
                    $"Doubled heap cannot fit. It would grow to {ToRoundedMB(totalManagedHeapSize * 2 + _padding, 2)}MB, but is limited to {ToRoundedMB(_memoryLimit, 2)}MB."
                );
                return false;
            }

            Debug.Log($"Doubled heap ({ToRoundedMB(totalManagedHeapSize * 2 + _padding, 2)}MB) can fit. Loading.");

            return true;
        }
        
        public static long GetFreeMemoryInManagedHeap()
        {
            uint memoryLimit = GetMemoryLimit();
            long managedHeap = Profiler.GetMonoHeapSizeLong();
            long freeInManagedHeap = managedHeap - Profiler.GetMonoUsedSizeLong();

            if (doLogStats)
            {
                Debug.Log(
                    $"Free in Managed Heap: {ToRoundedMB(freeInManagedHeap, 2)}MB.\n"
                    + $"Memory limit:     {ToRoundedMB(memoryLimit, 2)}MB.\n"
                );
            }

            return freeInManagedHeap;
        }

        public static long GetMemoryLimitMinusManagedHeap()
        {
            uint memoryLimit = GetMemoryLimit();
            long managedHeap = Profiler.GetMonoHeapSizeLong();
            long memoryLimitMinusManagedHeap = ((long)memoryLimit) - managedHeap;

            if (doLogStats)
            {
                Debug.Log(
                    $"MemLimit minus MgdHeap: {ToRoundedMB(memoryLimitMinusManagedHeap, 2)}MB.\n"
                    + $"Memory limit:           {ToRoundedMB(memoryLimit, 2)}MB.\n"
                );
            }

            return memoryLimitMinusManagedHeap;
        }

        public static uint GetHeapMaxSize(bool doLogThis)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return GetJSHeapMaxSize(doLogThis);
# else 
            Debug.LogWarning("Simulating GetHeapMaxSize(): returning maximum uint Value()");
            return uint.MaxValue;
#endif
        }

        public static uint GetTotalUsedMemorySize()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return GetUsedMemorySize();
# else 
            Debug.LogWarning("Simulating GetTotalUsedMemorySize(): returning 0");
            return 0;
#endif
        }

        public static double ToRoundedMB(ulong value, int places)
        {
            if (places < 0)
            {
                Debug.LogWarning("Places was less than 0. Setting to 0.");
                places = 0;
            }
            return Math.Round(value / 1024d / 1024d, places);
        }

        public static double ToRoundedMB(long value, int places)
        {
            if (places < 0)
            {
                Debug.LogWarning("Places was less than 0. Setting to 0.");
                places = 0;
            }
            return Math.Round(value / 1024d / 1024d, places);
        }
    }
}
