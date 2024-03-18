using System;
using Unity.Netcode;
using UnityEngine;

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

    [SerializeField] private Weapon none;
    [SerializeField] private Weapon bazooka;
    [SerializeField] private Weapon grenade;
    [SerializeField] private Weapon rifle;

    private NetworkVariable<WeaponInfo> activeWeaponInfo = new(
        writePerm: NetworkVariableWritePermission.Owner,
        value: WEAPON_NONE
    );

    private Weapon activeWeapon;
    private Projectile activeProjectile;

    private void Awake() {
        none.gameObject.SetActive(true);
        bazooka.gameObject.SetActive(false);
        grenade.gameObject.SetActive(false);
        rifle.gameObject.SetActive(false);

        activeWeaponInfo.OnValueChanged += simulateSwitchWeapon;
    }

    public override void OnNetworkSpawn() {
        simulateSwitchWeapon(WEAPON_NONE, activeWeaponInfo.Value);
    }

    private void simulateSwitchWeapon(WeaponInfo previousvalue, WeaponInfo newvalue) {
        holster(previousvalue);
        activeWeapon = getWeapon(newvalue.type);
        activeWeapon.gameObject.SetActive(true);

        if (newvalue.projectileId == -1) {
            activeProjectile = null;
        }
        else {
            activeProjectile = ProjectilePool.INSTANCE.release(newvalue.projectileId);
            activeProjectile.followTransform.followTarget = activeWeapon.projectileAnchor;
        }
    }

    public void equip(WeaponType weapon) {
        if (weapon == activeWeaponInfo.Value.type) {
            Debug.Log($"{GetType().logName()}: weapon {weapon} already equipped.");
            return;
        }

        var weaponInfo = performEquip(weapon);
        activeWeaponInfo.Value = weaponInfo;
    }

    private WeaponInfo performEquip(WeaponType weaponType) {
        holster(activeWeaponInfo.Value);

        activeWeapon = getWeapon(weaponType);

        var projectileId = -1;
        if (activeWeapon.spawnProjectile) {
            var projectile = ProjectilePool.INSTANCE.release(weaponType);
            projectile.followTransform.followTarget = activeWeapon.projectileAnchor;
            projectileId = projectile.id;
        }

        return new WeaponInfo {
            type = weaponType,
            projectileId = projectileId
        };
    }

    private Weapon getWeapon(WeaponType weaponType) {
        return weaponType switch {
            WeaponType.NONE => none,
            WeaponType.BAZOOKA => bazooka,
            WeaponType.GRENADE => grenade,
            WeaponType.RIFLE => rifle,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void holster(WeaponInfo weaponInfo) {
        getWeapon(weaponInfo.type).gameObject.SetActive(false);
        var projectileId = weaponInfo.projectileId;
        if (projectileId != -1) {
            ProjectilePool.INSTANCE.returnToPool(projectileId);
        }
    }

    public void fire() {
        var position = activeWeapon.transform.position;
        var rotation = activeWeapon.transform.rotation;
        
        var projectile = performFire(position, rotation);
        simulateFireServerRpc(projectile.id, position, rotation);
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

    [ServerRpc]
    private void simulateFireServerRpc(int projectileId, Vector3 position, Quaternion rotation) {
        simulateFireClientRpc(projectileId, position, rotation);
    }

    [ClientRpc]
    private void simulateFireClientRpc(int projectileId, Vector3 position, Quaternion rotation) {
        if (IsOwner) return;

        activeProjectile = null;
        var projectile = ProjectilePool.INSTANCE.release(projectileId);

        projectile.fly(position, rotation);

        projectile.simulateActivation();
    }
}