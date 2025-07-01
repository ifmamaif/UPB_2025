using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine;
using UnityEditor;

public class MeshGenerator : MonoBehaviour
{
    #region Components
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    #endregion

    private Material _materialProcedural;
    private Mesh _mesh;

    private GameObject _water;



    private Vector3[] _vertices;
    private int[] _triangles;
    private Color[] _colors;
    private float _minTerrainHeight;
    private float _maxTerrainHeight;

    public int xSize = 100;
    public int zSize = 100;
    public float ScallingNoiseParam = 2f;
    public float ScallingNoise = 20f;
    public float OffsetNoise = 0;
    public Gradient Gradient;

    public bool ShouldUpdate = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _meshFilter = gameObject.GetComponent<MeshFilter>();
        if(_meshFilter == null)
            _meshFilter = gameObject.AddComponent<MeshFilter>();

        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();

        _mesh = new Mesh();
        _materialProcedural = GetCreateMaterialAsset("Assets/Materials/procedural_terrain_material.mat", "Shader Graphs/procedural_terrain_texture");
        _meshRenderer.sharedMaterial = _materialProcedural;


        CreateTerrain();
        UpdateMesh();
        
        _water = CreateWaterGameObject();
        UpdateWater(_water);
    }

    // Update is called once per frame
    void Update()
    {
        if(!ShouldUpdate)
            return;

        CreateTerrain();
        UpdateMesh();
        UpdateWater(_water);
    }

    private void CreateTerrain()
    {
        _vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (var x = 0; x <= xSize; x++)
            {
                var xNoise = (float)x / xSize * ScallingNoiseParam + OffsetNoise;
                var zNoise = (float)z / xSize * ScallingNoiseParam + OffsetNoise;

                var y = Mathf.PerlinNoise(xNoise, zNoise) * ScallingNoise;
                _vertices[i++] = new Vector3(x, y, z);

                if (y > _maxTerrainHeight)
                    _maxTerrainHeight = y;
                if (y < _minTerrainHeight)
                    _minTerrainHeight = y;
            }
        }

        var vert = 0;
        var tris = 0;
        _triangles = new int[6 * xSize * zSize];

        for (var z = 0; z < zSize; z++)
        {
            for (var x = 0; x < xSize; x++)
            {
                _triangles[tris + 0] = vert + 0;
                _triangles[tris + 1] = vert + xSize + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + xSize + 1;
                _triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        _colors = new Color[_vertices.Length];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (var x = 0; x <= xSize; x++)
            {
                var height = _vertices[i].y / ScallingNoise;
                _colors[i++] = Gradient.Evaluate(height);
            }
        }
    }

    private void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.colors = _colors;
        _mesh.RecalculateNormals();

        _meshFilter.mesh = _mesh;
    }

    #region WATER

    GameObject CreateWaterGameObject()
    {
        var waterGameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        waterGameObject.transform.localRotation = Quaternion.Euler(90f, 0, 0);

        waterGameObject.GetComponent<MeshRenderer>().sharedMaterial = GetCreateMaterialAsset("Assets/Materials/water_material.mat", "Shader Graphs/WaterShader");
        waterGameObject.SetActive(true);

        waterGameObject.transform.parent = gameObject.transform;

        return waterGameObject;
    }

    void UpdateWater(GameObject waterGameObject)
    {
        waterGameObject.transform.localScale = new Vector3(xSize, zSize, 1);
        var gradientWaterKey1 = Gradient.colorKeys[0].time;
        var gradientWaterKey2 = Gradient.colorKeys[1].time;
        var waterBestKey = Mathf.Lerp(gradientWaterKey1, gradientWaterKey2, 0.8f) * ScallingNoise;
        waterGameObject.transform.localPosition = new Vector3(xSize / 2f, waterBestKey, zSize / 2f);
    }


    #endregion

    public static Material GetCreateMaterialAsset(string path = "Assets/Materials/test.mat",
        string shaderName = "Standard")
    {
        if (AssetDatabase.AssetPathExists(path))
            return AssetDatabase.LoadAssetAtPath<Material>(path);

        var guidFolder =AssetDatabase.CreateFolder("Assets", "Materials");
        if(string.IsNullOrEmpty(guidFolder))
            Debug.LogError("Wrong parameters in creating folders");

        Shader shader = Shader.Find(shaderName);
        var mat = new Material(shader);

        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return mat;
    }
}
