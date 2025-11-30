using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts
{
    public class OutlineObjects : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private GameObject thirdPersonCamera;
        [SerializeField] private float maxDistance = 10f;
        [SerializeField] private LayerMask interactableLayer;
        private Outline lastHighlightedOutline;
        [SerializeField] private PlayerStateManager playerStateManager;

        void Start()
        {
            playerStateManager = GetComponent<PlayerStateManager>();
        }

        void Update()
        {
            // Raycast-Logik bleibt wie zuvor
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (playerStateManager.currentPlayerState == PlayerState.Prop)
            {
                ray = thirdPersonCamera.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableLayer))
            {
                GameObject hitObject = hit.collider.gameObject;
                Outline outline = hitObject.GetComponent<Outline>();

                if (outline != null && outline != lastHighlightedOutline)
                {
                    if (lastHighlightedOutline != null)
                    {
                        lastHighlightedOutline.enabled = false;
                    }

                    outline.enabled = true;
                    lastHighlightedOutline = outline;
                }
            }
            else
            {
                if (lastHighlightedOutline != null)
                {
                    lastHighlightedOutline.enabled = false;
                    lastHighlightedOutline = null;
                }
            }
        }
    }
}