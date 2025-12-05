using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class GeneratorSkript : MonoBehaviour
    {
        [SerializeField] private float repairSpeedPerPlayer = 5f; // Fortschritt pro Sekunde und Spieler
        [SerializeField] private float maxProgress = 100f;
        [SerializeField] private float decaySpeed = 2f;

        public NetworkVariable<float> Progress = new NetworkVariable<float>(0f);
        public NetworkVariable<int> RepairingPlayers = new NetworkVariable<int>(0);
        public NetworkVariable<bool> IsRepaired = new NetworkVariable<bool>(false);


        [SerializeField] private Canvas progressBarCanvas; // Fortschritts-Canvas
        [SerializeField] private float activationDistance = 5f; // Sichtbarkeitsradius
        [SerializeField] private TasksManager tasksManager;

        private Slider progressBarSlider;
        public AudioSource startGeneratorSound;

        private void Start()
        {
            // Slider innerhalb des Canvas finden
            progressBarSlider = progressBarCanvas.GetComponentInChildren<Slider>();

            progressBarCanvas.gameObject.SetActive(false);
        }


        private void Update()
        {
            if ( RepairingPlayers.Value > 0 && !IsRepaired.Value)
            {
                Progress.Value += repairSpeedPerPlayer * RepairingPlayers.Value * Time.deltaTime;

                if (Progress.Value >= maxProgress)
                {
                    Progress.Value = maxProgress;
                    IsRepaired.Value = true;
                    OnRepaired();
                }
            }

            if ( Progress.Value > 0 && RepairingPlayers.Value == 0 && !IsRepaired.Value) //progress r�ckg�ngig machen
            {
                DecayProgress();
            }
            if (Progress.Value <= 0 && RepairingPlayers.Value == 0)
            {
                SetCanvasVisibilityClientRpc(false);
                if (startGeneratorSound.isPlaying)
                {
                    startGeneratorSound.Stop();
                }

            }
        }
        
        private void SetCanvasVisibilityClientRpc(bool isVisible)
        {
            progressBarCanvas.gameObject.SetActive(isVisible);
        }
        private void DecayProgress()
        {
            Progress.Value -= decaySpeed * Time.deltaTime;

            if (Progress.Value < 0)
            {
                Progress.Value = 0;
            }
        }

        
        public void StartRepairingServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsRepaired.Value)
            {
                if (!startGeneratorSound.isPlaying)
                {
                    startGeneratorSound.Play();
                }
                RepairingPlayers.Value++;
                SetCanvasVisibilityClientRpc(true);
                UpdateRepairingPlayersClientRpc(RepairingPlayers.Value);
            }
        }
        
        private void UpdateRepairingPlayersClientRpc(int currentRepairingPlayers)
        {
            RepairingPlayers.Value = currentRepairingPlayers;
        }
        
        public void StopRepairingServerRpc(ServerRpcParams rpcParams = default)
        {
            if (RepairingPlayers.Value > 0)
            {
                RepairingPlayers.Value--;
            }
        }

        private void OnRepaired()
        {
            tasksManager.RegisterGeneratorRepairedServerRpc();
            Debug.Log("Generator wurde repariert!");
        }

        
        private void UpdateProgressBarClientRpc(float progress)
        {
            if (progressBarSlider != null)
            {
                progressBarSlider.value = progress / 100f; // Normalisierten Fortschritt setzen
            }
        }

        private void OnEnable()
        {
            Progress.OnValueChanged += (oldValue, newValue) =>
            {
                UpdateProgressBarClientRpc(newValue);
            };
        }


        private void OnDisable()
        {
            Progress.OnValueChanged -= (oldValue, newValue) =>
            {
                UpdateProgressBarClientRpc(newValue);
            };
        }




    }
}