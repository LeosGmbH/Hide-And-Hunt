using Cinemachine;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class KillerFPSController : NetworkBehaviour
    {
        public static void ResetStaticData()
        {
            OnAnyPlayerSpawned = null;
        }

        [Header("Settings------------------------------------------------------------------------")]
        [SerializeField] private List<Vector3> spawnPositionList;
        private KillerStateManager killerStateManager;
        public static KillerFPSController LocalInstance { get; private set; }

        public static event EventHandler OnAnyPlayerSpawned;
        private CharacterController characterController;
        private Animator killerAnim;
        [SerializeField] private GameObject checkHitCollider;
        [SerializeField] private GameObject grabCollider;
        [SerializeField] private InGameMenuScreen menuScreen;

        [Header("Camera Settings-----------------------------------------------------------------")]
        [SerializeField] private GameObject tpCamera;
        [SerializeField] private GameObject freeLookCamera;
        private string originalXAxisName;
        private string originalYAxisName;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float lookSpeed = 2f;
        [SerializeField] private float lookXLimit = 45f;
        [SerializeField] private float camerarotation; //third person

        [Header("Movement Parameters--------------------------------------------------------------")]
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float runSpeed = 12f;
        [SerializeField] private float jumpPower = 1f;
        [SerializeField] private float gravity = 10f;
        [SerializeField] private bool canMove = true;

        private Vector3 moveDirection = Vector3.zero;
        private float rotationX = 0;
        private ulong survivorNetworkId;


        private void Start()
        {
            if (IsOwner)
            {
                GetComponentInChildren<Light>().enabled = false;
            }
            // PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
            if (killerStateManager == null)
            {
                killerStateManager = GetComponent<KillerStateManager>();
            }
            killerAnim = GetComponentInChildren<Animator>();
            characterController = GetComponent<CharacterController>();

            if (!IsLocalPlayer)
            {
                tpCamera.SetActive(false);
                freeLookCamera.SetActive(false);
                playerCamera.gameObject.SetActive(false);
                return;
            }

            if (IsLocalPlayer)
            {
                menuScreen = FindObjectOfType<InGameMenuScreen>();
            }

            if (freeLookCamera != null)
            {
                originalXAxisName = freeLookCamera.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName;
                originalYAxisName = freeLookCamera.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName;
            }
            else
            {
                Debug.LogError("Bitte eine CinemachineFreeLook-Kamera zuweisen!");
            }

            GameObject winnerWall = GameObject.FindGameObjectWithTag("WinnerWall");
            winnerWall.GetComponent<BoxCollider>().enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                LocalInstance = this;
            }
            // transform.position = spawnPositionList[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];
            OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);
        }

        private void Update()
        {
            if (!IsOwner)
            {
                tpCamera.GetComponent<Camera>().enabled = false;
                playerCamera.GetComponent<Camera>().enabled = false;
                return;
            }

            if (killerStateManager.currentKillerState == KillerState.Carry && !menuScreen.GetIsPaused())
            {
                // Aktiviert die Maussteuerung
                freeLookCamera.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName = originalXAxisName;
                freeLookCamera.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName = originalYAxisName;
            }
            else
            {
                // Deaktiviert die Maussteuerung
                freeLookCamera.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName = "";
                freeLookCamera.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName = "";

                // Fixiert die Achsen, um Bewegung zu stoppen
                freeLookCamera.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisValue = 0;
                freeLookCamera.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisValue = 0;
            }

            HandleMovementAndRotation();
        }
        public NetworkObject GetNetworkObject()
        {
            return NetworkObject;
        }

        private void HandleMovementAndRotation()
        {
            #region Movement Handling

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            //Left Shift for running
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
            float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
            float movementDirectionY = moveDirection.y;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            #endregion Movement Handling

            #region Animation

            const float epsilon = 0.0001f; // Toleranzwert
            if (curSpeedX > -epsilon && curSpeedX < epsilon && curSpeedY > -epsilon && curSpeedY < epsilon)
            {
                killerAnim.SetBool("walk", false); // Animation deaktivieren, wenn der Spieler steht
            }
            else
            {
                killerAnim.SetBool("walk", true); // Animation aktivieren
            }
            if (isRunning)
            {
                killerAnim.SetBool("run", true); // Animation aktivieren
            }
            else
            {
                killerAnim.SetBool("run", false); // Animation deaktivieren, wenn der Spieler steht
            }
            //attack
            #endregion Animation
            #region Actions
            if (killerStateManager.currentKillerState != KillerState.Carry && Input.GetMouseButtonDown(0))
            {
                killerAnim.SetTrigger("attack");
                CheckForSurvivor(true);
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (killerStateManager.currentKillerState == KillerState.Carry)
                {
                    InteractWithSurvivorServerRpc(survivorNetworkId, CheckforDeathChair());
                    killerStateManager.SetState(KillerState.Normal);
                    survivorNetworkId = 0;
                }
                CheckForSurvivor(false);
            }


            #endregion Actions
            #region Movement Jumping
            if (killerStateManager.currentHealthState == KillerHealthState.Healthy || killerStateManager.currentHealthState == KillerHealthState.Injured)
            {
                if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
                {
                    moveDirection.y = jumpPower;
                }
                else
                {
                    moveDirection.y = movementDirectionY;
                }
                if (!characterController.isGrounded)
                {
                    moveDirection.y -= gravity * Time.deltaTime;
                }
            }
            #endregion Movement Jumping
            #region Handles Rotation
            characterController.Move(moveDirection * Time.deltaTime);
            if (killerStateManager.currentKillerState != KillerState.Carry && !menuScreen.GetIsPaused())
            {
                rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
                rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

            }
            else
            {
                Vector3 direction = tpCamera.transform.forward;
                direction.y = 0f; // Keine Vertikale Rotation
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, camerarotation * Time.deltaTime);
            }
            #endregion Handles Rotation
        }

        private ulong CheckforDeathChair()
        {
            if (!IsOwner) return 0;

            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 5) && hit.collider.CompareTag("deathChair"))
            {
                return hit.collider.GetComponent<NetworkObject>().NetworkObjectId;
            }
            return 0;
        }
        private void CheckForSurvivor(bool mode)
        {
            if (!IsOwner) return;

            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 5) && hit.collider.CompareTag("survivor"))
            {
                Debug.Log("Survivor getroffen: " + hit.collider.name);
                // Hole das NetworkObject des getroffenen Survivors
                NetworkObject survivorNetworkObject = hit.collider.GetComponentInParent<NetworkObject>();
                if (survivorNetworkObject != null)
                {
                    Debug.Log("Survivor network object gefunden");
                    survivorNetworkId = survivorNetworkObject.NetworkObjectId;

                    if (mode)
                    {
                        AttackSurvivorServerRpc(survivorNetworkId);
                    }
                    else
                    {
                        PlayerStateManager survivorStateManager = NetworkManager.Singleton.SpawnManager.SpawnedObjects[survivorNetworkId].GetComponentInChildren<PlayerStateManager>();
                        if (survivorStateManager.currentHealthState == HealthState.Down && killerStateManager.currentKillerState == KillerState.Normal)
                        {
                            InteractWithSurvivorServerRpc(survivorNetworkId, 0);
                            killerStateManager.SetState(KillerState.Carry);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Kein Treffer.");
            }
        }
        [ServerRpc]
        private void AttackSurvivorServerRpc(ulong survivorNetworkId)
        {
            AttackSurvivorClientRpc(survivorNetworkId);
        }
        [ClientRpc]
        private void AttackSurvivorClientRpc(ulong survivorNetworkId)
        {
            NetworkObject survivorObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[survivorNetworkId];
            PlayerStateManager survivorStateManager = survivorObject.GetComponentInChildren<PlayerStateManager>();

            if (survivorStateManager != null)
            {
                Debug.Log("Takdamage ausgef�hrt in killer skript");
                survivorStateManager.TakeDamage();
            }
        }

        [ServerRpc]
        private void InteractWithSurvivorServerRpc(ulong survivorNetworkId, ulong deathChairNetworkId)
        {
            InteractWithSurvivorClientRpc(survivorNetworkId, deathChairNetworkId);
        }
        [ClientRpc]
        private void InteractWithSurvivorClientRpc(ulong survivorNetworkId, ulong deathChairNetworkId)
        {
            NetworkObject survivorObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[survivorNetworkId];
            PlayerStateManager survivorStateManager = survivorObject.GetComponentInChildren<PlayerStateManager>();

            if (survivorStateManager != null)
            {
                Debug.Log("Takdamage ausgef�hrt in killer skript");
                if (deathChairNetworkId == 0)
                {
                    survivorStateManager.gotGrabbed();
                }
                else
                {
                    NetworkObject deathChairObjekt = NetworkManager.Singleton.SpawnManager.SpawnedObjects[deathChairNetworkId];
                    if (!deathChairObjekt.GetComponent<DeathChairSkript>().DieingPlayer.Value)
                    {
                        survivorStateManager.goInDeathChair(survivorNetworkId, deathChairNetworkId);
                    }
                }
            }
        }
    }
}
