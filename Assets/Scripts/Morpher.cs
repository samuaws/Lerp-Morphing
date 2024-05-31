using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Vertex Pair maps two verices to each other, used to find the nearest vertex to a given vertex.
/// </summary>
public class VertexPair
{
    public Vector3 Vertex1 { get; set; }
    public Vector3 Vertex2 { get; set; }

    public VertexPair(Vector3 vertex1, Vector3 vertex2)
    {
        Vertex1 = vertex1;
        Vertex2 = vertex2;
    }
}


/// <summary>
/// Morpher is responsible for morphing between two meshe.
/// It uses the slider value to determine the percentage of the morphing.
/// </summary>
public class Morpher : MonoBehaviour
{
    public bool IsDeforming = true;

    [Tooltip("Mesh should be read/write enabled from the model import settings")]
    [SerializeField] private Mesh _oldMesh;
    [Tooltip("Mesh should be read/write enabled from the model import settings")]
    [SerializeField] private Mesh _newMesh;

    [SerializeField] private Material _oldMat;
    [SerializeField] private Material _newMat;

    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private Renderer _renderer;

    [Range(0f, 1f)]
    [SerializeField] private float _slider;
    [SerializeField] private bool subdivide = false;
    [SerializeField] private bool mesh1Geometry = false;


    private Vector3[] _oldVertices;
    private Vector3[] _newVertices;

    private int[] _oldTriangles;
    private int[] _newTriangles;

    private List<Vector3> _finalVertices;

    private Mesh _interpolatedMesh;
    private List<VertexPair> _pairsOfVertices1;

    private Material _finalMaterial;

    void Start()
    {
        _interpolatedMesh = new Mesh();
        _interpolatedMesh.MarkDynamic();

        if (_meshFilter != null)
        {
            _meshFilter.mesh = _interpolatedMesh;
        }
        if (subdivide) EnsureSimilarVertices( _oldMesh,  _newMesh);

        _oldVertices = _oldMesh.vertices;
        _newVertices = _newMesh.vertices;

        _oldTriangles = _oldMesh.triangles;
        _newTriangles = _newMesh.triangles;

        _finalVertices = new List<Vector3>(_oldVertices);
        if (mesh1Geometry) CreatePairs1();
        else CreatePairs2();
        _finalMaterial = new Material(_slider < 0.5 ? _oldMat : _newMat);

    }

    /// <summary>
    /// Create pairs of vertices, each pair contains a vertex from the old mesh and a vertex from the new mesh.
    /// Vertices are mapped by the nearest one to each other.
    /// </summary>
    private void CreatePairs1()
    {
        _pairsOfVertices1 = new List<VertexPair>();

        for (int i = 0; i < _oldVertices.Length; i++)
        {
            var oldVertex = _oldVertices[i];

            var nearestToOldVertex = _newVertices.OrderBy(v => Vector3.Distance(v, oldVertex)).FirstOrDefault();

            _pairsOfVertices1.Add(new VertexPair(oldVertex, nearestToOldVertex));
        }
    }
    private void CreatePairs2()
    {
        _pairsOfVertices1 = new List<VertexPair>();

        for (int i = 0; i < _newVertices.Length; i++)
        {
            var newVertex = _newVertices[i];

            var nearestToNewVertex = _oldVertices.OrderBy(v => Vector3.Distance(v, newVertex)).FirstOrDefault();

            _pairsOfVertices1.Add(new VertexPair(nearestToNewVertex, newVertex));
        }
    }



    private void EnsureSimilarVertices(Mesh mesh1, Mesh mesh2)
    {
        // Check if meshes have similar number of vertices
        if (!MeshUtilities.AreMeshesSimilar(mesh1, mesh2))
        {
            // Determine which mesh has fewer vertices
            Mesh smallerMesh = mesh1.vertices.Length < mesh2.vertices.Length ? mesh1 : mesh2;
            Mesh largerMesh = smallerMesh == mesh1 ? mesh2 : mesh1;

            // Subdivide the smaller mesh until it has similar vertices as the larger mesh
            smallerMesh = MeshUtilities.SubdivideMesh(smallerMesh, largerMesh.vertices.Length);

            // Assign the subdivided mesh back to the original mesh
            if (mesh1.vertices.Length < mesh2.vertices.Length)
            {
                print("hi");
                _oldMesh = smallerMesh;
            }
            else
            {
                _newMesh = smallerMesh;
            }
        }
    }




    void Update()
    {
        if (IsDeforming)
        {
            Deform();
        }
    }

    public void SetSlider(float slider)
    {
        _slider = slider;
    }

    private void Deform()
    {
        List<Vector3> finalVertices = new List<Vector3>();

        foreach (var pair in _pairsOfVertices1)
        {
            Vector3 interpolatedVertex = Vector3.Lerp(pair.Vertex1, pair.Vertex2, _slider);
            finalVertices.Add(interpolatedVertex);
        }
        _interpolatedMesh.Clear();
        _interpolatedMesh.SetVertices(finalVertices);

        if (mesh1Geometry)
        {
            // Use triangles, bounds, and UVs of the old mesh
            _interpolatedMesh.triangles = _oldTriangles;
            _interpolatedMesh.bounds = _oldMesh.bounds;
            _interpolatedMesh.uv = _oldMesh.uv;
            _interpolatedMesh.uv2 = _oldMesh.uv2;
            _interpolatedMesh.uv3 = _oldMesh.uv3;
        }
        else
        {
            // Use triangles, bounds, and UVs of the old mesh
            _interpolatedMesh.triangles = _newTriangles;
            _interpolatedMesh.bounds = _newMesh.bounds;
            _interpolatedMesh.uv = _newMesh.uv;
            _interpolatedMesh.uv2 = _newMesh.uv2;
            _interpolatedMesh.uv3 = _newMesh.uv3;
        }



        // Recalculate normals based on the morphed vertices
        _interpolatedMesh.RecalculateNormals();

        // Lerp material properties of the old mesh
        _finalMaterial.Lerp(_oldMat, _newMat, _slider);
        _finalMaterial.SetTexture("_MainTex", _oldMat.GetTexture("_MainTex"));

        _renderer.material = _finalMaterial;
    }
    
}