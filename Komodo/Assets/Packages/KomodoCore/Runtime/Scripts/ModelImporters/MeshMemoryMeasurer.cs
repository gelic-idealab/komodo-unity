using System;

namespace Komodo.AssetImport
{
    public static class MeshMemoryMeasurer
    {
        const uint BYTES_PER_FLOAT = 4;

        const uint FLOATS_PER_VEC4 = 4;

        const uint SIDES_PER_TRI = 3;

        /**
        *  "Estimate the number of bytes used to index each vertex in the list of polygons."
        */ 
        static uint EstimateVerticesSize (uint triangles)
        {
            return SIDES_PER_TRI * triangles * 2;
        }

        /**
        * "Estimate Pose info bytes for each vertex."
        */
        static uint EstimatePoseInfoSize (uint vertices)
        {
            return vertices * SIDES_PER_TRI * BYTES_PER_FLOAT;
        }

        /**
        * "Estimate Normal coordinate bytes usage per vertex."
        */
        static uint EstimateNormalsSize (uint vertices)
        {
            return vertices * SIDES_PER_TRI * BYTES_PER_FLOAT;
        }

        /**
        * "Estimate the UV coordinate bytes usage per vertex.
        * "Assume each vertex has one UV channel, unless we know the total."
        */
        static uint EstimateUVsSize(uint vertices) 
        {
            const uint COORDS_PER_UV = 2;
            return vertices * COORDS_PER_UV * BYTES_PER_FLOAT;
        }

        /**
        * "Estimate the Tangents bytes usage per vertex."
        */
        static uint EstimateTangentsSize (uint vertices)
        {
            return vertices * FLOATS_PER_VEC4 * BYTES_PER_FLOAT;
        }

        /**
        * "Estimate colors bytes usage per vertex."
        */
        static uint EstimateColorsSize (uint vertices)
        {
            return vertices * FLOATS_PER_VEC4 * BYTES_PER_FLOAT;
        }

        /**
        * Estimate the pessimistic size a mesh might occupy in memory.
        */
        public static uint EstimateSize (uint triangles, uint vertices) {
            uint totalSize = (
                EstimateVerticesSize(triangles) 
                + EstimatePoseInfoSize(vertices)
                + EstimateNormalsSize(vertices)
                + EstimateUVsSize(vertices)
                + EstimateTangentsSize(vertices)
                + EstimateColorsSize(vertices)
            );
            return totalSize;
        }
    }
}