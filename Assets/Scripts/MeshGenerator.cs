using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(MeshGenerator))]
public class ButtonMeshGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (MeshGenerator)target;
        if (GUILayout.Button("Build stuff"))
        {
            script.Start();
        }

        if (GUILayout.Button("Delete stuff"))
        {
            script.CleanUp();
        }

        if (script.ShouldUpdate)
        {
            script.Update();
        }
    }
}

public class MeshGenerator : MonoBehaviour
{
    #region Components

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    #endregion

    public int XSize = 100;
    public int ZSize = 100;

    public float ScalingNoiseParam = 2f; //.3f;
    public float ScalingNoise = 20f;
    public float OffsetNoiseX = 0;
    public float OffsetNoiseZ = 0;

    public Gradient Gradient = new()
    {
        colorKeys = new[]
        {
            new GradientColorKey(new Color(0.042f, 0.132f, 0.991f, 1f), 0.1f),
            new GradientColorKey(new Color(0.03f, 0.425f, 0.139f, 1f), 0.29f),
            new GradientColorKey(new Color(0, 0, 0, 1f), 0.63f),
            new GradientColorKey(new Color(1, 1, 1, 1f), 1f),
        }
    };

    public GameObject Player = null;

    public GameObject Tree = null;
    public int TreeMinIndex=0;
    public int TreeMaxIndex=0;

    public GameObject Grass = null;

    private GameObject _terrainGameObject;
    private readonly Dictionary<string,GameObject> _terrains = new();

    public bool ShouldUpdate = false;
    public bool ShouldTree = true;
    public bool ShouldGrass = true;
    public bool ShouldWater = true;
    public bool ShouldHideOldTerrain = false;
    public void CleanUp()
    {
        _terrains.Values.ToList().ForEach(DestroyImmediate); // It complains in Edit mode if you use Destroy
        _terrains.Clear();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        CleanUp();

        if (TreeMinIndex == 0 && TreeMaxIndex == 0)
        {
            TreeMinIndex = 1;
            TreeMaxIndex = Gradient.colorKeys.Length -2;
        }

        Debug.Log(Gradient.colorKeys.Length);
        Gradient.colorKeys.ToList().ForEach(x => Debug.Log(x.time));
        Gradient.colorKeys.ToList().ForEach(x => Debug.Log(x.color));

        _terrainGameObject = CreateTerrain(gameObject, Vector3.zero);

        if (Player != null)
            Player.transform.localPosition = new Vector3(0, 1, 0) +
                                             _terrainGameObject.GetComponent<ChildMeshGen>()
                                                 .Vertices[XSize / 2 * XSize];
    }

    private GameObject CreateTerrain(GameObject parent, Vector3 offset)
    {
        string gmObjName = $"{offset.x} {offset.y} {offset.z}";
        if (_terrains.TryGetValue(gmObjName, out var terrain))
            return terrain;

        var terrGmObject = new GameObject
        {
            name = gmObjName,
            transform =
            {
                parent = parent.transform,
                localPosition = offset
            }
        };

        var meshFilter = terrGmObject.AddComponent<MeshFilter>();
        var meshRenderer = terrGmObject.AddComponent<MeshRenderer>();
        var meshCollider = terrGmObject.AddComponent<MeshCollider>();

        var mesh = new Mesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        var materialProcedural = GetCreateMaterialAsset("Assets/Materials/procedural_terrain_material.mat",
            "Shader Graphs/procedural_terrain_texture");
        meshRenderer.sharedMaterial = materialProcedural;

        var childMeshGen = terrGmObject.AddComponent<ChildMeshGen>();
        childMeshGen.Gradient = Gradient;
        childMeshGen.XSize = XSize;
        childMeshGen.ZSize = ZSize;
        childMeshGen.ScalingNoiseParam = ScalingNoiseParam;
        childMeshGen.ScalingNoise = ScalingNoise;
        childMeshGen.OffsetNoiseX =
            OffsetNoiseX + (int)(terrGmObject.transform.localPosition.x / XSize * ScalingNoiseParam);
        childMeshGen.OffsetNoiseZ =
            OffsetNoiseZ + (int)(terrGmObject.transform.localPosition.z / ZSize * ScalingNoiseParam);

        childMeshGen.Tree = Tree;
        childMeshGen.TreeMinIndex = TreeMinIndex;
        childMeshGen.TreeMaxIndex = TreeMaxIndex;

        childMeshGen.Grass = Grass;

        childMeshGen.ShouldGrass = ShouldGrass;
        childMeshGen.ShouldTree = ShouldTree;
        childMeshGen.ShouldWater = ShouldWater;

        childMeshGen.CreateShape(terrGmObject);
        childMeshGen.UpdateMesh(terrGmObject);

        _terrains.Add(gmObjName, terrGmObject);

        return terrGmObject;
    }

    // Update is called once per frame
    public void Update()
    {
        if (!ShouldUpdate)
            return;

        if (Player == null)
            return;

        if (!(Player.transform.localPosition.x <= _terrainGameObject.transform.localPosition.x) &&
            !(Player.transform.localPosition.x >= (_terrainGameObject.transform.localPosition.x + XSize)) &&
            !(Player.transform.localPosition.z <= _terrainGameObject.transform.localPosition.z) &&
            !(Player.transform.localPosition.z >= _terrainGameObject.transform.localPosition.z + ZSize))
            return;

        var x = (int)(Player.transform.localPosition.x);
        var z = (int)(Player.transform.localPosition.z);
        if (x < 0) x -= XSize;
        if (z < 0) z -= XSize;

        x = (int)(x / (XSize));
        z = (int)(z / (XSize));

        if (ShouldHideOldTerrain)
            _terrainGameObject.SetActive(false);
        _terrainGameObject = CreateTerrain(gameObject, new Vector3(x * XSize, 0, z * XSize));
        _terrainGameObject.SetActive(true);
    }

    #region Util functions

    private static readonly Dictionary<string, Material> Materials = new();

    public static Material GetCreateMaterialAsset(string path, string shaderName)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(shaderName))
            return null;

        if (Materials.TryGetValue(path, out var asset))
            return asset;

        if (AssetDatabase.AssetPathExists(path))
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Materials.Add(path, material);
            return material;
        }

        var folderPath = string.Join('/', path.Replace('\\', '/').Split('/').SkipLast(1));
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            if (folderPath.StartsWith("Assets/"))
                folderPath = folderPath["Assets/".Length..];

            var guidFolder = AssetDatabase.CreateFolder("Assets", folderPath);
            if (string.IsNullOrEmpty(guidFolder))
                Debug.LogError("Wrong parameters in creating folders");
        }

        var shader = Shader.Find(shaderName);
        var mat = new Material(shader);

        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Materials.Add(path, mat);

        return mat;
    }

    #endregion
}
