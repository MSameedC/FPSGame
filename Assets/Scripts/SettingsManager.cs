using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    
    [SerializeField] private AudioMixer audioMixer;
    
    private const string RES_KEY = "resolution";
    private const string QUALITY_KEY = "quality";
    private const string RENDERSCALE_KEY = "renderScale";
    private const string MASTERVOLUME_KEY = "masterVolume";
    private const string MUSICVOLUME_KEY = "musicVolume";
    private const string SFXVOLUME_KEY = "sfxVolume";
    private const string VSYNC_KEY = "vsync";

    private const string MASTER_VOL = "MasterVol";
    private const string MUSIC_VOL = "MusicVol";
    private const string SFX_VOL = "SFXVol";
    
    // ---
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadSettings();
        ApplySettings();
    }
    
    public int ResolutionIndex { get; private set; }
    public int QualityIndex { get; private set; }
    public float RenderingScale { get; private set; }
    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }
    public bool VSync { get; private set; }

    private void LoadSettings()
    {
        ResolutionIndex = PlayerPrefs.GetInt(RES_KEY, 0);
        QualityIndex = PlayerPrefs.GetInt(QUALITY_KEY, 0);
        RenderingScale = PlayerPrefs.GetFloat(RENDERSCALE_KEY, 1f);
        MasterVolume = PlayerPrefs.GetFloat(MASTERVOLUME_KEY, 1f);
        MusicVolume = PlayerPrefs.GetFloat(MUSICVOLUME_KEY, 1f);
        SfxVolume = PlayerPrefs.GetFloat(SFXVOLUME_KEY, 1f);
        VSync = PlayerPrefs.GetInt(VSYNC_KEY, 1) == 1;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt(RES_KEY, ResolutionIndex);
        PlayerPrefs.SetInt(QUALITY_KEY, QualityIndex);
        PlayerPrefs.SetInt(VSYNC_KEY, VSync ? 1 : 0);
        PlayerPrefs.SetFloat(RENDERSCALE_KEY, RenderingScale);
        PlayerPrefs.SetFloat(MASTERVOLUME_KEY, MasterVolume);
        PlayerPrefs.SetFloat(MUSICVOLUME_KEY, MusicVolume);
        PlayerPrefs.SetFloat(SFXVOLUME_KEY, SfxVolume);
        PlayerPrefs.Save();
    }
    
    // -------- APPLY -----------
    public void ApplyResolution(int index)
    {
        ResolutionIndex = index;
        Resolution res = Screen.resolutions[index];
        Screen.SetResolution(res.width, res.height, FullScreenMode.FullScreenWindow);

        SaveSettings();
    }

    public void ApplyRenderScale(float scale)
    {
        RenderingScale = scale;
    
        // Force pipeline reload
        var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urp != null)
        {
            urp.renderScale = scale;
            // Force a camera refresh
            Camera.main.Reset();
        }
    
        SaveSettings();
    }

    public void ApplyQuality(int qualityIndex)
    {
        QualityIndex = qualityIndex;
        QualitySettings.SetQualityLevel(qualityIndex, true);
        
        SaveSettings();
    }

    public void ApplyVSync(bool state)
    {
        VSync = state;
        QualitySettings.vSyncCount = state ? 1 : 0;

        SaveSettings();
    }

    public void ApplyMasterVolume(float volume)
    {
        MasterVolume = volume;
        audioMixer.SetFloat(MASTER_VOL, LinearToDecibel(volume));

        SaveSettings();
    }
    
    public void ApplyMusicVolume(float volume)
    {
        MusicVolume = volume;
        audioMixer.SetFloat(MUSIC_VOL, LinearToDecibel(volume));

        SaveSettings();
    }
    
    public void ApplySfxVolume(float volume)
    {
        SfxVolume = volume;
        audioMixer.SetFloat(SFX_VOL, LinearToDecibel(volume));

        SaveSettings();
    }

    public void ApplySettings()
    {
        ApplyResolution(ResolutionIndex);
        ApplyRenderScale(RenderingScale);
        ApplyQuality(QualityIndex);
        ApplyVSync(VSync);
        ApplyMasterVolume(MasterVolume);
        ApplyMusicVolume(MusicVolume);
        ApplySfxVolume(SfxVolume);
    }
    
    float LinearToDecibel(float linear)
    {
        return linear <= 0.0001f ? -80f : Mathf.Lerp(-80f, 0f, linear);
    }

}
