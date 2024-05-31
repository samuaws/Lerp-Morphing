using UnityEngine;
using Torec;

public class MeshUtilities : MonoBehaviour
{
    /// <summary>
    /// Check if two meshes have approximately the same number of vertices.
    /// </summary>
    /// <param name="mesh1">First mesh.</param>
    /// <param name="mesh2">Second mesh.</param>
    /// <param name="tolerance">Tolerance level for comparison.</param>
    /// <returns>True if the number of vertices in both meshes is similar, false otherwise.</returns>
    public static bool AreMeshesSimilar(Mesh mesh1, Mesh mesh2, int tolerance = 5)
    {
        int vertexCount1 = mesh1.vertices.Length;
        int vertexCount2 = mesh2.vertices.Length;

        return Mathf.Abs(vertexCount1 - vertexCount2) <= tolerance;
    }

    /// <summary>
    /// Subdivide a mesh until it has approximately the same number of vertices as another mesh.
    /// </summary>
    /// <param name="meshToSubdivide">Mesh to subdivide.</param>
    /// <param name="targetVertexCount">Target vertex count.</param>
    /// <returns>Subdivided mesh.</returns>
    public static Mesh SubdivideMesh(Mesh meshToSubdivide, int targetVertexCount)
    {
        while (meshToSubdivide.vertices.Length < targetVertexCount)
        {
            print("subdivide");
            //meshToSubdivide = Subdivide(meshToSubdivide);
            meshToSubdivide = CatmullClark.Subdivide(meshToSubdivide,1);
        }
        return meshToSubdivide;
    }

    /// <summary>
    /// Subdivide a mesh by splitting each triangle into four triangles.
    /// </summary>
    /// <param name="originalMesh">Original mesh to subdivide.</param>
    /// <returns>Subdivided mesh.</returns>
    private static Mesh Subdivide(Mesh originalMesh)
    {
        Vector3[] originalVertices = originalMesh.vertices;
        int[] originalTriangles = originalMesh.triangles;

        int originalVertexCount = originalVertices.Length;
        int originalTriangleCount = originalTriangles.Length / 3;

        Vector3[] subdividedVertices = new Vector3[originalVertexCount + originalTriangleCount * 3];
        int[] subdividedTriangles = new int[originalTriangleCount * 4 * 3];

        // Copy original vertices
        System.Array.Copy(originalVertices, subdividedVertices, originalVertexCount);

        int vertexIndex = originalVertexCount;

        for (int i = 0; i < originalTriangleCount; i++)
        {
            // Get vertices of the original triangle
            Vector3 v0 = originalVertices[originalTriangles[i * 3]];
            Vector3 v1 = originalVertices[originalTriangles[i * 3 + 1]];
            Vector3 v2 = originalVertices[originalTriangles[i * 3 + 2]];

            // Calculate midpoints
            Vector3 m01 = (v0 + v1) * 0.5f;
            Vector3 m12 = (v1 + v2) * 0.5f;
            Vector3 m20 = (v2 + v0) * 0.5f;

            // Add new vertices
            subdividedVertices[vertexIndex] = m01;
            subdividedVertices[vertexIndex + 1] = m12;
            subdividedVertices[vertexIndex + 2] = m20;

            // Add new triangles
            subdividedTriangles[i * 12] = originalTriangles[i * 3];
            subdividedTriangles[i * 12 + 1] = vertexIndex;
            subdividedTriangles[i * 12 + 2] = vertexIndex + 2;

            subdividedTriangles[i * 12 + 3] = vertexIndex;
            subdividedTriangles[i * 12 + 4] = originalTriangles[i * 3 + 1];
            subdividedTriangles[i * 12 + 5] = vertexIndex + 1;

            subdividedTriangles[i * 12 + 6] = vertexIndex + 2;
            subdividedTriangles[i * 12 + 7] = vertexIndex;
            subdividedTriangles[i * 12 + 8] = vertexIndex + 1;

            subdividedTriangles[i * 12 + 9] = vertexIndex + 1;
            subdividedTriangles[i * 12 + 10] = vertexIndex + 2;
            subdividedTriangles[i * 12 + 11] = originalTriangles[i * 3 + 2];

            // Increment vertex index
            vertexIndex += 3;
        }

        // Create a new mesh and assign vertices and triangles
        Mesh subdividedMesh = new Mesh();
        subdividedMesh.vertices = subdividedVertices;
        subdividedMesh.triangles = subdividedTriangles;

        // Recalculate normals and bounds
        subdividedMesh.RecalculateNormals();
        subdividedMesh.RecalculateBounds();

        return subdividedMesh;
    }
}
