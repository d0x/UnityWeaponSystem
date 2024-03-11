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
        if (attachedProjectile != null) {
            attachedProjectile.fireLocal();
            attachedProjectile = null;
        }
        else {
            Debug.Log($"{GetType().logName()} [CID:{OwnerClientId}]: No Projectile found!");
        }
    }

    public void fireServer() {
        if (attachedProjectile == null) {
            Debug.Log($"{GetType().logName()}: No Projectile");
            var projectile = Instantiate(projectilePrefab, projectileAnchor);
            projectile.NetworkObject.SpawnWithOwnership(OwnerClientId);
            attachProjectileClientRpc(projectile.NetworkObject);
        }

        fireClientRpc();
    }

    public void spawnAndAttachProjectileIfNeeded() {
        if (IsServer && spawnProjectile && attachedProjectile == null) {
            var projectile = Instantiate(projectilePrefab, projectileAnchor);
            projectile.NetworkObject.SpawnWithOwnership(OwnerClientId);

            attachProjectileClientRpc(projectile.NetworkObject);
        }
    }

    [ClientRpc]
    private void attachProjectileClientRpc(NetworkObjectReference projectileNor) {
        projectileNor.TryGet(out var networkObject);
        var projectile = networkObject.GetComponent<Projectile>();

        projectile.GetComponent<FollowTransform>().followTarget = projectileAnchor;
        attachedProjectile = projectile;
    }
}