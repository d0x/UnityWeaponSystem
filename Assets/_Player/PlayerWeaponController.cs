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

    private void Awake() {
        weapons.Add(bazooka);
        weapons.Add(grenade);
        weapons.Add(rifle);

        activeWeaponEnum.OnValueChanged += (_, _) => { enableActiveWeaponOnly(); };
    }

    public override void OnNetworkSpawn() {
        enableActiveWeaponOnly();
    }

    #region API For PlayerController

    // In case you need to transfer ownership or smth, you can do it here
    public void startTurn() {
        Debug.Log($"[CID:{OwnerClientId}] starts turn");
    }

    // In case you need to transfer ownership or smth, you can do it here
    public void endTurn() {
        Debug.Log($"[CID:{OwnerClientId}] ends turn");
    }

    // Tells the player to change its current weapon
    public void equip(WeaponType weapon) {
        activeWeaponEnum.Value = weapon;
    }

    // Tells the player to fire its current weapon
    public void fire() {
        fireServerRpc();
    }

    #endregion

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

        spawnAndAttachProjectileIfNeeded(activeWeapon);

        Debug.Log($"[CID:{OwnerClientId}] {activeWeaponValue} is now active.");
    }

    private void spawnAndAttachProjectileIfNeeded(Weapon weapon) {
        if (weapon.spawnProjectile && weapon.attachedProjectile == null) {
            weapon.attachedProjectile = Instantiate(weapon.projectilePrefab, weapon.projectileAnchor);
        }
    }

    [ServerRpc]
    private void fireServerRpc() {
        if (activeWeapon == null) return;

        fireClientRpc();
    }

    [ClientRpc]
    private void fireClientRpc() {
        if (activeWeapon == null) return;

        var projectile = activeWeapon.attachedProjectile;

        if (projectile == null) {
            projectile = Instantiate(activeWeapon.projectilePrefab, activeWeapon.projectileAnchor);
        }

        // projectile.transform.SetParent(null);

        var projectileRb = projectile.GetComponent<Rigidbody>();
        projectileRb.isKinematic = false;
        projectileRb.useGravity = true;
        projectileRb.AddForce(projectile.transform.forward * 500);

        activeWeapon.attachedProjectile = null;
        projectile.activateExplosionTimer();
    }
}