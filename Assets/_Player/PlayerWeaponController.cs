using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public struct WeaponInfo : INetworkSerializable {
    public WeaponType type;
    public int projectileId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref projectileId);
    }
}

public class PlayerWeaponController : NetworkBehaviour {
    public static readonly WeaponInfo WEAPON_NONE = new() { type = WeaponType.NONE, projectileId = -1 };

    private NetworkVariable<WeaponInfo> activeWeaponInfo = new(
        writePerm: NetworkVariableWritePermission.Owner,
        value: WEAPON_NONE
    );

    private Weapon activeWeapon;
    public Projectile activeProjectile;

    private void Awake() {
        //activeWeaponInfo.OnValueChanged+= simulateSwitchWeapon;
    }

    public override void OnNetworkSpawn() {
        ProjectileSimulator.INSTANCE.add(NetworkObject.OwnerClientId, this);
       // simulateSwitchWeapon(WEAPON_NONE, activeWeaponInfo.Value);
    }

    private void simulateSwitchWeapon(WeaponInfo previousvalue, WeaponInfo newvalue) {
        if (previousvalue.type != WeaponType.NONE)
            holster(previousvalue);

        activeWeapon = getWeapon(newvalue.type);
        activeWeapon.gameObject.SetActive(true);

        if (newvalue.projectileId == -1) {
            activeProjectile = null;
        }
        else {
            activeProjectile = IsOwner
                ? ProjectilePool.INSTANCE.get(newvalue.projectileId)
                : ProjectilePool.INSTANCE.release(newvalue.projectileId);
            activeProjectile.followTransform.followTarget = activeWeapon.projectileAnchor;
        }
    }

    public void equip(WeaponType weapon) {
        if (weapon == activeWeaponInfo.Value.type) {
            Debug.Log($"{GetType().logName()}: weapon {weapon} already equipped.");
            return;
        }

        var weaponInstance = ProjectilePool.INSTANCE.releaseWeapon(weapon);
        weaponInstance.gameObject.SetActive(true);
        weaponInstance.followTransform.setFollowTarget(NetworkObject);

        // var weaponInfo = performEquip(weapon);
        // activeWeaponInfo.Value = weaponInfo;
    }

    private WeaponInfo performEquip(WeaponType weaponType) {
        holster(activeWeaponInfo.Value);

        activeWeapon = getWeapon(weaponType);

        var projectileId = -1;
        if (activeWeapon.spawnProjectile) {
            activeProjectile = ProjectilePool.INSTANCE.release(weaponType);
            projectileId = activeProjectile.id;
        }

        return new WeaponInfo {
            type = weaponType,
            projectileId = projectileId
        };
    }
    
    private Weapon getWeapon(WeaponType weaponType) {
        return null;
        // return weaponType switch {
        //     WeaponType.NONE => none,
        //     WeaponType.BAZOOKA => bazookaPrefab,
        //     WeaponType.GRENADE => grenadePrefab,
        //     WeaponType.RIFLE => riflePrefab,
        //     _ => throw new ArgumentOutOfRangeException()
        // };
    }

    private void holster(WeaponInfo weaponInfo) {
        // getWeapon(weaponInfo.type).gameObject.SetActive(false);
        // var projectileId = weaponInfo.projectileId;
        // if (projectileId != -1) {
        //     ProjectilePool.INSTANCE.returnToPool(projectileId);
        // }
    }

    public void fire() {
        var position = activeWeapon.transform.position;
        var rotation = activeWeapon.transform.rotation;

        var projectile = performFire(position, rotation);
        ProjectileSimulator.INSTANCE.simulateFireServerRpc( NetworkManager.LocalClientId, projectile.id, position, rotation);
    }

    private Projectile performFire(Vector3 position, Quaternion rotation) {
        var projectile = activeProjectile;
        activeProjectile = null;

        if (projectile == null) {
            projectile = ProjectilePool.INSTANCE.release(activeWeaponInfo.Value.type);
        }

        projectile.fly(position, rotation);
        projectile.performActivation();

        return projectile;
    }
}