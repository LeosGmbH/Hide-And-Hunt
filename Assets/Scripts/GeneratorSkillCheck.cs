using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class GeneratorSkillCheck : MonoBehaviour
    {
        [SerializeField] private Slider progressSlider; // Der Fortschrittsbalken
        private bool isBeingRepaired = false;
        private float progress = 0;
        [SerializeField] private Image skillbar;
        [SerializeField] private RectTransform skillcheck;
        [SerializeField] private RectTransform skillpointer;
        private bool skillcheckActive;
        [SerializeField] private float startpos = -140f; // Zielposition
        [SerializeField] private float targetX = 140f; // Zielposition
        [SerializeField] private float duration = 2f;
        private Vector2 resetPosCheck;
        private void Start()
        {
            resetPosCheck = new Vector2(skillcheck.anchoredPosition.x, skillcheck.anchoredPosition.y);
            skillcheckActive = false;
        }

        public void StopRepair()
        {
            if (skillcheckActive)
            {
                progressSlider.value -= 0.15f;
                Debug.Log("Fortschirtt verloren");
            }
            isBeingRepaired = false;
            progressSlider.gameObject.SetActive(false);
            skillbar.gameObject.SetActive(false);
            skillcheck.gameObject.SetActive(false);
            skillpointer.gameObject.SetActive(false);
        }

        public void StartRepair()
        {
            Debug.Log("generator started");
            if (!isBeingRepaired)
            {
                progressSlider.gameObject.SetActive(true);
                progressSlider.value = progress;
                isBeingRepaired = true;
                StartCoroutine(RepairGenerator());
                StartCoroutine(WaitForSkillCheck());
            }
        }

        IEnumerator RepairGenerator()
        {
            while (progressSlider.value < 1 && isBeingRepaired)
            {
                progressSlider.value += Time.deltaTime * 0.02f; // Erhöhe den Fortschritt
                progress = progressSlider.value;
                yield return null;
            }
        }
        private IEnumerator WaitForSkillCheck()
        {
            float waitTime = Random.Range(1f, 6f);
            yield return new WaitForSeconds(waitTime);
            if (isBeingRepaired)
            {
                PerformSkillCheck();
            }
        }
        void PerformSkillCheck()
        {
            skillbar.gameObject.SetActive(true);
            skillcheck.gameObject.SetActive(true);
            skillpointer.gameObject.SetActive(true);
            float pos = Mathf.Round(Random.Range(-70f, 100f) / 10f) * 10f;
            skillcheck.anchoredPosition = new Vector2(skillcheck.anchoredPosition.x + pos, skillcheck.anchoredPosition.y);
            StartCoroutine(HandleSkillCheck());
        }

        private IEnumerator HandleSkillCheck()
        {
            float startX = skillpointer.anchoredPosition.x; // Startposition
            float time = 0f;
            bool failedSkillcheck = true;

            while (time < duration)
            {
                skillcheckActive = true;
                time += Time.deltaTime; // Zeit erhöhen
                float t = Mathf.Clamp01(time / duration); // Normalisieren der Zeit
                skillpointer.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, t), skillpointer.anchoredPosition.y); // Bewege das Image

                if (Input.GetKey(KeyCode.Space))
                {
                    if (skillpointer.anchoredPosition.x > skillcheck.anchoredPosition.x - 35 && skillpointer.anchoredPosition.x < skillcheck.anchoredPosition.x + 35)
                    {
                        Debug.Log("Success");
                        failedSkillcheck = false;
                    }
                    else
                    {
                        Debug.Log("Failed");
                        progressSlider.value -= 0.15f;
                    }
                    break;
                }

                yield return null; // Warten bis zum nächsten Frame
            }
            if (failedSkillcheck)
            {
                Debug.Log("Failed");
                progressSlider.value -= 0.15f;
            }
            skillcheckActive = false;

            skillpointer.anchoredPosition = new Vector2(startpos, skillpointer.anchoredPosition.y);
            skillcheck.anchoredPosition = resetPosCheck;

            // Stelle sicher, dass die Endposition erreicht wird
            skillbar.gameObject.SetActive(false);
            skillcheck.gameObject.SetActive(false);
            skillpointer.gameObject.SetActive(false);
            StartCoroutine(WaitForSkillCheck());
        }
    }
}