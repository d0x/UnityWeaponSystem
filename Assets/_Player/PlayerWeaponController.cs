using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerWeaponController : NetworkBehaviour {
    public PlayerController playerController;
    public Weapon bazookaPrefab;
    public Weapon grenadePrefab;
    public Weapon riflePrefab;

    private NetworkVariable<WeaponType> activeWeaponEnum = new(writePerm: NetworkVariableWritePermission.Owner);
    public Weapon activeWeapon;


    private static Dictionary<ulong, PlayerWeaponController> controllers = new();

    private void Awake() {
        Debug.Log($"{GetType().logName()} [CID:{OwnerClientId}]: Awake");

        activeWeaponEnum.OnValueChanged += (_, _) => { spawnActiveWeapon(); };
    }

    public static PlayerWeaponController getInstance(ulong ownerClientId) {
        if (controllers.ContainsKey(ownerClientId)) {
            return controllers[ownerClientId];
        }

        var controller = FindObjectsByType<PlayerWeaponController>(FindObjectsSortMode.InstanceID)
            .First(it => it.NetworkObject.OwnerClientId == ownerClientId);
        controllers[ownerClientId] = controller;
        return controller;
    }

    public override void OnNetworkSpawn() {
        Debug.Log($"{GetType().logName()} [CID:{OwnerClientId}]: OnNetworkSpawn");

        controllers[OwnerClientId] = this;
        spawnActiveWeapon();
    }

    public override void OnNetworkDespawn() {
        controllers.Remove(OwnerClientId);
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

    private void spawnActiveWeapon() {
        if (activeWeapon != null) {
            activeWeapon.despawnAndDestroy();
        }

        var activeWeaponValue = activeWeaponEnum.Value;

        var prefab = activeWeaponValue switch {
            WeaponType.NONE => null,
            WeaponType.BAZOOKA => bazookaPrefab,
            WeaponType.GRENADE => grenadePrefab,
            WeaponType.RIFLE => riflePrefab,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (IsServer && prefab != null) {
            var weapon = Instantiate(
                prefab,
                playerController.weaponAnchor.position,
                playerController.weaponAnchor.rotation);

            weapon.NetworkObject.SpawnWithOwnership(NetworkObject.OwnerClientId);

            weapon.spawnAndAttachProjectileIfNeeded();
        }
    }

    [ServerRpc]
    private void fireServerRpc() {
        if (activeWeapon == null) return;
        activeWeapon.fireServer();
    }
}