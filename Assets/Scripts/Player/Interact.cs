using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class Interact : NetworkBehaviour
    {
        private ulong survivorNetworkId;
        [SerializeField] private Camera playerCamera;
        void Start()
        {
            survivorNetworkId = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                CheckForOtherSurvivor();
                if (survivorNetworkId != 0)
                {
                    Debug.Log("survivor Gefunden");
                    PlayerStateManager survivorStateManager = NetworkManager.Singleton.SpawnManager.SpawnedObjects[survivorNetworkId].GetComponentInChildren<PlayerStateManager>();
                    if (survivorStateManager.currentHealthState == HealthState.DeathChair || survivorStateManager.currentHealthState == HealthState.Down || survivorStateManager.currentHealthState == HealthState.Injured)
                    {
                        HelpSurvivorServerRpc(survivorNetworkId);
                    }
                    survivorNetworkId = 0;
                }
            }
        }

        private void CheckForOtherSurvivor()
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 5) && hit.collider.CompareTag("survivor"))
            {
                Debug.Log("Survivor getroffen: " + hit.collider.name);
                // Hole das NetworkObject des getroffenen Survivors
                NetworkObject survivorNetworkObject = hit.collider.GetComponentInParent<NetworkObject>();
                if (survivorNetworkObject != null)
                {
                    Debug.Log("Survivor network object gefunden");
                    survivorNetworkId = survivorNetworkObject.NetworkObjectId;

                }
                else
                {
                    Debug.Log("Kein Survivor network object gefunden");
                }
            }
            else
            {
                survivorNetworkId = 0;
                Debug.Log("Kein Treffer.");
            }
        }
        [ServerRpc(RequireOwnership = false)]
        private void HelpSurvivorServerRpc(ulong survivorNetworkId)
        {
            SafeSurvivorClientRpc(survivorNetworkId);
        }
        [ClientRpc]
        private void SafeSurvivorClientRpc(ulong survivorNetworkId)
        {
            NetworkObject survivorObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[survivorNetworkId];
            PlayerStateManager survivorStateManager = survivorObject.GetComponentInChildren<PlayerStateManager>();

            if (survivorStateManager != null)
            {
                Debug.Log("Heal client rpc in interact ausgeführt");
                survivorStateManager.Heal();
            }
        }

    }
}