//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using OfficeOpenXml;
//using System.Threading.Tasks;
//using System;
//// Copyright (C) 2019 All Rights Reserved.
//// Detail：AssetBundle	MyChessboard	2019/8/26
//// Version：1.0.0
//public class HueyAssetBundle
//{

//    string cuspath/* = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)*/;
//    bool canpreview;
//    const string ASSETBUNDLEPATH = "Assets/Prefabs/AssetBundle";
//    const string CONFIGPATH = "Assets/Prefabs/AssetBundleConfig";
//    string buildpath = string.Empty;
//    public void OnGUI()
//    {
//        EditorGUILayout.LabelField("1 添加目录 Assets/Prefabs/AssetBundle", new GUIStyle("WhiteLabel"));
//        EditorGUILayout.LabelField("2 添加目录 Assets/Prefabs/AssetBundleConfig", new GUIStyle("WhiteLabel"));
//        EditorGUILayout.LabelField("3 AssetBundle 添加音频,图片,Unity prefab等资源", new GUIStyle("WhiteLabel"));
//        EditorGUILayout.LabelField("4 AssetBundleConfig 添加Excel sheet1配置语言文件 sheet2配置config", new GUIStyle("WhiteLabel"));
//        EditorGUILayout.LabelField("5 生成路径 测试路径通常为  Application.streamingAssetsPath", new GUIStyle("WhiteLabel"));
//        EditorGUILayout.LabelField("6 surface下不建立文件夹 按单文件打包!!!", new GUIStyle("WhiteLabel"));
//        GUILayout.Space(10);

//        if (GUILayout.Button("建立AssetBundle打包前，项目相关文件夹"))
//        {
//            if (!Directory.Exists(CONFIGPATH))
//            {
//                Directory.CreateDirectory(CONFIGPATH);
//            }
//            var sudiopaths = System.Enum.GetNames(typeof(AppAudio.AudioPath));
//            foreach (var item in sudiopaths)
//            {
//                if (!Directory.Exists(ASSETBUNDLEPATH + "/music/" + item))
//                    Directory.CreateDirectory(ASSETBUNDLEPATH + "/music/" + item);
//            }
//            if (!Directory.Exists(ASSETBUNDLEPATH + "/surface"))
//            {
//                Directory.CreateDirectory(ASSETBUNDLEPATH + "/surface");
//            }
//            AssetDatabase.Refresh(ImportAssetOptions.Default);
//        }
//        if (GUILayout.Button("预览资源"))
//        {
//            bundlepathinfo = new DirectoryInfo(ASSETBUNDLEPATH);
//            canpreview = !canpreview;
//        }
//        if (canpreview)
//        {
//            Yulan();
//        }
//        GUILayout.Space(10);
//        cuspath = EditorGUILayout.TextField("生成路径", cuspath);
//        using (new EditorGUILayout.HorizontalScope())
//        {
//            if (GUILayout.Button("自定义"))
//                cuspath = EditorUtility.OpenFolderPanel("选择路径", cuspath, "");
//            if (GUILayout.Button("StreamingAssets"))
//                cuspath = Application.streamingAssetsPath;
//            if (GUILayout.Button("AppConfig"))
//                cuspath = UnityEngine.Object.FindObjectOfType<GameApp>()?.appConfig.updateLocal;
//        }
//        GUILayout.Space(10);
//        if (GUILayout.Button("删除路径下的数据"))
//            Delete();

//        GUILayout.Space(30);
//        using (new EditorGUILayout.HorizontalScope())
//        {
//#if UNITY_STANDALONE_WIN
//            if (GUILayout.Button("生成Windows", GUILayout.Width(150), GUILayout.Height(25)))
//            {
//                if (CheckCuspath())
//                    BuildAssetBundles(BuildTarget.StandaloneWindows64);
//            }
//#else
//            if (GUILayout.Button("生成Andriod", GUILayout.Width(150), GUILayout.Height(25)))
//            {
//                if (CheckCuspath())
//                    BuildAssetBundles(BuildTarget.Android);
//            }
//#endif
//        }
//    }

//    private void Delete()
//    {
//        if (!string.IsNullOrWhiteSpace(cuspath + AppConfig.BUNDLEPATN))
//        {
//            if (Directory.Exists(cuspath + AppConfig.BUNDLEPATN))
//            {
//                Directory.Delete(cuspath + AppConfig.BUNDLEPATN, true);
//            }
//            Directory.CreateDirectory(cuspath + AppConfig.BUNDLEPATN);
//        }

