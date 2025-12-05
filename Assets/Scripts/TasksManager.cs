using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts
{
    public class TasksManager : MonoBehaviour
    {
        public bool isEscapeGateEnabled = false;
        [SerializeField] private EscapeDoor escapeDoor;
        public bool opendoors = false;

        public GeneratorProgressBarUI generatorProgressBarUI;
        private NetworkVariable<int> repairedGeneratorsCountNV = new NetworkVariable<int>();

        private void HandleRepairedGeneratorsCountChanged(int oldValue, int newValue)
        {
            if (generatorProgressBarUI != null)
            {
                generatorProgressBarUI.UpdateGeneratorUI(newValue);
            }
        }
        private void OnDisable()
        {
            repairedGeneratorsCountNV.OnValueChanged -= HandleRepairedGeneratorsCountChanged;
        }
        public void OnGeneratorRepairedNV()
        {
            repairedGeneratorsCountNV.Value++;
            Debug.Log($"RepairedGeneratorsCount is now {repairedGeneratorsCountNV.Value}");
            if (repairedGeneratorsCountNV.Value >= 5 && !isEscapeGateEnabled)
            {
                EnableEscapeGate();
            }
        }
        void Update()
        {
            if (opendoors)
            {
                EnableEscapeGate();
            }
        }

        private void EnableEscapeGate()
        {
            isEscapeGateEnabled = true;
            escapeDoor.SetCanOpenTrueServerRpc();
            // Tor offnen

            Debug.Log("Escape Gate is now open!");
        }

        
        public void RegisterGeneratorRepairedServerRpc(ServerRpcParams rpcParams = default)
        {
            OnGeneratorRepairedNV();
        }
    }
}