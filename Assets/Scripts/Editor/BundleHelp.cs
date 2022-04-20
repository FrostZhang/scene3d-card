using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BundleHelp : EditorWindow
{
    private string cuspath = Application.streamingAssetsPath;

    [MenuItem("Tool/BundleHelp")]
    static void Creat()
    {
        GetWindow<BundleHelp>();
    }

    bool windowsTest;
    public void OnGUI()
    {
        windowsTest = EditorGUILayout.Toggle("windows下测试", windowsTest);
        if (GUILayout.Button("建立WebGL"))
        {
            var path = System.Environment.CurrentDirectory + "/Assets/AssetsBundle";
            var ds = Directory.GetDirectories(path);
            List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>(500);
            foreach (var item in ds)
            {
                var dirname = new DirectoryInfo(item).Name;
                var files = Directory.GetFiles(item, "*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (Path.GetExtension(file) != ".meta")
                    {
                        var uibuild = new AssetBundleBuild();
                        string n = file;
                        n = n.Remove(0, n.IndexOf("Assets"));
                        var pwithe = (file.Remove(0, file.IndexOf("AssetsBundle") + "AssetsBundle".Length + 1));
                        var p = pwithe.Remove(pwithe.LastIndexOf("."));
                        uibuild.assetBundleName = p + "_" + dirname;
                        uibuild.assetNames = new string[] { n };
                        buildMap.Add(uibuild);
                    }
                }
            }
            Delete();
            if (windowsTest)
                BuildPipeline.BuildAssetBundles(cuspath, buildMap.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
            else
                BuildPipeline.BuildAssetBundles(cuspath, buildMap.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);
        }

        ReadWall();
    }

    private void Delete()
    {
        var ds = Directory.GetDirectories(cuspath);
        foreach (var item in ds)
        {
            Directory.Delete(item, true);
        }
    }

    Transform node;
    void ReadWall()
    {
        node = EditorGUILayout.ObjectField(node, typeof(Transform), true) as Transform;
        if (GUILayout.Button("读取墙壁"))
        {
            if (!node)
                return;
            string w = string.Empty;
            foreach (Transform item in node)
            {
                w = item.position.x + "," + item.position.z + "," + item.localScale.x + "," + item.localScale.y + "," + item.localScale.z + "," + item.eulerAngles.y;
                Debug.Log(w);
            }
        }
    }
}
