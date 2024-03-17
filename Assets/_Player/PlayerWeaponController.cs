using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerWeaponController : NetworkBehaviour {
    [SerializeField] private Weapon bazooka;
    [SerializeField] private Weapon grenade;
    [SerializeField] private Weapon rifle;

    private NetworkVariable<WeaponType> activeWeaponEnum = new(writePerm: NetworkVariableWritePermission.Owner);
    private List<Weapon> weapons = new();

    private Weapon activeWeapon;
    private Projectile activeProjectile;

    private void Awake() {
        weapons.Add(bazooka);
        weapons.Add(grenade);
        weapons.Add(rifle);

        activeWeaponEnum.OnValueChanged += (_, _) => { enableActiveWeaponOnly(); };
    }

    public override void OnNetworkSpawn() {
        enableActiveWeaponOnly();
    }

    public void equip(WeaponType weapon) {
        activeWeaponEnum.Value = weapon;
    }

    public void fire() {
        var projectile = performFire();
        simulateFireServerRpc(projectile.id);
    }

    private Projectile performFire() {
        var projectile = ProjectileManager.INSTANCE.releaseProjectileFromPool(activeWeaponEnum.Value);

        projectile.fly(activeWeapon.projectileAnchor.position, activeWeapon.projectileAnchor.rotation);
        projectile.activateOwner();

        return projectile;
    }

    [ServerRpc]
    private void simulateFireServerRpc(int projectileId) {
        simulateFireClientRpc(projectileId);
    }

    [ClientRpc]
    private void simulateFireClientRpc(int projectileId) {
        if (IsOwner) return;

        var projectile = ProjectileManager.INSTANCE.releaseFromPool(projectileId);

        projectile.fly(activeWeapon.projectileAnchor.position, activeWeapon.projectileAnchor.rotation);

        projectile.activateDummy();
    }

    private void enableActiveWeaponOnly() {
        // deactivate all weapons
        foreach (var weapon in weapons) {
            weapon.gameObject.SetActive(false);
        }

        var activeWeaponValue = activeWeaponEnum.Value;

        activeWeapon = activeWeaponValue switch {
            WeaponType.NONE => null,
            WeaponType.BAZOOKA => bazooka,
            WeaponType.GRENADE => grenade,
            WeaponType.RIFLE => rifle,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (activeWeapon == null) {
            Debug.Log($"[CID:{OwnerClientId}] No active weapon.");
            return;
        }

        activeWeapon.gameObject.SetActive(true);

        // spawnAndAttachProjectileIfNeeded(activeWeapon);

        Debug.Log($"[CID:{OwnerClientId}] {activeWeaponValue} is now active.");
    }

    // private void spawnAndAttachProjectileIfNeeded(Weapon weapon) {
    //     if (weapon.spawnProjectile && weapon.attachedProjectile == null) {
    //         weapon.attachedProjectile = Instantiate(weapon.projectilePrefab, weapon.projectileAnchor);
    //     }
    // }
}