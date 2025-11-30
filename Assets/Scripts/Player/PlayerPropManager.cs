using Assets.Scripts.Transformation;
using System.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerPropManager : NetworkBehaviour
    {
        [Header("Settings------------------------------------------------------------------------")]
        private PlayerStateManager playerStateManager;
        private GameObject objectRootPos;
        [SerializeField] private LayerMask groundLayerMask;


        [Header("Transformation Parameters-------------------------------------------------------")]
        [SerializeField] public GameObject transformGameObject;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private GameObject originalPlayerModel;
        [SerializeField] private float transformRange = 10f;

        [Header("Camera---------------------------------------------------------------------------")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private GameObject tpCamera;
        [SerializeField] private GameObject tpCameraRoot;

        [Header("PropMovement--------------------------------------------------------------------")]
        private int jumpCount = 0;
        [SerializeField] private float objectJumpPower;
        [SerializeField] private float maxSpeed = 15f; // Maximalgeschwindigkeit
        [SerializeField] private float moveForce = 30f; // Die Stärke der Bewegungskraft
        private void Start()
        {
            if (playerStateManager == null)
            {
                playerStateManager = GetComponent<PlayerStateManager>();
            }
        }
        public void HandlePropMovement()
        {
            // Bewegungseingabe erfassen
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
            bool jumpPressed = Input.GetButtonDown("Jump");
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift);

            ExecuteLocalMovement(inputDirection, jumpPressed, shiftHeld);

            // Sende Eingaben an den Server
            SendPositionToServerRpc(transformGameObject.transform.position, rb.linearVelocity, transformGameObject.transform.rotation);
        }

        private void ExecuteLocalMovement(Vector3 inputDirection, bool jumpPressed, bool shiftHeld)
        {
            Vector3 localMoveDirection = tpCamera.transform.TransformDirection(inputDirection);
            localMoveDirection.y = 0f;
            localMoveDirection.Normalize();
            bool isGrounded = CheckGrounded();

            // Doppelsprung-Logik
            if (jumpPressed && jumpCount < 1)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, objectJumpPower, rb.linearVelocity.z);
                jumpCount++;
            }

            if (isGrounded)
            {
                jumpCount = 0;
            }

            if (shiftHeld)
            {
                rb.angularVelocity = Vector3.zero;
                transformGameObject.transform.rotation = Quaternion.Slerp(transformGameObject.transform.rotation, Quaternion.identity, Time.deltaTime * 15f);
            }

            if (localMoveDirection.magnitude > 0)
            {
                Vector3 desiredVelocity = localMoveDirection * maxSpeed;
                desiredVelocity.y = rb.linearVelocity.y;

                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, Time.deltaTime * moveForce);
            }
        }

        [ServerRpc]
        private void SendPositionToServerRpc(Vector3 clientPosition, Vector3 newVelocity, Quaternion clientRotation)
        {
            // Synchronisiere die Bewegung nur der anderen Clients
            UpdateMovementClientRpc(clientPosition, newVelocity, clientRotation);
        }

        // ClientRpc: Server sendet Bewegungsergebnisse an alle Clients
        [ClientRpc]
        private void UpdateMovementClientRpc(Vector3 newPosition, Vector3 newVelocity, Quaternion newRotation)
        {
            if (IsOwner) return;
            rb.linearVelocity = newVelocity;
            transformGameObject.transform.SetPositionAndRotation(newPosition, newRotation);
        }

        // Robuste Bodenabfragemethode
        private bool CheckGrounded()
        {
            const float rayLength = 1.1f; // Etwas mehr als 1, um eine zuverlässige Detektion zu gewährleisten
            return Physics.Raycast(transformGameObject.transform.position, Vector3.down, rayLength, groundLayerMask);
        }
       

        public void HandleTPCameraRootPos()
        {
            const float smoothing = 0.15f; // Dämpfungsfaktor für sanfte Bewegung

            // Zielposition berechnen
            Vector3 targetPosition = tpCameraRoot.transform.position;
            targetPosition.x = transformGameObject.transform.position.x;
            targetPosition.z = transformGameObject.transform.position.z;
            float yChange = (transformGameObject.transform.position.y - 2f) * 0.8f;
            targetPosition.y = 2.5f + yChange;

            // Sanfte Annäherung an die Zielposition
            tpCameraRoot.transform.position = Vector3.Lerp(tpCameraRoot.transform.position, targetPosition, smoothing);
        }
        public void HandleTransformation()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = new(playerCamera.transform.position, playerCamera.transform.forward);
                if (playerStateManager.currentPlayerState == PlayerState.Prop && playerStateManager.currentHealthState != HealthState.DeathChair)
                {
                    float distanceToObject = Vector3.Distance(tpCamera.transform.position, objectRootPos.transform.position);
                    Vector3 startPoint = tpCamera.transform.position + (tpCamera.transform.forward * distanceToObject);
                    ray = new Ray(startPoint, tpCamera.transform.forward);
                }

                // Starte Coroutine, um die Linie für eine Sekunde zu zeichnen
                StartCoroutine(DrawRayLineForDuration(ray, transformRange, 0.5f)); // 1 Sekunde

                if (Physics.Raycast(ray, out RaycastHit hit, transformRange) && hit.collider.TryGetComponent<TransformableObject>(out var transformableObject))
                {
                    NetworkObject targetNetworkObject = transformableObject.transformationPrefab.GetComponent<NetworkObject>();
                    TransformPlayerIntoObjectServerRpc(targetNetworkObject.NetworkObjectId);
                }
            }

            else if (Input.GetMouseButtonDown(1) && playerStateManager.currentPlayerState == PlayerState.Prop && playerStateManager.currentHealthState != HealthState.DeathChair && playerStateManager.currentHealthState != HealthState.OnKiller)
            {
                TransformPlayerIntoPlayerServerRpc();
            }
            

        }
        private static IEnumerator DrawRayLineForDuration(Ray ray, float range, float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                // Zeichne die Linie von der Kamera in Blickrichtung (rote Linie)
                Debug.DrawLine(ray.origin, ray.origin + (ray.direction * range), Color.blue);

                // Falls etwas getroffen wird, zeichne die Linie zum Trefferpunkt (grüne Linie)
                if (Physics.Raycast(ray, out RaycastHit hit, range))
                {
                    Debug.DrawLine(ray.origin, hit.point, Color.green);
                }
                timer += Time.deltaTime;
                yield return null; // Warte bis zum nächsten Frame
            }
        }

        [ServerRpc]
        private void TransformPlayerIntoObjectServerRpc(ulong targetNetworkObjectId)
        {
            RpcTransformPlayerClientRPC(targetNetworkObjectId);  // Auf allen Clients ausführen
        }

        // Client-RPC, um die Transformation zu synchronisieren
        [ClientRpc]
        private void RpcTransformPlayerClientRPC(ulong targetNetworkObjectId)
        {
            // Suche das GameObject anhand der NetworkObject-ID
            NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
            if (targetNetworkObject != null)
            {
                TransformIntoObject(targetNetworkObject.gameObject);
            }
        }
        private void TransformIntoObject(GameObject transformationPrefab)
        {
            GetComponent<CharacterController>().enabled = false;
            if (playerStateManager.currentPlayerState == PlayerState.Prop)
            {
                transform.position = rb.position;
            }
            transformGameObject.transform.position = transform.position;
            playerStateManager.SetState(PlayerState.Prop);

            if (transformationPrefab != null)
            {
                originalPlayerModel.SetActive(false);
                transformGameObject.SetActive(true);

                //in object verwandeln ( mesh, material,... auf eigenes gameobject übertragen)
                transformGameObject.GetComponent<MeshFilter>().mesh = transformationPrefab.GetComponent<MeshFilter>().mesh;
                transformGameObject.GetComponent<MeshRenderer>().material =
                    transformationPrefab.GetComponent<MeshRenderer>().material;
                transformGameObject.GetComponent<MeshCollider>().sharedMesh =
                    transformationPrefab.GetComponent<MeshCollider>().sharedMesh;
                transformGameObject.GetComponent<Transform>().localScale =
                    transformationPrefab.GetComponent<Transform>().localScale;

                GameObject childObject = new("ObjectCameraRoot");
                childObject.transform.SetParent(transformGameObject.transform);
                childObject.AddComponent<Rigidbody>();
                if (transformGameObject.TryGetComponent<Renderer>(out var renderer))
                {
                    // Setze die Position des Child-Objekts auf den Mittelpunkt des Parent-Objekts
                    childObject.transform.localPosition = renderer.bounds.center - transformGameObject.transform.position;
                }
                childObject.layer = 6;
                objectRootPos = childObject;
                Rigidbody rbObj = childObject.GetComponent<Rigidbody>();
                rbObj.useGravity = false;
                rbObj.isKinematic = true;
                rbObj.constraints = RigidbodyConstraints.FreezePosition;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TransformPlayerIntoPlayerServerRpc()
        {
            TransformPlayerIntoPlayerClientRpc();  // Auf allen Clients ausführen
        }

        // Client-RPC, um die Transformation zu synchronisieren
        [ClientRpc]
        private void TransformPlayerIntoPlayerClientRpc()
        {
            TransformIntoPlayer();
        }
        private void TransformIntoPlayer()
        {
            if (transformGameObject != null)
            {
                transform.position = transformGameObject.transform.position;
                transformGameObject.SetActive(false);
            }
            GetComponent<CharacterController>().enabled = true;
            playerStateManager.SetState(PlayerState.Human);
            originalPlayerModel.SetActive(true);
        }
    }
}
