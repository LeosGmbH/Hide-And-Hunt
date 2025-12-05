using Assets.Scripts.UI;
using Cinemachine;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class SurvivorFPSController : MonoBehaviour
    {
        public static void ResetStaticData()
        {
            OnAnyPlayerSpawned = null;
        }

        [Header("Settings------------------------------------------------------------------------")]
        [SerializeField] private List<Vector3> spawnPositionList;
        private PlayerStateManager playerStateManager;
        private PlayerPropManager playerPropManager;
        public static SurvivorFPSController LocalInstance { get; private set; }

        public static event EventHandler OnAnyPlayerSpawned;
        private CharacterController characterController;
        private Animator playerAnim;
        private GameObject holdSurvivorRoot;
        private GameObject deathChairPos;
        [SerializeField] private InGameMenuScreen menuScreen;

        [Header("Camera Settings-----------------------------------------------------------------")]
        [SerializeField] private GameObject tpCamera;

        [SerializeField] private GameObject freeLookCamera;
        private string originalXAxisName;
        private string originalYAxisName;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float lookSpeed = 2f;
        [SerializeField] private float lookXLimit = 45f;

        [Header("Movement Parameters--------------------------------------------------------------")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float jumpPower = 8f;
        [SerializeField] private float gravity = 10f;
        [SerializeField] private bool canMove = true;

        private Vector3 moveDirection = Vector3.zero;
        private float rotationX = 0;


        private void Start()
        {

                menuScreen=FindObjectOfType<InGameMenuScreen>();
                GameObject[] generators = GameObject.FindGameObjectsWithTag("Generator");
                foreach (GameObject generator in generators)
                {
                    Transform canvasTransform = generator.transform.Find("BillBoardCanvas");
                    if (canvasTransform != null)
                    {
                        BillBoard generatorBillboard = canvasTransform.GetComponent<BillBoard>();
                        if (generatorBillboard != null)
                        {
                            // Setze die Kamera des lokalen Spielers f�r jedes Generator-Billboard
                            generatorBillboard.SetTargetCamera(playerCamera);
                        }
                    }
                }


            holdSurvivorRoot = GameObject.Find("HoldSurvivorRoot");
            // PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
            if (playerStateManager == null)
            {
                playerStateManager = GetComponent<PlayerStateManager>();
            }
            if (playerPropManager == null)
            {
                playerPropManager = GetComponent<PlayerPropManager>();
            }
            playerAnim = GetComponentInChildren<Animator>();
            characterController = GetComponent<CharacterController>();


            if (freeLookCamera != null)
            {
                originalXAxisName = freeLookCamera.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName;
                originalYAxisName = freeLookCamera.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName;
            }
            else
            {
                Debug.LogError("Bitte eine CinemachineFreeLook-Kamera zuweisen!");
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (playerStateManager.currentPlayerState == PlayerState.Prop)
            {
                playerPropManager.HandlePropMovement();
                playerPropManager.HandleTPCameraRootPos();

                if (!menuScreen.GetIsPaused())
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
            }
            else
            {
                HandleMovementAndRotation();
            }
            if (playerStateManager.currentHealthState != HealthState.Down && !playerStateManager.getIsRepairing())
            {
                playerPropManager.HandleTransformation();
            }
            if (playerStateManager.currentHealthState == HealthState.OnKiller)
            {
                transform.localPosition = holdSurvivorRoot.transform.position;
                transform.localRotation = holdSurvivorRoot.transform.rotation;
            }
           
        }
        public void setDeathChairGameObject(GameObject gameObject)
        {
            deathChairPos = gameObject;
        }
        public void clearAnims()
        {
            playerAnim.SetBool("hanging", false);
            playerAnim.SetBool("sitting", false);
            playerAnim.SetBool("down", false);
            playerAnim.SetBool("crawl", false);
            playerAnim.SetBool("walk", false);
            playerAnim.SetBool("run", false);
            playerAnim.SetBool("repair", false);
        }
        public void setAnim(string name, bool mode)
        {
            playerAnim.SetBool(name, mode);
        }

        public void HealthStateChanged()
        {
            clearAnims();
            if (playerStateManager.currentHealthState == HealthState.Down)
            {
                walkSpeed = 0.6f;
                runSpeed = 0.6f;
                jumpPower = 0f;
                playerAnim.SetBool("down", true);
            }
            else if (playerStateManager.currentHealthState == HealthState.DeathChair || playerStateManager.currentHealthState == HealthState.OnKiller)
            {
                if (playerStateManager.currentHealthState == HealthState.DeathChair)
                {
                    playerAnim.SetBool("sitting", true);
                    Vector3 pos = deathChairPos.transform.position;
                    pos.y -= 1.6f;
                    Vector3 eulerAngles = deathChairPos.transform.rotation.eulerAngles;
                    eulerAngles.y -= 90f;
                    transform.localPosition = pos;
                    transform.localRotation = Quaternion.Euler(eulerAngles);

                }
                else if (playerStateManager.currentHealthState == HealthState.OnKiller)
                {
                    playerAnim.SetBool("hanging", true);
                }
                playerStateManager.SetState(PlayerState.Prop);
                walkSpeed = 0f;
                runSpeed = 0f;
                jumpPower = 0f;
            }
            else if (playerStateManager.currentHealthState == HealthState.Dead)
            {
                menuScreen.StartGameOverScreen();
                DestroyPlayerServerRpc();
                Destroy(gameObject);
            }
            else
            {
                walkSpeed = 4f;
                runSpeed = 6f;
                jumpPower = 8f;
            }
        }
        
        public void DestroyPlayerServerRpc()
        {
            DestroyPlayerClientRpc();
            Destroy(gameObject);
        }

        // Client-RPC, um die Transformation zu synchronisieren
        
        private void DestroyPlayerClientRpc()
        {
            Destroy(gameObject);
        }

        public void SetCanMove(bool move)
        {
            canMove = move;
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
            if (playerStateManager.currentHealthState == HealthState.Healthy || playerStateManager.currentHealthState == HealthState.Injured)
            {
                if (curSpeedX > -epsilon && curSpeedX < epsilon && curSpeedY > -epsilon && curSpeedY < epsilon)
                {
                    playerAnim.SetBool("walk", false); // Animation deaktivieren, wenn der Spieler steht
                }
                else
                {
                    playerAnim.SetBool("walk", true); // Animation aktivieren
                }
                if (isRunning)
                {
                    playerAnim.SetBool("run", true); // Animation aktivieren
                }
                else
                {
                    playerAnim.SetBool("run", false); // Animation deaktivieren, wenn der Spieler steht
                }
            }
            else if (playerStateManager.currentHealthState == HealthState.Down)
            {
                if (curSpeedX > -epsilon && curSpeedX < epsilon && curSpeedY > -epsilon && curSpeedY < epsilon)
                {
                    playerAnim.SetBool("crawl", false); // Animation deaktivieren, wenn der Spieler steht
                }
                else
                {
                    playerAnim.SetBool("crawl", true); // Animation aktivieren
                }
            }


            #endregion Animation

            #region Movement Jumping
            if (playerStateManager.currentHealthState == HealthState.Healthy || playerStateManager.currentHealthState == HealthState.Injured)
            {
                if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
                {
                    moveDirection.y = jumpPower;
                }
                else
                {
                    moveDirection.y = movementDirectionY;
                }
            }
            else if (playerStateManager.currentHealthState == HealthState.Down)
            {
                moveDirection.y = movementDirectionY;
            }
            if (!characterController.isGrounded)
            {
                moveDirection.y -= gravity * Time.deltaTime;
            }
            #endregion Movement Jumping

            #region Handles Rotation

            characterController.Move(moveDirection * Time.deltaTime);

            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            if (!menuScreen.GetIsPaused())
            {
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

                if (playerStateManager.currentHealthState != HealthState.DeathChair)
                {
                     transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
                }
            }
            #endregion Handles Rotation
        }

        public void PlayerWon()
        {
            menuScreen.StartWinMessage();

            GameObject winnerWall = GameObject.FindGameObjectWithTag("WinnerWall");
            winnerWall.GetComponent<BoxCollider>().enabled = true;
        }
    }
}