//    }

//    private bool CheckCuspath()
//    {
//        if (string.IsNullOrWhiteSpace(cuspath) || !Directory.Exists(cuspath))
//        {
//            cuspath = EditorUtility.OpenFolderPanel("选择路径", cuspath, "");
//            return false;
//        }
//        return true;
//    }

//    DirectoryInfo bundlepathinfo;
//    DirectoryInfo[] bundlepathchilds;
//    private void Yulan()
//    {
//        if (!bundlepathinfo.Exists)
//        {
//            EditorGUILayout.LabelField("没有资源文件夹，请建立  Assets/Prefabs/AssetBundle");
//            return;
//        }
//        else
//        {
//            if (bundlepathchilds == null)
//            {
//                bundlepathchilds = bundlepathinfo.GetDirectories("*", SearchOption.AllDirectories);
//            }
//            if (bundlepathchilds == null || bundlepathchilds.Length == 0)
//            {
//                EditorGUILayout.LabelField("资源文件夹没有子资源");
//            }
//            else
//            {
//                foreach (var item in bundlepathchilds)
//                {
//                    var dn = item.FullName;
//                    dn = dn.Remove(0, dn.IndexOf("AssetBundle") + "AssetBundle".Length + 1);
//                    EditorGUILayout.LabelField(dn);
//                }
//            }
//        }
//    }

//    void BuildAssetBundles(BuildTarget target)
//    {
//        if (target == BuildTarget.Android)
//            buildpath = cuspath + "/AndroidRes";
//        else if (target == BuildTarget.StandaloneWindows64)
//            buildpath = cuspath + "/Windows";

//        DirectoryInfo direction = new DirectoryInfo(ASSETBUNDLEPATH);
//        if (!direction.Exists)
//        {
//            Debug.LogError("Cont find path :" + ASSETBUNDLEPATH);
//            return;
//        }
//        List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>(500);
//        BuildAssetBundleBuild(direction, buildMap);
//        DirectoryInfo dir = new DirectoryInfo(buildpath);
//        if (!dir.Exists)
//            dir.Create();
//        BuildPipeline.BuildAssetBundles(buildpath, buildMap.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, target);
//        BuildConfig();
//        CreateUpdateTXT();
//        GUIUtility.ExitGUI();
//    }

//    private void BuildAssetBundleBuild(DirectoryInfo root, List<AssetBundleBuild> res)
//    {
//        var files = root.GetFiles("*", SearchOption.TopDirectoryOnly);
//        foreach (var file in files)
//        {
//            if (file.Extension != ".meta")
//            {

//                var uibuild = new AssetBundleBuild();
//                string n = file.FullName;
//                n = n.Remove(0, n.IndexOf("Assets"));
//                var pwithe = (file.FullName.Remove(0, file.FullName.IndexOf("AssetBundle") + "AssetBundle".Length + 1));
//                var p = pwithe.Remove(pwithe.LastIndexOf("."));
//                uibuild.assetBundleName = p;
//                uibuild.assetNames = new string[] { n };
//                res.Add(uibuild);
//            }
//        }
//        var dics = root.GetDirectories("*", SearchOption.TopDirectoryOnly);
//        foreach (var childPath in dics)
//            BuildAssetBundleBuild(childPath, res);
//    }

//    // 更新MD5
//    private void CreateUpdateTXT()
//    {
//        string[] files = Directory.GetFiles(buildpath, "*.*", SearchOption.AllDirectories)
//            .Where(s => !s.EndsWith(".meta")).ToArray();
//        //上层目录
//        string path1 = buildpath.Replace("/AndroidRes", string.Empty).Replace("/Windows", string.Empty);
//        StringBuilder stringBuilder = new StringBuilder();
//        foreach (string filePath in files)
//        {
//            if (new FileInfo(filePath).Name == "version.txt")
//                continue;
//            var ffp = filePath.Substring(path1.Length + 1);
//            string md5 = GameUnlity.BuildFileMd5(filePath);
//            stringBuilder.AppendLine(string.Format("{0}:{1}", ffp.Replace('\\', '/'), md5));
//        }

//        var hellopath = path1 + "/Hello/Helloworld"; //GameUnlity.GetstreamingAssets("Hello", "HotFix_Project.bytes").AbsolutePath;
//        if (File.Exists(hellopath))
//        {
//            var ffp = hellopath.Substring(path1.Length + 1);
//            string md5 = GameUnlity.BuildFileMd5(hellopath);
//            stringBuilder.AppendLine(string.Format("{0}:{1}", ffp.Replace('\\', '/'), md5));
//        }
//        string updatePath = Path.Combine(buildpath, "version.txt");
//        Debug.Log("Build version file :" + updatePath);
//        GameUnlity.CreateTXT(updatePath, stringBuilder.ToString(), Encoding.UTF8);
//        Debug.Log("----------Build Complete---------");
//    }

