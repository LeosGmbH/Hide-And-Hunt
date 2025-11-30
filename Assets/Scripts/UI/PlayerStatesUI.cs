using Assets.Scripts.Player;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class PlayerStatesUI : MonoBehaviour
    {
        public TextMeshProUGUI killerHeadline;
        public TextMeshProUGUI killerNameText;
        public TextMeshProUGUI[] survivorNameTexts;
        public TextMeshProUGUI[] survivorStateTexts;
        private GameObject[] survivors;

        void Awake()
        {
            killerHeadline.enabled = false;
            killerNameText.enabled = false;
            foreach (var x in survivorNameTexts) x.enabled = false;
            foreach (var x in survivorStateTexts) x.enabled = false;
        }

        void Start()
        {
            StartCoroutine(InitializePlayerStates());
        }

        private IEnumerator InitializePlayerStates()
        {
            yield return new WaitForSeconds(2.5f);
            UpdatePlayerListClientRpc();
        }

        [ClientRpc]
        private void UpdatePlayerListClientRpc()
        {
            // Finde alle Spieler und aktualisiere die UI
            survivors = GameObject.FindGameObjectsWithTag("survivor");

            // Setze vorherige UI-Einträge zurück
            foreach (var text in survivorNameTexts) text.enabled = false;
            foreach (var text in survivorStateTexts) text.enabled = false;

            int anz = 0;
            foreach (GameObject survivor in survivors)
            {
                NetworkObject networkObject = survivor.GetComponentInParent<NetworkObject>();
                if (networkObject != null)
                {
                    PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex((int)networkObject.OwnerClientId);
                    survivorNameTexts[anz].enabled = true;
                    survivorNameTexts[anz].text = playerData.playerName + "";
                    survivorStateTexts[anz].enabled = true;
                    anz++;
                }
            }

            // Killer-Information aktualisieren
            GameObject killer = GameObject.FindGameObjectWithTag("KillerTextTag");

            killerHeadline.enabled = false;
            killerNameText.enabled = false;

            if (killer != null)
            {
                NetworkObject killerNetworkObject = killer.GetComponentInParent<NetworkObject>();
                if (killerNetworkObject != null)
                {
                    PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex((int)killerNetworkObject.OwnerClientId);
                    killerHeadline.enabled = true;
                    killerNameText.enabled = true;
                    killerNameText.text = playerData.playerName + "";
                }
            }
        }

        void Update()
        {
            UpdatePlayerListClientRpc();
            UpdateHealthTextClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateHealthTextServerRpc()
        {
            UpdateHealthTextClientRpc();  // Auf allen Clients ausführen
        }

        [ClientRpc]
        private void UpdateHealthTextClientRpc()
        {
            if (survivors != null)
            {
                for (int i = 0; i < survivors.Length; i++)
                {
                    if (survivors[i] != null)
                    {
                        PlayerStateManager playerStateManager = survivors[i].GetComponent<PlayerStateManager>();
                        if (playerStateManager != null)
                        {
                            HealthState healthState = playerStateManager.currentHealthState;

                            switch (healthState)
                            {
                                case HealthState.Healthy:
                                    survivorStateTexts[i].text = "Healthy";
                                    survivorStateTexts[i].color = Color.green;
                                    break;

                                case HealthState.Injured:
                                    survivorStateTexts[i].text = "Injured";
                                    survivorStateTexts[i].color = Color.yellow;
                                    break;

                                case HealthState.Down:
                                    survivorStateTexts[i].text = "Down";
                                    survivorStateTexts[i].color = new Color(1f, 0.65f, 0f); // Orange
                                    break;

                                case HealthState.DeathChair:
                                    survivorStateTexts[i].text = "On Death Chair";
                                    survivorStateTexts[i].color = Color.red;
                                    break;

                                case HealthState.Dead:
                                    survivorStateTexts[i].text = "Dead";
                                    survivorStateTexts[i].color = new Color(191f, 0f, 0f);  // Dark red
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
