using System;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(FollowTransform))]
public class Projectile : NetworkBehaviour {
    public Action ExplodeEvent;
    private FollowTransform followTransform;
    private Boolean blownUp;

    private void Awake() {
        Debug.Log($"{GetType().logName()} [CID:{OwnerClientId}]: Awake");
        followTransform = GetComponent<FollowTransform>();
    }

    public void blowUp() {
        if (blownUp == false) {
            ExplodeEvent?.Invoke();
            despawnAndDestroy();
            blownUp = true;
        }
        else {
            Debug.Log($"{GetType().logName()}: Already blown up");
        }
    }

    public override void OnNetworkSpawn() {
        Debug.Log($"{GetType().logName()} [CID:{OwnerClientId}]: OnNetworkSpawn");

        var playerWeaponController = PlayerWeaponController.getInstance(OwnerClientId);

        if (playerWeaponController == null) {
            Debug.LogError($"PlayerController not found Owner: {OwnerClientId}");
            return;
        }

        var activeWeapon = playerWeaponController.activeWeapon;
        if (activeWeapon != null) {
            followTransform.followTarget = activeWeapon.projectileAnchor;
            activeWeapon.attachedProjectile = this;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void activateServerRpc() {
        activateClientRpc();
    }

    [ClientRpc]
    private void activateClientRpc() {
        Invoke(nameof(blowUp), 3f);
    }

    public void despawnAndDestroy() {
        if (!IsServer) return;
        Debug.Log($"{GetType().logName()}: DespawnAndDestroy");

        NetworkObject.Despawn();
        Destroy(gameObject);
    }

    public void fireLocal() {
        Debug.Log($"{GetType().logName()}: Fire Local");

        followTransform.followTarget = null;
        GetComponent<ClientNetworkTransform>().enabled = true;

        var projectileRb = GetComponent<Rigidbody>();
        projectileRb.isKinematic = false;
        projectileRb.useGravity = true;

        if (IsOwner) {
            Debug.Log($"{GetType().logName()}: Add Force");
            projectileRb.AddForce(transform.forward * 500);
        }
        else {
            // Maybe disable Rigidbody on other clients?
        }

        activateServerRpc();
    }
}