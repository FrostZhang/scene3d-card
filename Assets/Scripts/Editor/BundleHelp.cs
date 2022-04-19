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
        windowsTest = EditorGUILayout.Toggle("windwosœ¬≤‚ ‘", windowsTest);
        if (GUILayout.Button("Ω®¡¢WebGL"))
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
            if (windowsTest)
                BuildPipeline.BuildAssetBundles(cuspath, buildMap.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
            else
                BuildPipeline.BuildAssetBundles(cuspath, buildMap.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);
        }
    }

    private void Delete()
    {
        var ds = Directory.GetDirectories(cuspath);
        foreach (var item in ds)
        {
            Directory.Delete(item, true);
        }
    }
}
