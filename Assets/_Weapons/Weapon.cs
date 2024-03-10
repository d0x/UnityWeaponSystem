using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(FollowTransform))]
public class Weapon : NetworkBehaviour {
    /**
     * true if projectiles should be spawned when the weapon
     * gets equipped. Like the rocket of a bazooka.
     */
    public Boolean spawnProjectile;

    public Projectile projectilePrefab;
    public Transform projectileAnchor;

    /**
     * The currently projectile of the weapon.
     * It can be null (like on a rifle where the bullet is spawned
     * on demand)
     */
    public Projectile attachedProjectile;

    private FollowTransform followTransform;

    private void Awake() {
        Debug.Log($"{GetType().logName()} [CID:{OwnerClientId}]: Awake");
        followTransform = GetComponent<FollowTransform>();
    }

    public override void OnNetworkSpawn() {
        Debug.Log($"{GetType().logName()} [CID:{OwnerClientId}]: OnNetworkSpawn");

        var playerWeaponController = PlayerWeaponController.getInstance(OwnerClientId);

        if (playerWeaponController == null) {
            Debug.LogError($"PlayerController not found Owner: {OwnerClientId}");
            return;
        }

        followTransform.followTarget = playerWeaponController.playerController.weaponAnchor;
        playerWeaponController.activeWeapon = this;
    }

    public override void OnNetworkDespawn() {
        if (attachedProjectile != null) {
            attachedProjectile.despawnAndDestroy();
        }
    }

    public void despawnAndDestroy() {
        if (!IsServer) return;

        NetworkObject.Despawn();
        Destroy(gameObject);
    }

    [ClientRpc]
    public void fireClientRpc() {
        var projectile = attachedProjectile;
        attachedProjectile = null;

        if (projectile == null) {
            Debug.Log($"{GetType().logName()}: No Projectile");
            // TODO: Spawn it.
            return;
        }

        projectile.fireLocal();
    }
}