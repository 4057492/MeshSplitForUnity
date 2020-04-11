using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshSpliterWindowEditor : EditorWindow
{
    public const string MeshAssetParentPath = "Assets/SPLIT_MESHS/";
    private float gridSizeX = 1f;
    private float gridSizeY = 1f;
    private float gridSizeZ = 1f;
    
    private bool axisX = true;
    private bool axisY = true;
    private bool axisZ = true;

    private int renderLayerIndex = 0;
    private string renderLayerName = "Default";

    private bool useSortingLayerFromThisMesh = true;
    private bool useStaticSettingsFromThisMesh = true;
    // private bool myOverrideSaveAssetPath = false;
    // private string mySaveAssetsPath = string.Empty;
    private bool saveMeshToPath = false;
    private GameObject currentGameObject = null;
    
    private static readonly GUIContent overrideSaveAssetsPathContent = new GUIContent("Override Save Assets Path", "If you want to override the path where the generated assets are saved.");
    private static readonly GUIContent saveAssetsPathContent = new GUIContent("Save Assets Path", "The path within the project to save the generated assets. Leave this empty to use the default path.");
    

    [MenuItem("Examples/Mesh Spliter")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MeshSpliterWindowEditor window = (MeshSpliterWindowEditor)EditorWindow.GetWindow(typeof(MeshSpliterWindowEditor));
        window.Show();
    }
    void OnInspectorUpdate()
    {
        Repaint();
    }
    void OnGUI()
    {
        DrawProperty();
        if(Selection.gameObjects.Length > 0)
        {
            if ((Selection.gameObjects[0].GetComponent<MeshFilter>() != null) && (Selection.gameObjects[0].GetComponent<Renderer>() != null))
            {
                EditorGUILayout.HelpBox("Bounds : " + Selection.gameObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.ToString(), MessageType.Info, true);
                EditorGUILayout.HelpBox("Vertices : " + Selection.gameObjects[0].GetComponent<MeshFilter>().sharedMesh.vertices.Length.ToString(), MessageType.Info, true);
                EditorGUILayout.HelpBox("Max vertices count per Mesh : 65535 ", MessageType.Warning, true);
                if(GUILayout.Button("Split Mesh"))
                {
                    currentGameObject = Selection.gameObjects[0];
                    SplitMesh();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("This GameObject has no MeshFilter or Renderer.", MessageType.Error, true);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please select the GameObject to operate.", MessageType.Info, true);
        }
    }

    void DrawProperty()
    {
        axisX = EditorGUILayout.Toggle("Axis X", axisX);
        gridSizeX = EditorGUILayout.Slider("Grid Size", gridSizeX, 0, 100f);
        axisY = EditorGUILayout.Toggle("Axis Y", axisY);
        gridSizeY = EditorGUILayout.Slider("Grid Size", gridSizeY, 0, 100f);
        axisZ = EditorGUILayout.Toggle("Axis Z", axisZ);
        gridSizeZ = EditorGUILayout.Slider("Grid Size", gridSizeZ, 0, 100f);
        renderLayerIndex = EditorGUILayout.IntField("Render Layer Index", renderLayerIndex);
        renderLayerName = EditorGUILayout.TextField("Render Layer Name", renderLayerName);
        useSortingLayerFromThisMesh = EditorGUILayout.Toggle("Use Sorting Layer From This Mesh", useSortingLayerFromThisMesh);
        useStaticSettingsFromThisMesh = EditorGUILayout.Toggle("Use Static Settings From This Mesh", useStaticSettingsFromThisMesh);
        // myOverrideSaveAssetPath = EditorGUILayout.Toggle(overrideSaveAssetsPathContent, myOverrideSaveAssetPath);
        // if(myOverrideSaveAssetPath)
        //     mySaveAssetsPath = EditorGUILayout.TextField(saveAssetsPathContent, mySaveAssetsPath);
        // else
        //     mySaveAssetsPath = string.Empty;
        saveMeshToPath = EditorGUILayout.Toggle("Save Mesh to Path", saveMeshToPath);
    }
    Vector3[] baseVerticles;
    int[] baseTriangles;
    Vector2[] baseUVs;
    Vector3[] baseNormals;
    Vector3 GetGirdKey(Vector3 pointPosition)
    {
        Vector3 gridKey = Vector3.zero;
        if(axisX)
            gridKey.x = Mathf.Round(pointPosition.x / gridSizeX);
        if(axisY)
            gridKey.y = Mathf.Round(pointPosition.y / gridSizeY);
        if(axisZ)
            gridKey.z = Mathf.Round(pointPosition.z / gridSizeZ);
        return gridKey;
    }
    void SplitMesh()
    {
        currentGameObject.GetComponent<Renderer>().enabled = false;

        Mesh baseMesh = Selection.gameObjects[0].GetComponent<MeshFilter>().sharedMesh;
        baseVerticles = baseMesh.vertices;
        baseTriangles = baseMesh.triangles;
        baseUVs = baseMesh.uv;
        baseNormals = baseMesh.normals;

        Dictionary<Vector3, List<int>> triDictionary = new Dictionary<Vector3, List<int>>();

        Vector3 triCenter;
        Vector3 triKey;
        for (int i = 0; i < baseTriangles.Length; i += 3)
        {
            triCenter = 
                (baseVerticles[baseTriangles[i]] +
                 baseVerticles[baseTriangles[i + 1]] +
                 baseVerticles[baseTriangles[i + 2]]) / 3;
            triKey = GetGirdKey(triCenter);
            if(!triDictionary.ContainsKey(triKey))
            {
                triDictionary.Add(triKey, new List<int>());
            }
            triDictionary[triKey].Add(baseTriangles[i]);
            triDictionary[triKey].Add(baseTriangles[i + 1]);
            triDictionary[triKey].Add(baseTriangles[i + 2]);
            EditorUtility.DisplayProgressBar("Mapping triangles to grids", i + "/" + baseTriangles.Length, (float)i / baseTriangles.Length);
        }
        EditorUtility.ClearProgressBar();
        foreach(var item in triDictionary.Keys)
        {
            CreatMesh(item, triDictionary[item]);
        }
        if (saveMeshToPath)
        {
            Mesh m;
            for (int i = 0; i < currentGameObject.transform.childCount; i++)
            {
                m = currentGameObject.transform.GetChild(i).gameObject.GetComponent<MeshFilter>().sharedMesh;
                SaveMeshAsset(m, currentGameObject.name, currentGameObject.GetInstanceID(), i);
                EditorUtility.DisplayProgressBar("Saving meshs to " + MeshAssetParentPath, i + "/" + currentGameObject.transform.childCount, (float)i / currentGameObject.transform.childCount);
            }
            EditorUtility.ClearProgressBar();
        }
    }

    void CreatMesh(Vector3 triKey, List<int> dictionaryTriangles)
    {
        GameObject newObject = new GameObject();
        newObject.name = "SubMesh" + triKey.ToString();
        newObject.transform.SetParent(currentGameObject.transform);
        newObject.transform.localPosition = Vector3.zero;
        newObject.transform.localScale = Vector3.one;
        newObject.transform.localRotation = currentGameObject.transform.localRotation;
        newObject.AddComponent<MeshFilter>();
        newObject.AddComponent<MeshRenderer>();

        MeshRenderer newRenderer = newObject.GetComponent<MeshRenderer>();
        newRenderer.sharedMaterial = currentGameObject.GetComponent<MeshRenderer>().sharedMaterial;

        Renderer baseRenderer = currentGameObject.GetComponent<Renderer>();
        if (!useSortingLayerFromThisMesh)
        {
            newRenderer.sortingLayerName = renderLayerName;
            newRenderer.sortingOrder = renderLayerIndex;
        }
        else if (baseRenderer)
        {
            newRenderer.sortingLayerName = baseRenderer.sortingLayerName;
            newRenderer.sortingOrder = baseRenderer.sortingOrder;
        }

        if (useStaticSettingsFromThisMesh)
            newObject.isStatic = currentGameObject.isStatic;

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        for (int i = 0; i < dictionaryTriangles.Count; i += 3)
        {

            verts.Add(baseVerticles[dictionaryTriangles[i]]);
            verts.Add(baseVerticles[dictionaryTriangles[i + 1]]);
            verts.Add(baseVerticles[dictionaryTriangles[i + 2]]);

            tris.Add(i);
            tris.Add(i + 1);
            tris.Add(i + 2);

            if (baseUVs.Length > 0)
            {
                uvs.Add(baseUVs[dictionaryTriangles[i]]);
                uvs.Add(baseUVs[dictionaryTriangles[i + 1]]);
                uvs.Add(baseUVs[dictionaryTriangles[i + 2]]);
            }
            if (baseNormals.Length > 0)
            {
                normals.Add(baseNormals[dictionaryTriangles[i]]);
                normals.Add(baseNormals[dictionaryTriangles[i + 1]]);
                normals.Add(baseNormals[dictionaryTriangles[i + 2]]);
            }
            EditorUtility.DisplayProgressBar("Adding triangles to meshs", i + "/" + dictionaryTriangles.Count, (float)i / dictionaryTriangles.Count);
        }
        EditorUtility.ClearProgressBar();
        Mesh m = new Mesh();
        m.name = triKey.ToString();
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();
        m.uv = uvs.ToArray();
        m.normals = normals.ToArray();

        UnityEditor.MeshUtility.Optimize(m);
        MeshFilter newMeshFilter = newObject.GetComponent<MeshFilter>();
        newMeshFilter.mesh = m;
    }


    static void SaveMeshAsset(Object meshAsset, string parentGameObjectName, int gameObjectID, int meshIndex)
    {
        parentGameObjectName = MakePathSafe(parentGameObjectName);
        string gID = parentGameObjectName + MakePathSafe(gameObjectID.ToString());
        string path = string.Format("{0}{1}/{2}.mesh", MeshAssetParentPath, gID, meshIndex.ToString());
        SaveAsset(meshAsset, path);
    }
    private static void SaveAsset(Object asset, string path)
        {
#if UNITY_EDITOR
            CreateParentDirectory(path);

            // Make sure that there is no asset with the same path already
            path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);

            UnityEditor.AssetDatabase.CreateAsset(asset, path);
#endif
        }

        private static void CreateParentDirectory(string path)
        {
#if UNITY_EDITOR
            int lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex != -1)
            {
                string parentPath = path.Substring(0, lastSlashIndex);
                if (!UnityEditor.AssetDatabase.IsValidFolder(parentPath))
                {
                    lastSlashIndex = parentPath.LastIndexOf('/');
                    if (lastSlashIndex != -1)
                    {
                        string folderName = parentPath.Substring(lastSlashIndex + 1);
                        string folderParentPath = parentPath.Substring(0, lastSlashIndex);
                        CreateParentDirectory(parentPath);
                        UnityEditor.AssetDatabase.CreateFolder(folderParentPath, folderName);
                    }
                    else
                    {
                        UnityEditor.AssetDatabase.CreateFolder(string.Empty, parentPath);
                    }
                }
            }
#endif
        }

        private static string MakePathSafe(string name)
        {
            var sb = new System.Text.StringBuilder(name.Length);
            bool lastWasSeparator = false;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    lastWasSeparator = false;
                    sb.Append(c);
                }
                else if (c == '_' || c == '-')
                {
                    if (!lastWasSeparator)
                    {
                        lastWasSeparator = true;
                        sb.Append(c);
                    }
                }
                else
                {
                    if (!lastWasSeparator)
                    {
                        lastWasSeparator = true;
                        sb.Append('_');
                    }
                }
            }
            return sb.ToString();
        }

        private static string ValidateSaveAssetsPath(string saveAssetsPath)
        {
            if (string.IsNullOrEmpty(saveAssetsPath))
                return null;

            saveAssetsPath = saveAssetsPath.Replace('\\', '/');
            saveAssetsPath = saveAssetsPath.Trim('/');

            if (System.IO.Path.IsPathRooted(saveAssetsPath))
                throw new System.InvalidOperationException("The save assets path cannot be rooted.");
            else if (saveAssetsPath.Length > 100)
                throw new System.InvalidOperationException("The save assets path cannot be more than 100 characters long to avoid I/O issues.");

            // Make the path safe
            var pathParts = saveAssetsPath.Split('/');
            for (int i = 0; i < pathParts.Length; i++)
            {
                pathParts[i] = MakePathSafe(pathParts[i]);
            }
            saveAssetsPath = string.Join("/", pathParts);

            return saveAssetsPath;
        }
}
