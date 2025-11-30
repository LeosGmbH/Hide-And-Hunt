using Cinemachine;
using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections;

namespace Assets.Scripts.Player
{
    public enum KillerState
    {
        Normal,
        Carry,
    }
    public enum KillerHealthState
    {
        Healthy,
        Injured
    }

    public class KillerStateManager : NetworkBehaviour
    {
        //health state
        public KillerHealthState currentHealthState = KillerHealthState.Healthy;
        public delegate void HealthStateChanged(KillerHealthState newState); // Event f�r Status�nderungen
        public event HealthStateChanged OnHealthStateChanged;
        private KillerFPSController killerFPScontroller;


        public KillerState currentKillerState { get; private set; } = KillerState.Normal;
        [SerializeField] private GameObject tpCamera;
        [SerializeField] private GameObject freeLookCamera;
        [SerializeField] private GameObject mainCamera;

        // Optional: Events f�r State-Wechsel
        public static event Action<KillerState> OnStateChanged;
        private void Awake()
        {
            ApllyChanges();
        }
        private void Start()
        {
            killerFPScontroller = GetComponent<KillerFPSController>();
        }
        public void SetState(KillerState newState)
        {
            if (currentKillerState != newState)
            {
                currentKillerState = newState;
                OnStateChanged?.Invoke(currentKillerState); // Event ausl�sen, falls gew�nscht
                ApllyChanges();
            }
        }

        public void ApllyChanges()
        {
            if (currentKillerState == KillerState.Carry)    // mainCamera --> tpCamera
            {
                tpCamera.GetComponent<Camera>().enabled = true;
                tpCamera.GetComponent<AudioListener>().enabled = true;

                mainCamera.GetComponent<Camera>().enabled = false;
                mainCamera.GetComponent<AudioListener>().enabled = false;

                freeLookCamera.GetComponent<CinemachineFreeLook>().enabled = true;
                mainCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;
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

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Respawn"))  // �berpr�fen, ob der andere Collider den "Weapon"-Tag hat
            {
                SetState(KillerState.Carry);
            }
            if (other.gameObject.CompareTag("Finish"))  // �berpr�fen, ob der andere Collider den "Weapon"-Tag hat
            {
                SetState(KillerState.Normal);
            }
        }
        public void isCarrying(bool iscarrying)
        {
            if (iscarrying)
            {
                SetState(KillerState.Carry);
            }
            else
            {
                SetState(KillerState.Normal);
            }
        }
    }
}