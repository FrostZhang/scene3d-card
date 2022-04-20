using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseWeather : MonoBehaviour
{
    public static HouseWeather Instance;

    ParticleSystem.MainModule rmain;
    ParticleSystem.MainModule smain;
    ParticleSystem.MainModule fmain;
    ParticleSystem.MainModule lmain;
    public Light mainlight;
    public Color32 cH = new Color32(255, 255, 218, 255);
    public Color32 cL = new Color32(0, 0, 0, 255);
    private string lastWeather;

    readonly string[] weather = new string[] {
    "«Á", "∂‡‘∆", "…Ÿ‘∆" , "«Áº‰∂‡‘∆", "“ı" ,//4
    "”–∑Á", "Œ¢∑Á", "∫Õ∑Á", "«Â∑Á","«ø∑Á", "º≤∑Á", "¥Û∑Á", "¡“∑Á",//12
    "Ï´∑Á", "¡˙æÌ∑Á", "»»¥¯∑Á±©", "øÒ±©∑Á", "∑Á±©",//17
    "”Í", "√´√´”Í", "–°”Í", "÷–”Í", "¥Û”Í", "’Û”Í",//23
    "±©”Í", "¥Û±©”Í", "Ãÿ¥Û±©”Í", "«ø’Û”Í", "º´∂ÀΩµ”Í",//28
    "¿◊’Û”Í", "«ø¿◊’Û”Í",//30
    "ŒÌ", "±°ŒÌ","ˆ≤",//33
    "¿◊’Û”Í∞È”–±˘±¢",//34
    "—©", "–°—©", "÷–—©", "¥Û—©", "±©—©", "’Û—©",//40
    "”Íº–—©", "”Í—©ÃÏ∆¯", "’Û”Íº–—©"//43
    };
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(LoadRain("rain"));
        StartCoroutine(LoadSnow("snow"));
        StartCoroutine(LoadFog("fog"));
        StartCoroutine(LoadLighting("lighting"));
    }

    public void SetTianGuang(float v)
    {
        mainlight.color = Color.Lerp(cL, cH, v);
        RenderSettings.skybox.SetFloat("_Exposure", v + 0.15f);
    }
    public void SetRain(float value)
    {
        rmain.maxParticles = (int)value;
    }
    public void SetSnow(float value)
    {
        smain.maxParticles = (int)value;
    }
    public void SetFog(float value)
    {
        fmain.maxParticles = (int)value;
    }
    public void SetLighting(float value)
    {
        lmain.maxParticles = (int)value;
    }

    internal void SetWeather(string str)
    {
        if (lastWeather == str)
        {
            return;
        }
        lastWeather = str;
        SetRain(0);
        SetSnow(0);
        SetLighting(0);
        SetFog(0);
        switch (str)
        {
            case "«Á":
            case "∂‡‘∆":
            case "…Ÿ‘∆":
            case "«Áº‰∂‡‘∆":
            case "“ı":

                break;
            case "–°”Í":
            case "”Í":
            case "√´√´”Í":
                SetRain(200);
                break;
            case "÷–”Í":
                SetRain(300);
                break;
            case "¥Û”Í":
                SetRain(400);
                break;
            case "’Û”Í":
                SetRain(500);
                break;
            case "±©”Í":
            case "¥Û±©”Í":
                SetRain(700);
                break;
            case "Ãÿ¥Û±©”Í":
                SetRain(800);
                break;
            case "«ø’Û”Í":
            case "º´∂ÀΩµ”Í":
                SetRain(1000);
                break;
            case "¿◊’Û”Í":
            case "«ø¿◊’Û”Í":
                SetRain(700);
                SetLighting(3);
                break;
            case "—©":
            case "–°—©":
                SetSnow(200);
                break;
            case "÷–—©":
                SetSnow(400);
                break;
            case "¥Û—©":
                SetSnow(700);
                break;
            case "±©—©":
            case "’Û—©":
                SetSnow(1000);
                break;
            case "”Íº–—©":
                SetSnow(300);
                SetRain(200);
                break;
            case "”Í—©ÃÏ∆¯":
                SetSnow(400);
                SetRain(300);
                break;
            case "’Û”Íº–—©":
                SetSnow(700);
                SetRain(500);
                break;
            case "ŒÌ":
                SetFog(40);  //80
                ChangeFogColor(Color.white);
                break;
            case "±°ŒÌ":
                SetFog(20);  //80
                ChangeFogColor(Color.white);
                break;
            case "ˆ≤":
                SetFog(80);  //80
                ChangeFogColor(Color.yellow);
                break;
            default:
                break;
        }
    }

    private void ChangeFogColor(Color color)
    {
        var c = fmain.startColor;
        var y = color;
        y.a = 0.19f;
        c.color = y;
        fmain.startColor = c;
    }

    public IEnumerator CreatSky(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            yield break;
        yield return Help.Instance.ABLoad("sky", str);
        var ab = Help.Instance.GetBundle("sky", str);
        if (ab)
        {
            var ma = ab.LoadAsset<Material>(str);
            if (ma)
                RenderSettings.skybox = ma;
            SetTianGuang(0.1f);
        }
    }

    IEnumerator LoadRain(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) goto err;
        yield return Help.Instance.ABLoad("weather", str);
        var ab = Help.Instance.GetBundle("weather", str);
        if (!ab) goto err;
        var parti = ab.LoadAsset<GameObject>(str);
        if (!parti) goto err;
        var ps = Instantiate(parti).GetComponent<ParticleSystem>();
        if (!ps) goto err;
        rmain = ps.main;
        SetRain(0);
        yield break;
    err:
        Debug.LogError("‘ÿ»Î¥ÌŒÛ");
    }

    IEnumerator LoadSnow(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) goto err;
        yield return Help.Instance.ABLoad("weather", str);
        var ab = Help.Instance.GetBundle("weather", str);
        if (!ab) goto err;
        var parti = ab.LoadAsset<GameObject>(str);
        if (!parti) goto err;
        var ps = Instantiate(parti).GetComponent<ParticleSystem>();
        if (!ps) goto err;
        smain = ps.main;
        SetSnow(0);
        yield break;
    err:
        Debug.LogError("‘ÿ»Î¥ÌŒÛ");
    }

    IEnumerator LoadFog(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) goto err;
        yield return Help.Instance.ABLoad("weather", str);
        var ab = Help.Instance.GetBundle("weather", str);
        if (!ab) goto err;
        var parti = ab.LoadAsset<GameObject>(str);
        if (!parti) goto err;
        var ps = Instantiate(parti).GetComponent<ParticleSystem>();
        if (!ps) goto err;
        fmain = ps.main;
        SetFog(0);
        yield break;
    err:
        Debug.LogError("‘ÿ»Î¥ÌŒÛ");
    }

    IEnumerator LoadLighting(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) goto err;
        yield return Help.Instance.ABLoad("weather", str);
        var ab = Help.Instance.GetBundle("weather", str);
        if (!ab) goto err;
        var parti = ab.LoadAsset<GameObject>(str);
        if (!parti) goto err;
        var ps = Instantiate(parti).GetComponent<ParticleSystem>();
        if (!ps) goto err;
        lmain = ps.main;
        SetLighting(0);
        yield break;
    err:
        Debug.LogError("‘ÿ»Î¥ÌŒÛ");
    }

}
