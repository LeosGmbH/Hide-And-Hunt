using UnityEngine;

public class MusicManager : MonoBehaviour {
    private const string PLAYER_PREFS_MUSIC_VOLUME = "MusicVolume";

    public static MusicManager Instance { get; private set; }

    private AudioSource audioSource;
    private float volume;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();

        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_MUSIC_VOLUME, 0.5f);
        audioSource.volume = volume;
    }

    public void ChangeVolume(float value) {
        audioSource.volume = value;

        PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUME, value);
        PlayerPrefs.Save();
    }

    public float GetVolume() {
        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_MUSIC_VOLUME, .5f);
        return volume;
    }

}