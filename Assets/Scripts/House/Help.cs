using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Help : MonoBehaviour
{
    List<string> works;

    public static Help Instance;
    Dictionary<string, AssetBundle> bundles;
    Dictionary<string, Color> colors;
    private void Awake()
    {
        Instance = this;
        works = new List<string>();
        bundles = new Dictionary<string, AssetBundle>(50);
        colors = new Dictionary<string, Color>(10);
        colors.Add("red", Color.red);
        colors.Add("grey", Color.grey);
        colors.Add("gray", Color.gray);
        colors.Add("yellow", Color.yellow);
        colors.Add("magenta", Color.magenta);
        colors.Add("cyan", Color.cyan);
        colors.Add("black", Color.black);
        colors.Add("white", Color.white);
        colors.Add("blue", Color.blue);
        colors.Add("green", Color.green);
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
                Debug.LogError("找不到ab:"+dk);
            }
            else
            {
                bundles.Add(dk, DownloadHandlerAssetBundle.GetContent(request));
            }
        }
        works.Remove(dk);
    }

    public bool TryColor(string cstr, out Color va)
    {
        if (string.IsNullOrWhiteSpace(cstr))
        {

        }
        else if (cstr.StartsWith("#"))
        {
            return ColorUtility.TryParseHtmlString(cstr, out va);
        }
        else if (colors.ContainsKey(cstr))
        {
            va = colors[cstr];
            return true;
        }
        else
        {
            return RGBAStr2Color(cstr, out va);
        }
        va = Color.magenta;
        return false;

    }

    bool RGBAStr2Color(string str, out Color color)
    {
        var css = str.Split(',');
        byte va;
        if (css.Length == 4)
        {
            byte r, g, b, a;
            if (byte.TryParse(css[0], out va))
                r = va;
            else goto end;
            if (byte.TryParse(css[1], out va))
                g = va;
            else goto end;
            if (byte.TryParse(css[2], out va))
                b = va;
            else goto end;
            if (byte.TryParse(css[3], out va))
                a = va;
            else goto end;
            color = new Color32(r, g, b, a);
            return true;
        }
    end:
        color = Color.magenta;
        return false;
    }

}
