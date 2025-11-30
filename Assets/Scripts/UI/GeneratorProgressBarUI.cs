using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class GeneratorProgressBarUI : MonoBehaviour
    {
        private Slider generatorProgressBar;
        public TasksManager tasksManager;
        public TextMeshProUGUI progressText;
        public AudioSource successSound;
        public Image fillArea;

        void Start()
        {
            generatorProgressBar = GetComponent<Slider>();
            UpdateGeneratorUI(0);
        }

        public void UpdateGeneratorUI(int count)
        {
            generatorProgressBar.value = count;
            progressText.text = $"Generators: {count}/5";
            UpdateFillColor(count);
        }
        private void UpdateFillColor(int count)
        {
            switch (count)
            {
                case 0: fillArea.color = new Color(0.4f, 0.0f, 0.0f); break;
                case 1: fillArea.color = new Color(0.6f, 0.2f, 0.0f); 
                        fillArea.maskable = false; break;
                case 2: fillArea.color = new Color(0.8f, 0.4f, 0.0f); 
                        fillArea.maskable = true; break;
                case 3: fillArea.color = new Color(0.6f, 0.6f, 0.0f); break;
                case 4: fillArea.color = new Color(0.2f, 0.8f, 0.0f); break;
                case 5: fillArea.color = new Color(0.0f, 0.6f, 0.0f); break;
                default: Debug.LogError("Invalid progress bar value"); break;
            }
        }
        public void PlaySuccessSound()
        {
            if (generatorProgressBar.value > 0) successSound.Play();
        }
    }
}