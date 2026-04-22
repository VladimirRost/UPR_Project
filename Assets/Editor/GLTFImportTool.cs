using UnityEngine;
using UnityEditor;
using System.IO;
//using System.Linq;

public class GLTFImportTool : EditorWindow
{
    private string selectedFilePath = "";
    private bool generateLightmap = false;
    private bool optimizeForMobile = true;
    private bool createLODs = false;
    private bool generateColliders = true;
    private Vector2 scrollPosition;

    // Настройки для LOD
    private float lodPercent1 = 0.6f;
    private float lodPercent2 = 0.3f;
    private float lodPercent3 = 0.1f;

    [MenuItem("Tools/glTF Import Tool")]
    public static void ShowWindow()
    {
        GetWindow<GLTFImportTool>("glTF Import Tool");
    }

    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("glTF/GLB Import Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // ===== НАСТРОЙКИ ИМПОРТА (ПЕРЕД ВЫБОРОМ ФАЙЛА) =====
        GUILayout.Label("Import Settings", EditorStyles.boldLabel);
        generateLightmap = EditorGUILayout.Toggle("Generate Lightmap UVs", generateLightmap);
        optimizeForMobile = EditorGUILayout.Toggle("Optimize for Mobile/WebGL", optimizeForMobile);
        createLODs = EditorGUILayout.Toggle("Auto-create LODs on Import", createLODs);
        generateColliders = EditorGUILayout.Toggle("Auto-generate Colliders", generateColliders);

        EditorGUILayout.Space(15);

        // ===== ВЫБОР ФАЙЛА =====
        GUILayout.Label("File Selection", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("File Path:", selectedFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            selectedFilePath = EditorUtility.OpenFilePanel("Select glTF/GLB File", "", "gltf,glb");
        }
        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(selectedFilePath) && File.Exists(selectedFilePath))
        {
            if (GUILayout.Button("Import File", GUILayout.Height(30)))
            {
                ImportGLTFFile();
            }
        }

        EditorGUILayout.Space(20);

        // ===== КОЛЛАЙДЕРЫ =====
        GUILayout.Label("Colliders", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Box Colliders to Selected"))
        {
            AddBoxCollidersToSelected();
        }

        if (GUILayout.Button("Add Mesh Colliders to Selected"))
        {
            AddMeshCollidersToSelected();
        }

        if (GUILayout.Button("Remove Colliders from Selected"))
        {
            RemoveCollidersFromSelected();
        }

        EditorGUILayout.Space(20);

        // ===== МАТЕРИАЛЫ =====
        GUILayout.Label("Materials", EditorStyles.boldLabel);
        if (GUILayout.Button("Convert Selected Materials to URP"))
        {
            ConvertMaterialsToURP();
        }

        EditorGUILayout.Space(20);

        // ===== ПОЛЕЗНЫЕ ИНСТРУМЕНТЫ =====
        GUILayout.Label("Useful Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Optimize Selected Models"))
        {
            OptimizeSelectedModels();
        }

        if (GUILayout.Button("Combine Selected Meshes (Static Batching)"))
        {
            CombineSelectedMeshes();
        }

        if (GUILayout.Button("Generate Lightmap UVs for Selected"))
        {
            GenerateLightmapUVs();
        }

        // НОВЫЙ ПУНКТ: Generate LODs for Selected
        GUILayout.Space(5);
        GUILayout.Label("LOD Settings:", EditorStyles.miniLabel);
        lodPercent1 = EditorGUILayout.Slider("LOD 0 (%):", lodPercent1, 0.01f, 1.0f);
        lodPercent2 = EditorGUILayout.Slider("LOD 1 (%):", lodPercent2, 0.01f, 1.0f);
        lodPercent3 = EditorGUILayout.Slider("LOD 2 (%):", lodPercent3, 0.01f, 1.0f);

        if (GUILayout.Button("Generate LODs for Selected", GUILayout.Height(25)))
        {
            GenerateLODsForSelected();
        }

        EditorGUILayout.Space(20);

        // ===== СТАТИСТИКА =====
        GUILayout.Label("Statistics", EditorStyles.boldLabel);
        if (GUILayout.Button("Show Scene Statistics"))
        {
            ShowSceneStatistics();
        }

        if (GUILayout.Button("Show Selected Statistics"))
        {
            ShowSelectedStatistics();
        }

        EditorGUILayout.EndScrollView();
    }

