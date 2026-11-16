using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Standalone settings page script for audio.
/// - BGM and SFX volume sliders
/// - BGM and SFX mute toggles (ticked = muted)
/// - ALL saving and loading (PlayerPrefs) happens in this script only.
/// Saved settings are also applied automatically when the game starts,
/// even if the settings page is never opened.
/// </summary>
public class AudioSettingsMenu : MonoBehaviour
{
    private const string BGMVolumeKey = "Settings_BGMVolume";
    private const string SFXVolumeKey = "Settings_SFXVolume";
    private const string BGMMutedKey  = "Settings_BGMMuted";
    private const string SFXMutedKey  = "Settings_SFXMuted";

    private const float DefaultVolume = 1f;

    [Header("Volume Sliders (Min 0, Max 1)")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Mute Toggles (ticked = muted)")]
    [SerializeField] private Toggle bgmMuteToggle;
    [SerializeField] private Toggle sfxMuteToggle;

    // ---------------- Apply saved settings at game start ----------------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyOnStartup()
    {
        // Re-apply on every scene load too, in case the AudioManager
        // only appears in a later scene.
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        ApplySavedSettings();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySavedSettings();
    }

    private static void ApplySavedSettings()
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.SetBGMVolume(PlayerPrefs.GetFloat(BGMVolumeKey, DefaultVolume));
        AudioManager.Instance.SetSFXVolume(PlayerPrefs.GetFloat(SFXVolumeKey, DefaultVolume));
        AudioManager.Instance.SetBGMMuted(PlayerPrefs.GetInt(BGMMutedKey, 0) == 1);
        AudioManager.Instance.SetSFXMuted(PlayerPrefs.GetInt(SFXMutedKey, 0) == 1);
    }

    // ---------------- Settings page ----------------

    private void OnEnable()
    {
        // Show the saved values without triggering the change handlers.
        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(BGMVolumeKey, DefaultVolume));

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SFXVolumeKey, DefaultVolume));

        if (bgmMuteToggle != null)
            bgmMuteToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(BGMMutedKey, 0) == 1);

        if (sfxMuteToggle != null)
            sfxMuteToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(SFXMutedKey, 0) == 1);

        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (bgmMuteToggle != null) bgmMuteToggle.onValueChanged.AddListener(OnBGMMuteChanged);
        if (sfxMuteToggle != null) sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteChanged);

        ApplySavedSettings();
    }

    private void OnDisable()
    {
        if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        if (bgmMuteToggle != null) bgmMuteToggle.onValueChanged.RemoveListener(OnBGMMuteChanged);
        if (sfxMuteToggle != null) sfxMuteToggle.onValueChanged.RemoveListener(OnSFXMuteChanged);

        // Write to disk when the settings page closes.
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        // Mobile: make sure settings survive the app being closed in the background.
        if (paused)
            PlayerPrefs.Save();
    }

    private void OnBGMVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(BGMVolumeKey, value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SFXVolumeKey, value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    private void OnBGMMuteChanged(bool muted)
    {
        PlayerPrefs.SetInt(BGMMutedKey, muted ? 1 : 0);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMMuted(muted);
    }

    private void OnSFXMuteChanged(bool muted)
    {
        PlayerPrefs.SetInt(SFXMutedKey, muted ? 1 : 0);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXMuted(muted);
    }

    // Optional: hook to a "Reset" button on the settings page.
    public void ResetToDefaults()
    {
        PlayerPrefs.SetFloat(BGMVolumeKey, DefaultVolume);
        PlayerPrefs.SetFloat(SFXVolumeKey, DefaultVolume);
        PlayerPrefs.SetInt(BGMMutedKey, 0);
        PlayerPrefs.SetInt(SFXMutedKey, 0);

        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(DefaultVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(DefaultVolume);
        if (bgmMuteToggle != null) bgmMuteToggle.SetIsOnWithoutNotify(false);
        if (sfxMuteToggle != null) sfxMuteToggle.SetIsOnWithoutNotify(false);

        ApplySavedSettings();
        PlayerPrefs.Save();
    }
}
