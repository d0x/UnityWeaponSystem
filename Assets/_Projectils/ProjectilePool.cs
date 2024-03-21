using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct PoolItem {
    public WeaponType weaponType;
    public Weapon weaponPrefab;
    public int count;
}

[Serializable]
public struct ProjectilePoolItem {
    public ProjectileType projectileType;
    public Projectile projectilePrefab;
    public int count;
}

public class ProjectilePool : NetworkBehaviour {
    public static ProjectilePool INSTANCE;

    [SerializeField] private PoolItem[] weapons;
    [SerializeField] private ProjectilePoolItem[] projectiles;
    
    private Dictionary<WeaponType, List<Weapon>> weaponPool = new();
    private Dictionary<ulong, Weapon> weaponById = new();
    private Dictionary<WeaponType, GameObject> weaponParents = new();


    private Dictionary<ProjectileType, List<Projectile>> projectilePool = new();
    private Dictionary<ulong, Projectile> projectileById = new();
    private Dictionary<ProjectileType, GameObject> projectileParents = new();

    private int idSequence = 0;

    private void Awake() {
        INSTANCE = this;
        
        foreach (WeaponType weaponType in Enum.GetValues(typeof(WeaponType)))
        {
            weaponPool[weaponType] = new List<Weapon>();
        }
    }

    public void addToPool(Weapon weapon) {
        Debug.Log($"{GetType().logName()}: Add {weapon.type} to pool");
        
        
        
        weaponPool[weapon.type].Add(weapon);
    }
    
    public override void OnNetworkSpawn() {
        if (!IsServer) return;

        foreach (var weapon in weapons) {
            initializeWeaponPool(weapon.weaponType, weapon.weaponPrefab, weapon.count);
        }

        foreach (var projectile in projectiles) {
            initializeProjectilePool(projectile.projectileType, projectile.projectilePrefab, projectile.count);
        }
    }

    private void initializeWeaponPool(WeaponType weaponType, Weapon prefab, int count) {
        weaponPool[weaponType] = new List<Weapon>(count);

        // var parentObject = new GameObject($"{weaponType.ToString()} Pool");
        // parentObject.transform.parent = transform;
        // weaponParents[weaponType] = parentObject;

        for (int i = 0; i < count; i++) {
            var weapon = Instantiate(prefab);
            var id = idSequence++;
            weapon.gameObject.name = prefab.name + " (" + id + ")";
            weapon.NetworkObject.Spawn();
            weapon.gameObject.SetActive(false);

         //   weaponById[weapon.NetworkObjectId] = weapon;
        //    weaponPool[weaponType].Add(weapon);
        }
    }

    private void initializeProjectilePool(ProjectileType projectileType, Projectile prefab, int count) {
        projectilePool[projectileType] = new List<Projectile>(count);

        // var parentObject = new GameObject($"{projectileType.ToString()} Pool");
        // parentObject.transform.parent = transform;
        // projectileParents[projectileType] = parentObject;

        for (int i = 0; i < count; i++) {
            var projectile = Instantiate(prefab);
            projectile.id = idSequence++;
            projectile.gameObject.name = prefab.name + " (" + projectile.id + ")";
            projectile.NetworkObject.Spawn();
            projectile.gameObject.SetActive(false);

            projectileById[projectile.NetworkObjectId] = projectile;
            projectilePool[projectileType].Add(projectile);
        }
    }

    public Projectile release(int projectileId) {
        return release(get(projectileId));
    }

    private Projectile release(Projectile projectile) {
        if (projectile.gameObject.activeSelf) {
            Debug.LogWarning(
                $"{GetType().logName()}: release got called on an already active object. Id: {projectile.id} - Name: {projectile.gameObject.name}");
        }

        projectile.transform.parent = null;
        projectile.gameObject.SetActive(true);
        return projectile;
    }

    public Projectile release(ProjectileType projectileType) {
        for (var i = 0; i < projectilePool[projectileType].Count; i++) {
            var projectile = projectilePool[projectileType][i];
            if (projectile.gameObject.activeInHierarchy) continue;
            return release(projectile);
        }

        throw new InvalidOperationException("No object could be found. It could be that all objects are used.");
    }

    public Projectile release(WeaponType projectileType) {
        return projectileType switch {
            WeaponType.GRENADE => release(ProjectileType.GRENADE),
            WeaponType.BAZOOKA => release(ProjectileType.ROCKET),
            WeaponType.RIFLE => release(ProjectileType.BULLET),
            WeaponType.NONE => null,
            _ => null
        };
    }


    public Weapon releaseWeapon(WeaponType weaponType) {
        for (var i = 0; i < weaponPool[weaponType].Count; i++) {
            var weapon = weaponPool[weaponType][i];
            if (weapon.gameObject.activeInHierarchy) continue;
            weapon.gameObject.SetActive(true);
            return weapon;
        }

        throw new InvalidOperationException("No object could be found. It could be that all objects are used.");
    }

    public void returnToPool(Projectile projectile) {
        projectile.reset();
        projectile.transform.parent = projectileParents[projectile.type].transform;
        projectile.gameObject.SetActive(false);
    }

    public void returnToPool(int projectileId) {
        returnToPool(get(projectileId));
    }

    public Projectile get(int projectileId) {
        //return projectileById[projectileId];
        return null;
    }
}