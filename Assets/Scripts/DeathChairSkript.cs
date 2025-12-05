using Assets.Scripts.Player;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class DeathChairSkript : MonoBehaviour
    {
        [SerializeField] private float dieSpeed = 20f; // Fortschritt pro Sekunde und Spieler
        [SerializeField] private float maxProgress = 100f;

        public NetworkVariable<float> Progress = new NetworkVariable<float>(0f);
        public NetworkVariable<bool> DieingPlayer = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(false);


        [SerializeField] private Canvas progressBarCanvas; // Fortschritts-Canvas

        private Slider progressBarSlider;
        private NetworkObject DieingPlayerNObject;

        private void Start()
        {
            // Slider innerhalb des Canvas finden
            progressBarSlider = progressBarCanvas.GetComponentInChildren<Slider>();

            progressBarCanvas.gameObject.SetActive(false);
        }


        private void Update()
        {
            if ( DieingPlayer.Value && !IsDead.Value)
            {
                Progress.Value += dieSpeed * Time.deltaTime;

                if (Progress.Value >= maxProgress)
                {
                    Progress.Value = maxProgress;
                    OnPlayerDied();
                    IsDead.Value = true;
                }
            }

            if ( Progress.Value > 0 && !DieingPlayer.Value && !IsDead.Value) //progress r�ckg�ngig machen
            {
                Progress.Value = 0;
                SetCanvasVisibilityClientRpc(false);
            }

        }
        
        private void SetCanvasVisibilityClientRpc(bool isVisible)
        {
            progressBarCanvas.gameObject.SetActive(isVisible);
        }


        
        public void StartDieingServerRpc(ulong survivorNetworkId, ServerRpcParams rpcParams = default)
        {
            if (!IsDead.Value)
            {
                DieingPlayerNObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[survivorNetworkId];
                DieingPlayer.Value = true;
                SetCanvasVisibilityClientRpc(true);
                UpdateDieingPlayersClientRpc(DieingPlayer.Value);
            }
        }
        
        private void UpdateDieingPlayersClientRpc(bool isplayerOnChair)
        {
            DieingPlayer.Value = isplayerOnChair;
        }
        
        public void ResetDeathChairServerRpc()
        {
            ResetDeathChairClientRpc();
            SetCanvasVisibilityClientRpc(false);
        }
        
        public void ResetDeathChairClientRpc()
        {
            IsDead.Value = false;
            DieingPlayer.Value = false;
            Progress.Value = 0;
            DieingPlayerNObject = null;
            UpdateDieingPlayersClientRpc(DieingPlayer.Value);
        }
        public void ResetDeathChair()
        {
            IsDead.Value = false;
            DieingPlayer.Value = false;
            Progress.Value = 0;
            DieingPlayerNObject = null;
            UpdateDieingPlayersClientRpc(DieingPlayer.Value);
        }

        private void OnPlayerDied()
        {
            DieingPlayerNObject.GetComponentInChildren<PlayerStateManager>().Die();
            ResetDeathChairServerRpc();
            ResetDeathChair();
            Debug.Log("Player Died!");
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