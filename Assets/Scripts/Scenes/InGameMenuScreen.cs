#if UNITY_EDITOR
using System.Diagnostics;
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class InGameMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameObject gamePauseUI;
        [SerializeField] private GameObject gamePauseOptionUI;
        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private GameObject winMessage;

        [SerializeField] private Button optionButton;
        [SerializeField] private Slider volumeSlider;
        private MusicManager musicManager;

        [SerializeField] private bool isPaused = false;

        void Awake()
        {
            // Stelle sicher, dass der Slider mit der persistierenden AudioSource verbunden ist
            var persistantMusicManager = FindObjectOfType<PersistentAudio>();
            if (persistantMusicManager != null) {
                musicManager = persistantMusicManager.GetComponent<MusicManager>();
            }
            else {
                UnityEngine.Debug.LogError("MusicManager not found in the scene.");
            }

            optionButton.onClick.AddListener(() => {
                volumeSlider.value = musicManager.GetVolume();
            });

            volumeSlider.value = musicManager.GetVolume();
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        void Start()
        {
            gamePauseUI.SetActive(false);
            gamePauseOptionUI.SetActive(false);
            gameOverUI.SetActive(false);
            winMessage.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    CloseMenuScreen();
                }
                else
                {
                    StartMenuScreen();
                }
            }
        }

        void StartMenuScreen()
        {
            isPaused = true;

            gamePauseUI.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CloseMenuScreen()
        {
            isPaused = false;

            gamePauseUI.SetActive(false);
            gamePauseOptionUI.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void StartGameOverScreen()
        {
            gamePauseUI.SetActive(false);
            gamePauseOptionUI.SetActive(false);
            gameOverUI.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void StartWinMessage()
        {
            winMessage.SetActive(true);
        }

        public void RestartToFirstScene()
        {
            SceneManager.LoadScene(0);
            //TODO: Fix Networking
        }

        public void SetVolume(float volume) {
            if (musicManager != null) {
                musicManager.ChangeVolume(volume);
            }
        }

        public bool GetIsPaused() {return isPaused;}
    }
}