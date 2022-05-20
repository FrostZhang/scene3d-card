using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HouseWeather : HassEntity
{
    public static HouseWeather Instance;
    public string Skyname { get => skyname; }

    public Light mainlight;
    public Color32 cH = new Color32(255, 255, 218, 255);
    public Color32 cL = new Color32(0, 0, 0, 255);

    public Volume volume;
    private string lastWeather;
    string skyname;
    bool ready;
    ParticleSystem rmain;
    ParticleSystem smain;
    ParticleSystem fmain;
    ParticleSystem lmain;

    void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        await LoadRain("rain");
        await LoadSnow("snow");
        await LoadFog("fog");
        await LoadLighting("lighting");
        ready = true;
    }

    public void SetTianGuang(float v)
    {
        mainlight.color = Color.Lerp(cL, cH, v);
        if (RenderSettings.skybox)
            RenderSettings.skybox.SetFloat("_Exposure", v + 0.15f);
    }
    public void SetRain(float value)
    {
        if (rmain == null)
            return;
        var m = rmain.main;
        m.maxParticles = (int)value;
    }
    public void SetSnow(float value)
    {
        if (smain == null)
            return;
        var m = smain.main;
        m.maxParticles = (int)value;
    }
    public void SetFog(float value)
    {
        if (fmain == null)
            return;
        var m = fmain.main;
        m.maxParticles = (int)value;
    }
    public void SetLighting(float value)
    {
        if (lmain == null)
            return;
        var m = lmain.main;
        m.maxParticles = (int)value;
    }
    protected override void Destine(string state)
    {
        if (ready)
            SetWeather(state);
    }
    internal void SetWeather(string str)
    {
        if (lastWeather == str)
            return;
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
            case "cloudy":
            case "partlycloudy":
            case "sunny":
                break;
            case "”–∑Á":
            case "Œ¢∑Á":
            case "∫Õ∑Á":
            case "«Â∑Á":
            case "windy":
                break;
            case "«ø∑Á":
            case "º≤∑Á":
            case "¥Û∑Á":
            case "¡“∑Á":
                break;
            case "Ï´∑Á":
            case "¡˙æÌ∑Á":
            case "»»¥¯∑Á±©":
            case "øÒ±©∑Á":
            case "∑Á±©":
                break;
            case "–°”Í":
            case "”Í":
            case "√´√´”Í":
            case "rainy":
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
            case "storm":
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
            case "thunderstorm":
                SetRain(700);
                SetLighting(3);
                break;
            case "—©":
            case "–°—©":
            case "snow":
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
        var m = fmain.main;
        var c = m.startColor;
        var y = color;
        y.a = 0.19f;
        c.color = y;
        m.startColor = c;
    }

    public async Task CreatSky(string str)
    {
        if (string.IsNullOrWhiteSpace(str) || skyname == str) return;
        skyname = str;
        await Help.Instance.ABLoad("sky", str);
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
        var ps = Instantiate(parti, transform).GetComponent<ParticleSystem>();
        if (!ps) goto err;
        rmain = ps;
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
        var ps = Instantiate(parti, transform).GetComponent<ParticleSystem>();
        if (!ps) goto err;
        smain = ps;
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
        var ps = Instantiate(parti, transform).GetComponent<ParticleSystem>();
        if (!ps) goto err;
        fmain = ps;
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
        var ps = Instantiate(parti, transform).GetComponent<ParticleSystem>();
        if (!ps) goto err;
        lmain = ps;
        SetLighting(0);
        yield break;
    err:
        Debug.LogError("‘ÿ»Î¥ÌŒÛ");
    }

    public void SetBloom(float value)
    {
        Bloom bl;
        if (volume.sharedProfile.TryGet<Bloom>(out bl))
        {
            bl.intensity.value = value;
        }
    }

    public void SetVignette(float value)
    {
        Vignette bl;
        if (volume.sharedProfile.TryGet<Vignette>(out bl))
        {
            bl.intensity.value = value;
        }
    }

    public void SetVignette(Color value)
    {
        Vignette bl;
        if (volume.sharedProfile.TryGet<Vignette>(out bl))
        {
            bl.color.value = value;
        }
    }

    Coroutine dangerCor;
    public void SetInDanger(bool value)
    {
        if (!value)
        {
            if (dangerCor != null)
            {
                StopCoroutine(dangerCor);
                dangerCor = null;
            }
            return;
        }
        if (dangerCor != null)
            return;
        dangerCor = StartCoroutine(danger());
    }

    IEnumerator danger()
    {
        SetVignette(Color.red);
        while (true)
        {
            SetVignette(Mathf.PingPong(Time.time, 0.5f));
            yield return null;
        }
    }
}
