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
            Delete();
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
                        n = n.Remove(0, n.IndexOf("Assets")).Replace("\\","/");
                        var pwithe = (file.Remove(0, file.IndexOf("AssetsBundle") + "AssetsBundle".Length + 1));
                        var p = pwithe.Remove(pwithe.LastIndexOf(".")).Replace("\\", "/");
                        uibuild.assetBundleName = p + "_" + dirname + ".asherlinkdata";
                        uibuild.assetNames = new string[] { n };
                        buildMap.Add(uibuild);
                    }
                }
            }
            if (windowsTest)
                BuildPipeline.BuildAssetBundles(cuspath, buildMap.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
            else
                BuildPipeline.BuildAssetBundles(cuspath, buildMap.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);

            BuildTex(buildMap);
        }

        ReadWall();

        ReadLine();

        ReadPreviewTex();
    }

    private async void BuildTex(List<AssetBundleBuild> buildMap)
    {
        List<ZICHAN> zichans = new List<ZICHAN>(buildMap.Count);
        foreach (var item in buildMap)
        {
            ZICHAN zi = new ZICHAN();
            zichans.Add(zi);
            zi.dadapath = item.assetBundleName;
            var n = item.assetNames[0];
            var o = AssetDatabase.LoadAssetAtPath<GameObject>(n);
            if (o)
            {
                var te = AssetPreview.GetAssetPreview(o);
                if (AssetPreview.IsLoadingAssetPreviews())
                {
                    await System.Threading.Tasks.Task.Delay(100);
                }
                n = n.Remove(0, n.IndexOf("AssetsBundle") + "/AssetsBundle".Length);
                var pre = n.Remove(n.LastIndexOf('.')) + ".png";
                zi.previewpath = pre;
                if (te != null)
                {
                    var dir = Path.GetDirectoryName(Application.streamingAssetsPath + "/" + pre);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    System.IO.File.WriteAllBytes(Application.streamingAssetsPath + "/" + pre, te.EncodeToPNG());
                }
            }
        }
        System.IO.File.WriteAllText(Application.streamingAssetsPath + "/3dscene.json", LitJson.JsonMapper.ToJson(zichans));
    }

    public class ZICHAN
    {
        public string dadapath;
        public string previewpath;
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

    void ReadLine()
    {
        node = EditorGUILayout.ObjectField(node, typeof(Transform), true) as Transform;
        if (GUILayout.Button("读取线条"))
        {
            if (!node)
                return;
            foreach (Transform item in node)
            {
                var lr = item.GetComponent<LineRenderer>();
                if (lr)
                {
                    var posy = lr.transform.position.y;
                    string w = string.Empty;
                    for (int i = 0; i < lr.positionCount; i++)
                    {
                        var p = lr.GetPosition(i);
                        w += p.x + "," + p.z + "," + (p.y - posy) + ",";
                    }
                    w = w.Substring(0, w.Length - 1);
                    Color32 c = lr.startColor;
                    Debug.Log(c.r + "," + c.g + "," + c.b + "," + c.a);
                    Debug.Log(w);
                    Debug.Log(lr.sharedMaterial.GetFloat("_Speed"));
                }
            }
        }
    }

    void ReadPreviewTex()
    {
        if (GUILayout.Button("读取预览图片"))
        {
            var os = Directory.GetFiles(System.Environment.CurrentDirectory + "/Assets/AssetsBundle", "*", SearchOption.AllDirectories);
            foreach (var item in os)
            {
                if (item.EndsWith(".prefab"))
                {
                    var path = item.Remove(0, item.IndexOf("Assets/"));
                    var o = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (o)
                    {
                        AssetPreview.SetPreviewTextureCacheSize(512);
                        var te = AssetPreview.GetAssetPreview(o);
                        System.IO.File.WriteAllBytes(Application.streamingAssetsPath + "/test.png", te.EncodeToPNG());
                        break;
                    }
                }
            }
        }
    }
}
