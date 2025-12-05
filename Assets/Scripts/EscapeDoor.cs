using Assets.Scripts.Player;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class EscapeDoor : MonoBehaviour
    {
        public void OpenDoors()
        {
            transform.SetLocalPositionAndRotation(new Vector3(24.6f, 0.413f, -19.7f), Quaternion.Euler(-85, -90, 0));
        }

        [SerializeField] private float repairSpeedPerPlayer = 2f; // Fortschritt pro Sekunde und Spieler
        [SerializeField] private float maxProgress = 100f;
        [SerializeField] private float decaySpeed = 1f;

        public NetworkVariable<float> Progress = new NetworkVariable<float>(0f);
        public NetworkVariable<int> RepairingPlayers = new NetworkVariable<int>(0);
        public NetworkVariable<bool> IsOpened = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> IsEnabled = new NetworkVariable<bool>(false);


        [SerializeField] private Canvas progressBarCanvas; // Fortschritts-Canvas
        [SerializeField] private float activationDistance = 5f; // Sichtbarkeitsradius

        private Slider progressBarSlider;


        private void Start()
        {
            // Slider innerhalb des Canvas finden
            progressBarSlider = progressBarCanvas.GetComponentInChildren<Slider>();

            progressBarCanvas.gameObject.SetActive(false);
        }


        private void Update()
        {
            if (RepairingPlayers.Value > 0 && !IsOpened.Value && IsEnabled.Value)
            {
                Progress.Value += repairSpeedPerPlayer * Time.deltaTime;

                if (Progress.Value >= maxProgress)
                {
                    Progress.Value = maxProgress;
                    IsOpened.Value = true;
                    OpenDoors();
                }
            }

            if (Progress.Value > 0 && RepairingPlayers.Value == 0 && !IsOpened.Value) //progress r�ckg�ngig machen
            {
                DecayProgress();
            }
            if (Progress.Value <= 0 && RepairingPlayers.Value == 0)
            {
                SetCanvasVisibilityClientRpc(false);
            }
        }

        
        public void SetCanOpenTrueServerRpc(ServerRpcParams rpcParams = default)
        {
            IsEnabled.Value = true;
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

        
        public void StartOpeningServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsOpened.Value)
            {
                RepairingPlayers.Value++;
                SetCanvasVisibilityClientRpc(true);
                UpdateOpeningPlayersClientRpc(RepairingPlayers.Value);
            }
        }
        
        private void UpdateOpeningPlayersClientRpc(int currentRepairingPlayers)
        {
            RepairingPlayers.Value = currentRepairingPlayers;
        }
        
        public void StopOpeningServerRpc(ServerRpcParams rpcParams = default)
        {
            if (RepairingPlayers.Value > 0)
            {
                RepairingPlayers.Value--;
            }
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


        #region Winning
        //[SerializeField] private List<ulong> playersWhoEscaped = new List<ulong>(); // Liste der Spieler, die entkommen sind

        private void OnTriggerEnter(Collider other)
        {
            //if (!IsServer) return; // Nur der Server verarbeitet den Trigger

            // Prüfe, ob das Objekt ein SurvivorFPSController hat
            SurvivorFPSController survivor = other.GetComponent<SurvivorFPSController>();
            if (survivor != null)
            {
                survivor.PlayerWon();
            }
        }

        
        private void NotifyPlayerEscapeClientRpc(ulong playerId)
        {
            Debug.Log($"Spieler {playerId} hat die Flucht geschafft (Client-seitig sichtbar).");
            // Hier kannst du z. B. UI-Anpassungen vornehmen oder andere Aktionen auslösen.
        }
        #endregion Winning
    }
}