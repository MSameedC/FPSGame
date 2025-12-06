using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsUi : MonoBehaviour
{
    public static SettingsUi Instance;
    
    private UIDocument uiDoc;
    
    private VisualElement root;
    private VisualElement audioRoot;
    private VisualElement videoRoot;

    private DropdownField resolutionDropbox;
    private DropdownField qualityDropbox;
    private Slider renderingScaleSlider;
    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;
    private Toggle vsyncToggle;

    private SettingsManager SettingsManager;
    
    public bool Active {get; private set;}
    
    // ---

    private void Awake()
    {
        Instance = this;
        uiDoc = GetComponent<UIDocument>();
    }

    private void Start()
    {
        SettingsManager = SettingsManager.Instance;
        if (SettingsManager == null)
        {
            Debug.LogError("Settings Manager not found!");
            return;
        }
        
        root = uiDoc.rootVisualElement;
        
        root.Q<Button>("audio-btn").clickable.clicked += OpenAudio;
        root.Q<Button>("video-btn").clickable.clicked += OpenVideo;
        root.Q<Button>("back-btn").clickable.clicked += ExitSettings;
        
        audioRoot =  root.Q<VisualElement>("Audio");
        videoRoot =  root.Q<VisualElement>("Video");
        
        // Video
        resolutionDropbox = videoRoot.Q<DropdownField>("resolution-dropbox");
        qualityDropbox = videoRoot.Q<DropdownField>("quality-dropbox");
        renderingScaleSlider = videoRoot.Q<Slider>("rendering-scale-slider");
        vsyncToggle = videoRoot.Q<Toggle>("vsync-toggle");
        
        // Audio
        masterVolumeSlider = audioRoot.Q<Slider>("master-slider");
        musicVolumeSlider = audioRoot.Q<Slider>("music-slider");
        sfxVolumeSlider = audioRoot.Q<Slider>("sfx-slider");
        
        // Populate resolution dropdown
        List<string> resOptions = new List<string>();
        foreach (var res in Screen.resolutions)
        {
            resOptions.Add(res.width + " x " + res.height);
        }
        resolutionDropbox.choices = resOptions;
        
        // Populate quality dropdown
        List<string> qualityOptions = new List<string>();
        foreach (var quality in QualitySettings.names)
        {
            qualityOptions.Add(quality);
        }
        qualityDropbox.choices = qualityOptions;

        RegisterCallbacks();
        LoadUiValues();
        ExitSettings();
    }

    private void LoadUiValues()
    {
        // Resolution
        resolutionDropbox.index = Mathf.Clamp(SettingsManager.ResolutionIndex, 0, resolutionDropbox.choices.Count - 1);
        // Quality
        qualityDropbox.index = Mathf.Clamp(SettingsManager.QualityIndex, 0, qualityDropbox.choices.Count - 1); 
        // Render Scale
        renderingScaleSlider.value = SettingsManager.RenderingScale;
        renderingScaleSlider.highValue = 1.2f;
        renderingScaleSlider.lowValue = 0.25f;
        // Master
        masterVolumeSlider.value = Mathf.Max(SettingsManager.MasterVolume, 0.001f);;
        masterVolumeSlider.highValue = 1f;
        masterVolumeSlider.lowValue = 0;
        // Music
        musicVolumeSlider.value = Mathf.Max(SettingsManager.MusicVolume, 0.001f);;
        musicVolumeSlider.highValue = 1f;
        musicVolumeSlider.lowValue = 0;
        // Sfx
        sfxVolumeSlider.value = Mathf.Max(SettingsManager.SfxVolume, 0.001f);;
        sfxVolumeSlider.highValue = 1f;
        sfxVolumeSlider.lowValue = 0;
        // Vsync
        vsyncToggle.value = SettingsManager.VSync;
    }

    public void EnterSettings()
    {
        root.style.display = DisplayStyle.Flex;
        Active = true;
    }
    private void ExitSettings()
    {
        root.style.display = DisplayStyle.None;
        Active = false;
    }

    private void OpenAudio()
    {
        audioRoot.style.display = DisplayStyle.Flex;
        ExitVideo();
    }
    private void ExitAudio()
    {
        audioRoot.style.display = DisplayStyle.None;
    }

    private void OpenVideo()
    {
        videoRoot.style.display = DisplayStyle.Flex;
        ExitAudio();
    }
    private void ExitVideo()
    {
        videoRoot.style.display = DisplayStyle.None;
    }
    
    private void RegisterCallbacks()
    {
        resolutionDropbox.RegisterValueChangedCallback(evt =>
        {
            SettingsManager.ApplyResolution(resolutionDropbox.index);
        });

        qualityDropbox.RegisterValueChangedCallback(evt =>
        {
            SettingsManager.ApplyQuality(qualityDropbox.index);
        });

        renderingScaleSlider.RegisterValueChangedCallback(evt =>
        {
            SettingsManager.ApplyRenderScale(evt.newValue);
        });

        masterVolumeSlider.RegisterValueChangedCallback(evt =>
        {
            SettingsManager.ApplyMasterVolume(evt.newValue);
        });
        
        musicVolumeSlider.RegisterValueChangedCallback(evt =>
        {
            SettingsManager.ApplyMusicVolume(evt.newValue);
        });
        
        sfxVolumeSlider.RegisterValueChangedCallback(evt =>
        {
            SettingsManager.ApplySfxVolume(evt.newValue);
        });

        vsyncToggle.RegisterValueChangedCallback(evt =>
        {
            SettingsManager.ApplyVSync(evt.newValue);
        });
    }
}
