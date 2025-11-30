using Cinemachine;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public enum PlayerState
    {
        Human,
        Prop,
    }
    public enum HealthState
    {
        Healthy,
        Injured,
        Down,
        OnKiller,
        DeathChair,
        Dead
    }

    public class PlayerStateManager : MonoBehaviour
    {
        [SerializeField] public PlayerState currentPlayerState = PlayerState.Human;
        [SerializeField] private GameObject tpCamera;
        [SerializeField] private GameObject freeLookCamera;
        [SerializeField] private GameObject mainCamera;
        [SerializeField] private Collider propCollider;

        private bool isRepairing = false;

        public void setIsRepairing(bool mode)
        {
            isRepairing = mode;
        }
        public bool getIsRepairing()
        {
            return isRepairing;
        }

        // Optional: Events f�r State-Wechsel
        public static event Action<PlayerState> OnStateChanged;
        private void Awake()
        {
            ApllyChanges();
        }
        public void SetState(PlayerState newState)
        {
            if (currentPlayerState != newState)
            {
                currentPlayerState = newState;
                OnStateChanged?.Invoke(currentPlayerState); // Event ausl�sen, falls gew�nscht
                ApllyChanges();
            }
        }

        public void ApllyChanges()
        {
            if (currentPlayerState == PlayerState.Prop)     // mainCamera --> tpCamera
            {
                tpCamera.GetComponent<Camera>().enabled = true;
                tpCamera.GetComponent<AudioListener>().enabled = true;

                mainCamera.GetComponent<Camera>().enabled = false;
                mainCamera.GetComponent<AudioListener>().enabled = false;

                freeLookCamera.GetComponent<CinemachineFreeLook>().enabled = true;
                mainCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;

                //freeLookCamera.GetComponent<CinemachineFreeLook>().m_XAxis.Value = transform.localRotation.eulerAngles.x;
                Debug.Log("transformed Update");
            }
            else                                            // tpCamera --> mainCamera
            {
                Debug.Log("normal Update");

                transform.rotation = new(transform.rotation.x, tpCamera.transform.rotation.y, transform.rotation.z, transform.rotation.w);

                mainCamera.GetComponent<CinemachineVirtualCamera>().enabled = true;
                freeLookCamera.GetComponent<CinemachineFreeLook>().enabled = false;

                StartCoroutine(EnableMainCameraAfterDelay());   // Die Coroutine wartet 0.3 Sekunden
            }
        }

        private IEnumerator EnableMainCameraAfterDelay()        // Die Coroutine wartet 0.3 Sekunden
        {
            yield return new WaitForSeconds(0.3f); // Warte 0.3 Sekunden

            mainCamera.GetComponent<Camera>().enabled = true;
            mainCamera.GetComponent<AudioListener>().enabled = true;

            tpCamera.GetComponent<Camera>().enabled = false;
            tpCamera.GetComponent<AudioListener>().enabled = false;
        }









        //health state
        public HealthState currentHealthState = HealthState.Healthy;
        public delegate void HealthStateChanged(HealthState newState); // Event f�r Status�nderungen
        public event HealthStateChanged OnHealthStateChanged;
        private SurvivorFPSController survivorFPSController;
        private PlayerPropManager playerPropManager;
        private void Start()
        {
            survivorFPSController = GetComponent<SurvivorFPSController>();
            playerPropManager = GetComponent<PlayerPropManager>();
        }

        private bool canTakeDamage = true; // Flag, ob Schaden genommen werden darf
        private float damageCooldown = 0.5f; // Cooldown-Zeit (falls n�tig)

        public void gotGrabbed()
        {
            if (currentHealthState == HealthState.Down)
            {
                currentHealthState = HealthState.OnKiller;
                survivorFPSController.HealthStateChanged();
            }
            else if (currentHealthState == HealthState.OnKiller)
            {
                currentHealthState = HealthState.Down;
                survivorFPSController.HealthStateChanged();
            }
        }
        public void goInDeathChair(ulong survivorNetworkId, ulong deathChairNetworkId)
        {
            if (currentHealthState == HealthState.OnKiller)
            {
                NetworkObject deathChairObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[deathChairNetworkId];
                survivorFPSController.setDeathChairGameObject(deathChairObj.gameObject);
                currentHealthState = HealthState.DeathChair;
                deathChairObj.GetComponent<DeathChairSkript>().StartDieingServerRpc(survivorNetworkId);
                survivorFPSController.HealthStateChanged();

            }
        }
        // ** Schaden nehmen **
        public void TakeDamage()
        {
            Debug.Log("Tagedamage bei survivor ausgef�hrt");
            if (!canTakeDamage) return; // Abbrechen, wenn Schaden gesperrt ist
            canTakeDamage = false;
            if (currentHealthState == HealthState.Healthy)
            {
                currentHealthState = HealthState.Injured;
                //spieler humpelt anim
            }
            else if (currentHealthState == HealthState.Injured)
            {
                currentHealthState = HealthState.Down;
                playerPropManager.TransformPlayerIntoPlayerServerRpc();
                PlayerDown();
            }
            OnHealthStateChanged?.Invoke(currentHealthState);
            if (currentHealthState == HealthState.Dead)
            {
                // Spieler stirbt
            }
            survivorFPSController.HealthStateChanged();
            Debug.Log(currentHealthState + " spieler hat schaden genommen");
            Invoke(nameof(ResetDamageFlag), damageCooldown);
        }

        private void PlayerDown()
        {
            Debug.Log(" Downcamerachange");
            Vector3 localPosition = mainCamera.transform.localPosition;
            localPosition.y = 1;
            localPosition.z = -1;
            mainCamera.transform.localPosition = localPosition;

        }

        // ** Heilen **
        public void Heal()
        {
            if (currentHealthState == HealthState.Injured)
            {
                currentHealthState = HealthState.Healthy; // Von verletzt zu gesund
            }
            else if (currentHealthState == HealthState.Down|| currentHealthState == HealthState.DeathChair)
            {
                currentHealthState = HealthState.Injured; 
                Vector3 localPosition = mainCamera.transform.localPosition; 
                localPosition.y = 1.656f; 
                localPosition.z = 0.191f;
                mainCamera.transform.localPosition = localPosition; 
            }
            survivorFPSController.HealthStateChanged();

        }
        public void Die()
        {
            if (currentHealthState == HealthState.DeathChair)
            {
                currentHealthState = HealthState.Dead;
                survivorFPSController.HealthStateChanged();
            }
        }

        private void ResetDamageFlag()
        {
            canTakeDamage = true; // Schaden wieder erlauben
        }
    }
}