    void ImportGLTFFile()
    {
        if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a valid file!", "OK");
            return;
        }

        // Копируем файл в Assets
        string fileName = Path.GetFileName(selectedFilePath);
        string destPath = "Assets/ImportedModels/" + fileName;

        if (!Directory.Exists("Assets/ImportedModels"))
            Directory.CreateDirectory("Assets/ImportedModels");

        // Убеждаемся, что файл не используется
        AssetDatabase.Refresh();

        try
        {
            File.Copy(selectedFilePath, destPath, true);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to copy file: {e.Message}", "OK");
            return;
        }

        AssetDatabase.Refresh();

        // Импортируем
        ModelImporter importer = AssetImporter.GetAtPath(destPath) as ModelImporter;
        if (importer != null)
        {
            // Настройки импорта
            importer.isReadable = generateColliders;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.optimizeMeshPolygons = optimizeForMobile;
            importer.optimizeMeshVertices = optimizeForMobile;
            importer.meshCompression = optimizeForMobile ? ModelImporterMeshCompression.Medium : ModelImporterMeshCompression.Off;

            // Генерация Lightmap UV
            if (generateLightmap)
                importer.generateSecondaryUV = true;

            importer.SaveAndReimport();

            // Получаем импортированный объект
            GameObject importedObject = AssetDatabase.LoadAssetAtPath<GameObject>(destPath);
            if (importedObject != null)
            {
                // Инстанцируем в сцену
                GameObject instance = Instantiate(importedObject);
                instance.name = Path.GetFileNameWithoutExtension(fileName);

                // Генерируем коллайдеры
                if (generateColliders)
                {
                    AddCollidersToObject(instance);
                }

                // Создаем LOD
                if (createLODs)
                {
                    CreateLODGroup(instance, new float[] { lodPercent1, lodPercent2, lodPercent3 });
                }

                Selection.activeGameObject = instance;
                EditorUtility.DisplayDialog("Success", $"File imported successfully!\nObject: {instance.name}", "OK");
            }
        }

