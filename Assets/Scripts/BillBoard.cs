using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class BillBoard : MonoBehaviour
    {
        private Camera targetCamera;
        void Update()
        {
            if (targetCamera != null)
            {
                transform.LookAt(transform.position + targetCamera.transform.forward);
            }
        }

        public void SetTargetCamera(Camera camera)
        {
            targetCamera = camera;
        }
    }
}