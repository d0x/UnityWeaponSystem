using System;
using Unity.Netcode;
using UnityEngine;

// TODO Should be NetworkBehaviour
public class Projectile : MonoBehaviour {
    public Action ExplodeEvent;

    public void blowUp() {
        ExplodeEvent?.Invoke();
        Destroy(gameObject);
    }

    [ServerRpc]
    public void activateServerRpc() {
        activateClientRpc();
    }

    [ClientRpc]
    private void activateClientRpc() {
        Invoke(nameof(blowUp), 3f);
    }
}