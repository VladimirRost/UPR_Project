using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SmartExporter
{
    [MenuItem("Tools/Smart Export Selected")]
    static void Export()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("Ничего не выбрано");
            return;
        }

        string exportRoot = "Assets/SmartExport";
        string prefabFolder = exportRoot + "/Prefabs";

        Directory.CreateDirectory(exportRoot);
        Directory.CreateDirectory(prefabFolder);

        HashSet<string> dependencies = new HashSet<string>();

        foreach (GameObject obj in Selection.gameObjects)
        {
            // Сохраняем временный prefab
            string tempPath = "Assets/__temp.prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, tempPath);

            string[] deps = AssetDatabase.GetDependencies(tempPath, true);

            foreach (string dep in deps)
            {
                if (dep.StartsWith("Assets"))
                    dependencies.Add(dep);
            }

            AssetDatabase.DeleteAsset(tempPath);
        }

        // Копируем зависимости
        foreach (string path in dependencies)
        {
            string targetPath = Path.Combine(exportRoot, path.Replace("Assets/", ""));
            string dir = Path.GetDirectoryName(targetPath);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(path))
                File.Copy(path, targetPath, true);
        }

        AssetDatabase.Refresh();

        // Создаём prefab'ы
        foreach (GameObject obj in Selection.gameObjects)
        {
            string prefabPath = prefabFolder + "/" + obj.name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
        }

        AssetDatabase.Refresh();

        Debug.Log("Экспорт завершён → " + exportRoot);
    }
}