//    private void BuildConfig()
//    {
//        DirectoryInfo direction = new DirectoryInfo(CONFIGPATH);
//        if (!direction.Exists)
//            return;
//        var fs = direction.GetFiles();
//        for (int i = 0; i < fs.Length; i++)
//        {
//            if (fs[i].Extension == ".meta")
//                continue;
//            if (fs[i].Name.ToLower().Contains("config"))
//            {
//                var ff = fs[i];
//                CreateConfig(ff);
//            }
//        }
//    }

//    private void CreateConfig(FileInfo file)
//    {
//        try
//        {
//            using (ExcelPackage package = new ExcelPackage(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)))
//            {
//                ExcelWorksheet sheet;
//                var count = package.Workbook.Worksheets.Count;

//                StringBuilder stringBuilder = new StringBuilder(20000);

//                if (count > 0)
//                {
//                    sheet = package.Workbook.Worksheets[1];
//                    if (sheet.Dimension.Equals(null))
//                        throw new System.Exception("Excel列表没有内容");
//                    BuildLan(sheet, stringBuilder);
//                }
//                int cs, ce, rs, re;
//                for (int sheetnum = 2; sheetnum < count + 1; sheetnum++)
//                {
//                    stringBuilder.Clear();
//                    sheet = package.Workbook.Worksheets[sheetnum];
//                    if (sheet.Dimension == null)
//                    {
//                        Debug.Log("Excel（表" + sheet.Name + "）没有Config内容");
//                        return;
//                    }
//                    cs = sheet.Dimension.Start.Column;   //起始1
//                    ce = sheet.Dimension.End.Column + 1;
//                    rs = sheet.Dimension.Start.Row;
//                    re = sheet.Dimension.End.Row + 1;
//                    //+1 去掉首行Title
//                    for (int i = rs + 1; i < re; i++)
//                    {
//                        for (int n = cs; n < ce; n++)
//                        {
//                            stringBuilder.Append(sheet.GetValue(i, n) + ":");
//                        }
//                        stringBuilder.Remove(stringBuilder.Length - 1, 1);
//                        stringBuilder.Append("\n");
//                    }
//                    string mudulepath = $"{buildpath}/{sheet.Name}.txt";
//                    using (StreamWriter sw = new StreamWriter(mudulepath, false, Encoding.UTF8))
//                    {
//                        Debug.Log("Build Config file :" + mudulepath);
//                        sw.Write(AppConfig.EnAes(stringBuilder.ToString()));
//                        //sw.Write(stringBuilder.ToString());
//                    }
//                }
//                stringBuilder.Clear();
//            }
//        }
//        catch (System.IO.IOException e)
//        {
//            throw e;
//        }
//    }

//    private void BuildLan(ExcelWorksheet sheet, StringBuilder stringBuilder)
//    {
//        int cs = sheet.Dimension.Start.Column;   //起始1
//        int ce = sheet.Dimension.End.Column + 1;
//        int rs = sheet.Dimension.Start.Row;
//        int re = sheet.Dimension.End.Row + 1;
//        List<string> ids = new List<string>();
//        for (int i = rs; i < re; i++)
//        {
//            ids.Add(sheet.GetValue(i, 1).ToString());
//        }
//        //+1 去掉首行Title
//        for (int i = cs + 1; i < ce; i++)
//        {
//            stringBuilder.Clear();
//            for (int n = rs + 1; n < re; n++)
//            {
//                stringBuilder.Append(ids[n - 1] + ":" + sheet.GetValue(n, i).ToString() + "\n");
//            }
//            string lanpath = Path.Combine(buildpath, "Lan", sheet.GetValue(1, i).ToString() + ".txt");
//            string dir = Path.GetDirectoryName(lanpath);
//            if (!Directory.Exists(dir))
//                Directory.CreateDirectory(dir);
//            using (StreamWriter sw = new StreamWriter(lanpath, false, Encoding.UTF8))
//            {
//                Debug.Log("Build Lan file :" + lanpath);
//                sw.Write(AppConfig.EnAes(stringBuilder.ToString()));
//                //sw.Write(stringBuilder.ToString());
//            }
//        }
//    }
//}