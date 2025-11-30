using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {


    [SerializeField] private Button playButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Slider volumeSlider;
    private MusicManager musicManager;


    private void Awake() {
        // Stelle sicher, dass der Slider mit der persistierenden AudioSource verbunden ist
        var persistantMusicManager = FindObjectOfType<PersistentAudio>();
        if (persistantMusicManager != null) {
            musicManager = persistantMusicManager.GetComponent<MusicManager>();
        }
        else {
            Debug.LogError("MusicManager not found in the scene.");
        }

        playButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.LobbyScene);
        });

        optionButton.onClick.AddListener(() => {
            volumeSlider.value = musicManager.GetVolume();
        });

        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });

        volumeSlider.value = musicManager.GetVolume();
        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.onValueChanged.AddListener(SetVolume);

        Time.timeScale = 1f;
    }

    private void Start()
    {
        // Stelle sicher, dass der Slider mit der persistierenden AudioSource verbunden ist
        var persistantMusicManager = FindObjectOfType<PersistentAudio>();
        if (persistantMusicManager != null) {
            musicManager = persistantMusicManager.GetComponent<MusicManager>();
        }
        else {
            Debug.LogError("MusicManager not found in the scene.");
        }
    }

    public void SetVolume(float volume) {
        if (musicManager != null) {
            musicManager.ChangeVolume(volume);
        }
    }
}