        AssetDatabase.Refresh();
    }

    void AddCollidersToObject(GameObject obj)
    {
        MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>(true);
        int count = 0;
        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.GetComponent<Collider>() == null && mf.sharedMesh != null)
            {
                MeshCollider collider = mf.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = mf.sharedMesh;
                collider.convex = false;
                count++;
            }
        }
        Debug.Log($"Added {count} mesh colliders to {obj.name}");
    }

    void AddBoxCollidersToSelected()
    {
        int count = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.GetComponent<Collider>() == null)
            {
                BoxCollider collider = obj.AddComponent<BoxCollider>();
                // Автоматически подгоняем размер
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    collider.size = renderer.bounds.size;
                    collider.center = renderer.bounds.center - obj.transform.position;
                }
                count++;
            }
        }
        Debug.Log($"Added {count} box colliders");
        EditorUtility.DisplayDialog("Complete", $"Added {count} box colliders", "OK");
    }

    void AddMeshCollidersToSelected()
    {
        int count = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                if (obj.GetComponent<Collider>() == null)
                {
                    MeshCollider collider = obj.AddComponent<MeshCollider>();
                    collider.sharedMesh = mf.sharedMesh;
                    count++;
                }
            }
        }
        Debug.Log($"Added {count} mesh colliders");
        EditorUtility.DisplayDialog("Complete", $"Added {count} mesh colliders", "OK");
    }

    void RemoveCollidersFromSelected()
    {
        int count = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            Collider[] colliders = obj.GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                DestroyImmediate(collider);
                count++;
            }
        }
        Debug.Log($"Removed {count} colliders");
        EditorUtility.DisplayDialog("Complete", $"Removed {count} colliders", "OK");
    }

    void ConvertMaterialsToURP()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            EditorUtility.DisplayDialog("Error", "URP not found! Make sure URP is installed.", "OK");
            return;
        }

        int count = 0;
        foreach (Object obj in Selection.objects)
        {
            if (obj is Material)
            {
                Material mat = obj as Material;
                ConvertMaterialToURP(mat, urpLit);
                count++;
            }
        }

        Debug.Log($"Converted {count} materials to URP");
        EditorUtility.DisplayDialog("Complete", $"Converted {count} materials to URP", "OK");
    }

    void ConvertMaterialToURP(Material mat, Shader urpShader)
    {
        Material newMat = new Material(urpShader);
        newMat.name = mat.name + "_URP";

        // Копируем основные свойства
        if (mat.HasProperty("_MainTex"))
            newMat.SetTexture("_BaseMap", mat.GetTexture("_MainTex"));
        if (mat.HasProperty("_Color"))
            newMat.SetColor("_BaseColor", mat.GetColor("_Color"));
        if (mat.HasProperty("_BumpMap"))
            newMat.SetTexture("_BumpMap", mat.GetTexture("_BumpMap"));
        if (mat.HasProperty("_MetallicGlossMap"))
            newMat.SetTexture("_MetallicGlossMap", mat.GetTexture("_MetallicGlossMap"));
        if (mat.HasProperty("_Glossiness"))
            newMat.SetFloat("_Smoothness", mat.GetFloat("_Glossiness"));

        newMat.enableInstancing = optimizeForMobile;

        string path = AssetDatabase.GetAssetPath(mat);
        string newPath = path.Replace(".mat", "_URP.mat");
        AssetDatabase.CreateAsset(newMat, newPath);
    }

    void GenerateLightmapUVs()
    {
        int count = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                string path = AssetDatabase.GetAssetPath(mf.sharedMesh);
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null)
                {
                    importer.generateSecondaryUV = true;
                    importer.SaveAndReimport();
                    count++;
                }
            }
        }
        Debug.Log($"Generated lightmap UVs for {count} meshes");
        EditorUtility.DisplayDialog("Complete", $"Generated lightmap UVs for {count} meshes", "OK");
    }

    void OptimizeSelectedModels()
    {
        int count = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                string path = AssetDatabase.GetAssetPath(mf.sharedMesh);
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null)
                {
                    importer.optimizeMeshPolygons = true;
                    importer.optimizeMeshVertices = true;
                    importer.meshCompression = ModelImporterMeshCompression.Medium;
                    importer.SaveAndReimport();
                    count++;
                }
            }
        }
        Debug.Log($"Optimized {count} models");
        EditorUtility.DisplayDialog("Complete", $"Optimized {count} models", "OK");
    }

    void CombineSelectedMeshes()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a parent object with mesh children!", "OK");
            return;
        }

        GameObject combined = new GameObject("CombinedMesh");
        MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No meshes found in selected object hierarchy!", "OK");
            DestroyImmediate(combined);
            return;
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            DestroyImmediate(meshFilters[i].gameObject);
        }

        MeshFilter mf = combined.AddComponent<MeshFilter>();
        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine);
        mf.mesh = newMesh;
        combined.AddComponent<MeshRenderer>();

        combined.transform.position = Selection.activeGameObject.transform.position;

        Debug.Log($"Combined {meshFilters.Length} meshes into one object");
        EditorUtility.DisplayDialog("Complete", $"Combined {meshFilters.Length} meshes into one object", "OK");
    }

    // НОВАЯ ФУНКЦИЯ: Generate LODs for Selected
    void GenerateLODsForSelected()
    {
        int count = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            // Проверяем, есть ли уже LOD Group
            LODGroup existingLOD = obj.GetComponent<LODGroup>();
            if (existingLOD != null)
            {
                if (EditorUtility.DisplayDialog("LOD Exists", $"Object '{obj.name}' already has LOD Group. Override?", "Yes", "No"))
                {
                    DestroyImmediate(existingLOD);
                }
                else
                {
                    continue;
                }
            }

            CreateLODGroup(obj, new float[] { lodPercent1, lodPercent2, lodPercent3 });
            count++;
        }

        Debug.Log($"Generated LODs for {count} objects");
        EditorUtility.DisplayDialog("Complete", $"Generated LODs for {count} objects\n\nLOD Levels:\nLOD0: {lodPercent1 * 100}%\nLOD1: {lodPercent2 * 100}%\nLOD2: {lodPercent3 * 100}%", "OK");
    }

    void CreateLODGroup(GameObject obj, float[] lodPercentages)
    {
        // Получаем все рендереры на объекте и его дочерних объектах
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning($"No renderers found on {obj.name}, cannot create LOD Group");
            return;
        }

        // Добавляем компонент LOD Group
        LODGroup lodGroup = obj.AddComponent<LODGroup>();

        // Создаем уровни LOD
        LOD[] lods = new LOD[lodPercentages.Length];
        for (int i = 0; i < lodPercentages.Length; i++)
        {
            lods[i] = new LOD(lodPercentages[i], renderers);
        }

        // Применяем LOD
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();

        Debug.Log($"Created LOD Group on {obj.name} with {lodPercentages.Length} levels");
    }

    void ShowSceneStatistics()
    {
        int totalVertices = 0;
        int totalTriangles = 0;
        int totalMeshes = 0;
        int totalObjects = 0;

        MeshFilter[] meshFilters = FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh != null)
            {
                totalVertices += mf.sharedMesh.vertexCount;
                totalTriangles += mf.sharedMesh.triangles.Length / 3;
                totalMeshes++;
            }
        }

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        totalObjects = allObjects.Length;

        string stats = $"=== Scene Statistics ===\n\n" +
                      $"Total Objects: {totalObjects}\n" +
                      $"Total Meshes: {totalMeshes}\n" +
                      $"Total Vertices: {totalVertices:N0}\n" +
                      $"Total Triangles: {totalTriangles:N0}\n\n" +
                      $"Average Vertices per Mesh: {(totalMeshes > 0 ? (totalVertices / totalMeshes).ToString("N0") : "0")}\n" +
                      $"Average Triangles per Mesh: {(totalMeshes > 0 ? (totalTriangles / totalMeshes).ToString("N0") : "0")}";

        EditorUtility.DisplayDialog("Scene Statistics", stats, "OK");
        Debug.Log(stats);
    }

    void ShowSelectedStatistics()
    {
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No objects selected!", "OK");
            return;
        }

        int totalVertices = 0;
        int totalTriangles = 0;
        int totalMeshes = 0;
        int totalColliders = 0;
        int totalMaterials = 0;
        int totalLODGroups = 0;

        foreach (GameObject obj in Selection.gameObjects)
        {
            // Считаем меши
            MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter mf in meshFilters)
            {
                if (mf.sharedMesh != null)
                {
                    totalVertices += mf.sharedMesh.vertexCount;
                    totalTriangles += mf.sharedMesh.triangles.Length / 3;
                    totalMeshes++;
                }
            }

            // Считаем коллайдеры
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            totalColliders += colliders.Length;

            // Считаем материалы
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                totalMaterials += renderer.sharedMaterials.Length;
            }

            // Считаем LOD группы
            LODGroup[] lodGroups = obj.GetComponentsInChildren<LODGroup>(true);
            totalLODGroups += lodGroups.Length;
        }

        string stats = $"=== Selected Statistics ===\n\n" +
                      $"Selected Objects: {Selection.gameObjects.Length}\n" +
                      $"Total Meshes: {totalMeshes}\n" +
                      $"Total Vertices: {totalVertices:N0}\n" +
                      $"Total Triangles: {totalTriangles:N0}\n" +
                      $"Total Colliders: {totalColliders}\n" +
                      $"Total Materials: {totalMaterials}\n" +
                      $"Total LOD Groups: {totalLODGroups}\n\n" +
                      $"Average Vertices per Mesh: {(totalMeshes > 0 ? (totalVertices / totalMeshes).ToString("N0") : "0")}\n" +
                      $"Average Triangles per Mesh: {(totalMeshes > 0 ? (totalTriangles / totalMeshes).ToString("N0") : "0")}";

        // Добавляем информацию о каждом выбранном объекте
        stats += "\n\n=== Selected Objects ===\n";
        foreach (GameObject obj in Selection.gameObjects)
        {
            int objVertices = 0;
            int objTriangles = 0;
            MeshFilter[] mfs = obj.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter mf in mfs)
            {
                if (mf.sharedMesh != null)
                {
                    objVertices += mf.sharedMesh.vertexCount;
                    objTriangles += mf.sharedMesh.triangles.Length / 3;
                }
            }
            stats += $"\n• {obj.name}\n  Vertices: {objVertices:N0}, Triangles: {objTriangles:N0}";
        }

        EditorUtility.DisplayDialog("Selected Statistics", stats, "OK");
        Debug.Log(stats);
    }
}