using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChildMeshGen))]
public class ButtonChildMeshGen : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (ChildMeshGen)target;
        script.Update();
    }
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChildMeshGen : MonoBehaviour
{
    public Vector3[] Vertices { get; private set; }
    private int[] _triangles;
    private Color[] _colors;

    public int XSize = 20;
    public int ZSize = 20;

    public float ScalingNoiseParam = .3f;
    public float ScalingNoise = 2f;
    public float OffsetNoiseX = 0;
    public float OffsetNoiseZ = 0;

    private float _mOldOffSetX;
    private float _mOldOffSetZ;
    private float _mScalingNoiseParam;
    private float _mScalingNoise;

    public Gradient Gradient;
    public GameObject Tree = null;
    public int TreeMinIndex;
    public int TreeMaxIndex;
    public float TreeDensity = 0.3f;
    public float TreeNoiseScale;
    public float TreeMinScale = 0.8f;
    public float TreeMaxScale = 1.2f;

    private readonly List<GameObject> _tree = new();
    private int _treeMinIndex;
    private int _treeMaxIndex;
    private float _treeDensity;
    private float _treeNoiseScale;
    private float _treeMinScale;
    private float _treeMaxScale;

    public GameObject Grass = null;
    public float GrassDensity = 0.5f;
    public int GrassBushes = 20; // <=30

    private float _grassDensity;
    private int _grassBushes;
    private readonly List<GameObject> _grass = new();

    private GameObject _waterObject = null;

    // Update is called once per frame
    public void Update()
    {
        if (Math.Abs(_mOldOffSetX - OffsetNoiseX) > 0.01f ||
            Math.Abs(_mOldOffSetZ - OffsetNoiseZ) > 0.01f ||
            Math.Abs(_mScalingNoiseParam - ScalingNoiseParam) > 0.01f ||
            Math.Abs(_mScalingNoise - ScalingNoise) > 0.01f)
        {
            CreateShape(gameObject);
            UpdateMesh(gameObject);
        }

        if (Math.Abs(_treeMinIndex - TreeMinIndex) > 0.01f ||
            Math.Abs(_treeMaxIndex - TreeMaxIndex) > 0.01f ||
            Math.Abs(_treeDensity - TreeDensity) > 0.01f ||
            Math.Abs(_treeNoiseScale - TreeNoiseScale) > 0.01f ||
            Math.Abs(_treeMinScale - TreeMinScale) > 0.01f ||
            Math.Abs(_treeMaxScale - TreeMaxScale) > 0.01f)
        {
            AddTrees();
        }

        if (Math.Abs(_grassDensity - GrassDensity) > 0.01f ||
            Math.Abs(_grassBushes - GrassBushes) > 0.01f)
        {
            AddGrass();
        }
    }
    
    public void UpdateMesh(GameObject terGameObject)
    {
        var mesh = terGameObject.GetComponent<MeshFilter>().sharedMesh; // It complains in Edit mode if you use mesh

        mesh.Clear();
        mesh.vertices = Vertices;
        mesh.triangles = _triangles;
        mesh.colors = _colors;
        mesh.RecalculateNormals();

        terGameObject.GetComponent<MeshFilter>().mesh = mesh;
        terGameObject.GetComponent<MeshCollider>().sharedMesh = mesh;

        AddTrees();
        AddGrass();

        if (_waterObject == null)
        {
            _waterObject = CreateWaterGameObject(gameObject.transform);
        }
        else
        {
            UpdateWater(_waterObject);
        }
    }

    public void CreateShape(GameObject terGameObject)
    {
        _mOldOffSetX = OffsetNoiseX;
        _mOldOffSetZ = OffsetNoiseZ;
        _mScalingNoiseParam = ScalingNoiseParam;
        _mScalingNoise = ScalingNoise;

        Vertices = new Vector3[(XSize + 1) * (ZSize + 1)];
        for (int i = 0, z = 0; z <= ZSize; z++)
        {
            for (var x = 0; x <= XSize; x++)
            {
                var xNoise = (float)x / XSize * ScalingNoiseParam + OffsetNoiseX;
                var zNoise = (float)z / XSize * ScalingNoiseParam + OffsetNoiseZ;

                var y = Mathf.PerlinNoise(xNoise, zNoise) * ScalingNoise;
                Vertices[i++] = new Vector3(x, y, z);
            }
        }

        var vert = 0;
        var tris = 0;
        _triangles = new int[6 * XSize * ZSize];

        for (var z = 0; z < ZSize; z++)
        {
            for (var x = 0; x < XSize; x++)
            {
                _triangles[tris + 0] = vert + 0;
                _triangles[tris + 1] = vert + XSize + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + XSize + 1;
                _triangles[tris + 5] = vert + XSize + 2;

                vert++;
                tris += 6;
            }

            vert++;
        }

        _colors = new Color[Vertices.Length];
        for (int i = 0, z = 0; z <= ZSize; z++)
        {
            for (var x = 0; x <= XSize; x++)
            {
                var height = Vertices[i].y / ScalingNoise;
                _colors[i++] = Gradient.Evaluate(height);
            }
        }
    }

    private void AddVegetation(GameObject gmObj,
        Action updatePrivates,
        List<GameObject> list,
        int minDistance = 0,
        float density = 0,
        int nrBushes = 0)
    {

        if (gmObj == null)
            return;

        updatePrivates();

        list.ForEach(Destroy);
        list.Clear();

        var minHeightTree = Gradient.colorKeys[TreeMinIndex].time * ScalingNoise;
        var maxHeightTree = Gradient.colorKeys[TreeMaxIndex].time * ScalingNoise;

        for (int i = 0, z = 0; z <= ZSize; z++)
        {
            for (var x = 0; x <= XSize; x++)
            {
                // Tree/Grass/Vegetation valid position/area on terrain
                var vert = Vertices[i++];
                if (vert.y < minHeightTree || vert.y > maxHeightTree)
                    continue;

                // Tree/Grass/Vegetation Density
                (var xoffset, float yOffSet) = (UnityEngine.Random.Range(-10000f, 10000f),
                    UnityEngine.Random.Range(-10000f, 10000f));
                var noiseTree = Mathf.PerlinNoise((float)x / XSize * TreeNoiseScale + xoffset,
                    (float)z / ZSize * TreeNoiseScale + yOffSet);

                if (noiseTree > UnityEngine.Random.Range(0, density))
                    continue;

                if (minDistance != 0 && list.Any(tr =>
                        Math.Abs(tr.transform.localPosition.x - vert.x) < minDistance ||
                        Math.Abs(tr.transform.localPosition.z - vert.z) < minDistance))
                    continue;

                var tree = Instantiate(gmObj, transform);
                //tree.transform.localPosition = vert;  // replaced by SetLocalPositionAndRotation
                //tree.transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);    // replaced by SetLocalPositionAndRotation
                tree.transform.SetLocalPositionAndRotation(vert, Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0));
                tree.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.8f, 1.2f);

                var bushes = new List<GameObject> { tree };

                for (int j = 0; j < nrBushes; j++)
                {
                    var newPos = vert + new Vector3(UnityEngine.Random.Range(-3f, 3f), 0,
                        UnityEngine.Random.Range(-3f, 3f));

                    if(XSize - newPos.x < 0.1 ||newPos.x < 0 ||
                       ZSize - newPos.z < 0.1 || newPos.z < 0)
                        continue;

                    if (bushes.Any(b =>
                            b.transform.localPosition.x.Equals(newPos.x) &&
                            b.transform.localPosition.z.Equals(newPos.z)))
                        continue;

                    var bush = Instantiate(gmObj, transform);
                    bush.transform.localPosition = newPos;
                    bush.transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
                    bush.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.8f, 1.2f);

                    bushes.Add(bush);
                }

                list.AddRange(bushes);
            }
        }
    }

    private void AddTrees() => AddVegetation(gmObj: Tree,
            updatePrivates: () =>
            {
                _treeMinIndex = TreeMinIndex;
                _treeMaxIndex = TreeMaxIndex;
                _treeDensity = TreeDensity;
                _treeNoiseScale = TreeNoiseScale;
                _treeMinScale = TreeMinScale;
                _treeMaxScale = TreeMaxScale;
            },
            list: _tree,
            minDistance: 2,
            density: TreeDensity);
    

    private void AddGrass() => AddVegetation(gmObj: Grass,
            updatePrivates: () =>
            {
                _grassDensity = GrassDensity;
                _grassBushes = GrassBushes;
            },
            list: _grass,
            minDistance: 0,
            density: GrassDensity,
            nrBushes: GrassBushes);
    
    #region WATER

    private GameObject CreateWaterGameObject(Transform parent)
    {
        var waterGameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        waterGameObject.name = "Water";
        waterGameObject.transform.localRotation = Quaternion.Euler(90f, 0, 0);

        waterGameObject.GetComponent<MeshRenderer>().sharedMaterial = MeshGenerator.GetCreateMaterialAsset("Assets/Materials/water_material.mat", "Shader Graphs/WaterShader");
        waterGameObject.SetActive(true);

        waterGameObject.transform.parent = parent;

        UpdateWater(waterGameObject);

        return waterGameObject;
    }

    private void UpdateWater(GameObject waterGameObject)
    {
        waterGameObject.transform.localScale = new Vector3(XSize, ZSize, 1);
        var gradientWaterKey1 = Gradient.colorKeys[0].time;
        var gradientWaterKey2 = Gradient.colorKeys[1].time;
        var waterBestKey = Mathf.Lerp(gradientWaterKey1, gradientWaterKey2, 0.75f) * ScalingNoise; // 0.8f
        waterGameObject.transform.localPosition = new Vector3(XSize / 2f, waterBestKey, ZSize / 2f);
    }

    #endregion


}