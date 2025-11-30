using System.Globalization;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class RepairGenerator : MonoBehaviour
    {
        [SerializeField] private float raycastDistance = 2f; // Max. Entfernung für den Raycast
        [SerializeField] private LayerMask generatorLayer;   // Layer für Generatoren, um die Suche zu optimieren
        [SerializeField] private LayerMask escapeDoorLayer;   
        private SurvivorFPSController survivorController;
        private PlayerStateManager playerStateManager;

        private GeneratorSkript currentGenerator;
        private bool isRepairing;
        private EscapeDoor currentEscapeDoor;
        private bool isOpening;
        private TasksManager tasksManager;
       
        void Start()
        {
            survivorController = GetComponent<SurvivorFPSController>();
            playerStateManager = GetComponent<PlayerStateManager>();
            tasksManager = FindFirstObjectByType<TasksManager>();
            isRepairing = false;
            isOpening = false;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E) && CheckForGenerator() && currentGenerator != null && !isRepairing && currentGenerator.RepairingPlayers.Value <= 3 && !tasksManager.isEscapeGateEnabled)
            {
                currentGenerator.StartRepairingServerRpc();
                survivorController.SetCanMove(false);
                playerStateManager.SetState(PlayerState.Prop);
                GetComponent<PlayerPropManager>().transformGameObject.transform.position = transform.position;
                survivorController.clearAnims();
                survivorController.setAnim("repair", true);
                isRepairing = true;
                playerStateManager.setIsRepairing(true);
            }
            if (isRepairing && currentGenerator.GetComponent<GeneratorSkript>().IsRepaired.Value)
            {
                StopRepair();
            }

            if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)) && isRepairing)
            {
                StopRepair();
            }

            //escape Door
            if (Input.GetKeyDown(KeyCode.E) && CheckForEscapeDoor() && currentEscapeDoor != null && !isOpening && currentEscapeDoor.RepairingPlayers.Value == 0 && currentEscapeDoor.IsEnabled.Value)
            {
                currentEscapeDoor.StartOpeningServerRpc();
                survivorController.SetCanMove(false);
                playerStateManager.SetState(PlayerState.Prop);
                survivorController.clearAnims();
                survivorController.setAnim("repair", true);
                isOpening = true;
                playerStateManager.setIsRepairing(true);
            }
            
           


            if (((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)) && isOpening) || isOpening && currentEscapeDoor.GetComponent<EscapeDoor>().IsOpened.Value)
            {
                StopOpening();
            }
        }

        public void StopRepair()
        {
            Debug.Log("StopRepair");
            isRepairing = false;
            survivorController.clearAnims();
            survivorController.setAnim("repair", false);
            currentGenerator.StopRepairingServerRpc();
            survivorController.SetCanMove(true);
            playerStateManager.SetState(PlayerState.Human);
            playerStateManager.setIsRepairing(false);
        }
        private bool CheckForGenerator()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastDistance, generatorLayer) && hit.collider.CompareTag("Generator"))
            {
                // Generator gefunden
                currentGenerator = hit.collider.GetComponent<GeneratorSkript>();
                return true;
            }
            currentGenerator = null;
            return false;
        }
        private bool CheckForEscapeDoor()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            //if (Physics.Raycast(ray, out hit, raycastDistance))
            //{
            //    Debug.Log($"Hit: {hit.collider.name}, Tag: {hit.collider.tag}, Layer: {hit.collider.gameObject.layer}");
            //}
            if (Physics.Raycast(ray, out hit, raycastDistance, escapeDoorLayer) && hit.collider.CompareTag("EscapeDoor"))
            {
                // Generator gefunden
                currentEscapeDoor = hit.collider.GetComponent<EscapeDoor>();
                Debug.Log("check escapedoor gefunden : " + currentEscapeDoor);
                return true;
            }
            currentEscapeDoor = null;
            return false;
        }
        public void StopOpening()
        {
            isOpening = false;
            survivorController.clearAnims();
            survivorController.setAnim("repair", false);
            currentEscapeDoor.StopOpeningServerRpc();
            survivorController.SetCanMove(true);
            playerStateManager.SetState(PlayerState.Human);
            playerStateManager.setIsRepairing(false);
        }
       

    }
}