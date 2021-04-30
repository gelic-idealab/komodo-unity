var MemoryStatsPlugin = {

	GetTotalMemorySize: function() {
		return TOTAL_MEMORY; // WebGLMemorySize in bytes
	},

	GetTotalStackSize: function() {
		return TOTAL_STACK;
	},

	GetStaticMemorySize: function() {
		return STATICTOP - STATIC_BASE;
	},

	GetDynamicMemorySize: function() {
		if (typeof DYNAMICTOP !== 'undefined') {
		    return DYNAMICTOP - DYNAMIC_BASE;
		}
		else {
			// Unity 5.6+
			return HEAP32[DYNAMICTOP_PTR >> 2] - DYNAMIC_BASE;
		}
    },

    GetUsedMemorySize: function () {
        var dynMem;
		if (typeof DYNAMICTOP !== 'undefined') {
		    dynMem = DYNAMICTOP - DYNAMIC_BASE;
		}
		else {
			// Unity 5.6+
			dynMem = HEAP32[DYNAMICTOP_PTR >> 2] - DYNAMIC_BASE;
		}
        return TOTAL_STACK + (STATICTOP - STATIC_BASE) + dynMem;
    },
    
    LogMemoryStatsInMegabytes: function () {
        var dynMem;
		if (typeof DYNAMICTOP !== 'undefined') {
		    dynMem = DYNAMICTOP - DYNAMIC_BASE;
		}
		else {
			// Unity 5.6+
			dynMem = HEAP32[DYNAMICTOP_PTR >> 2] - DYNAMIC_BASE;
		}
        dynMem = dynMem / 1024.0 / 1024.0;
        var totalMem = TOTAL_MEMORY / 1024.0 / 1024.0;
        var totalStack = TOTAL_STACK / 1024.0 / 1024.0;
        var staticMem = (STATICTOP - STATIC_BASE) / 1024.0 / 1024.0;
        console.info("Highwater Used Memory: " + totalMem + "MB.      Highwater Dynamic Memory: " + dynMem + "MB.\n" + "Stack Memory: " + totalStack + "MB.\n" + "Static Memory: " + staticMem + "MB.\n");
    },

    GetJSHeapMaxSize: function (doLog) {
        if (!window || 
            !window.performance ||
            !window.performance.memory ||
            !window.performance.memory.jsHeapSizeLimit) {
                console.warn("Was not able to get JS Heap Max Size: unsupported in browser. All further memory computations that rely on this value will be incorrect. Returning max uint value.")
                return Number.MAX_VALUE;
            }
        var sizeInBytes = window.performance.memory.jsHeapSizeLimit;
        return sizeInBytes;
    }
};

mergeInto(LibraryManager.library, MemoryStatsPlugin);
