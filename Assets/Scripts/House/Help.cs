using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Help : MonoBehaviour
{
    List<string> works;

    public static Help Instance;
    Dictionary<string, AssetBundle> bundles;
    private void Awake()
    {
        Instance = this;
        works = new List<string>();
        bundles = new Dictionary<string, AssetBundle>(50);
    }

    public IEnumerator ABLoad(string root, string key)
    {
        string dk = key + "_" + root;
        if (bundles.ContainsKey(dk))
        {
            yield break;
        }
        else if (works.Contains(dk))
        {
            yield return new WaitUntil(() => bundles.ContainsKey(dk));
            yield break;
        }
        var path = $"{ Application.streamingAssetsPath}/{root}/{dk}";
        System.Uri uri = new System.Uri(path);
        yield return Request(dk, uri);
    }

    /// <summary>
    /// 在获取bundle前最好 ABLoad！
    /// </summary>
    public AssetBundle GetBundle(string root, string key)
    {
        string dk = key + "_" + root;
        if (bundles.ContainsKey(dk))
        {
            return bundles[dk];
        }
        return null;
    }

    IEnumerator Request(string dk, System.Uri uri)
    {
        works.Add(dk);
        using (var request = UnityWebRequestAssetBundle.GetAssetBundle(uri))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                bundles.Add(dk, null);
                Debug.LogError("找不到ab");
            }
            else
            {
                bundles.Add(dk, DownloadHandlerAssetBundle.GetContent(request));
            }
        }
        works.Remove(dk);
    }
}
