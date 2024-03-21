using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkActivationSyncer : NetworkBehaviour {
    private NetworkVariable<bool> isActive = new();

    public override void OnNetworkSpawn() {
        if (IsServer) {
            isActive.Value = gameObject.activeSelf;
        }
        else {
            HandleActiveStateChanged(false, isActive.Value);
        }

        isActive.OnValueChanged += HandleActiveStateChanged;
    }

    private void OnEnable() {
        if(IsServer)
        isActive.Value = true;
    }

    private void OnDisable() {
        if(IsServer)
        isActive.Value = false;
    }

    private void HandleActiveStateChanged(bool oldState, bool newState) {
        Debug.Log($"{GetType().logName()}: {gameObject.name} Change active from {oldState} to {newState}");
        
        gameObject.SetActive(newState);
    }

}