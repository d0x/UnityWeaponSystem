using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum ProjectileType {
    BULLET,
    CLUSTER_PART,
    GRENADE,
    ROCKET
}

public class ProjectileManager : NetworkBehaviour {
    public static ProjectileManager INSTANCE;

    [SerializeField] private int poolSizePerType;

    [SerializeField] private Projectile bulletPrefab;
    [SerializeField] private Projectile clusterPartPrefab;
    [SerializeField] private Projectile grenadePrefab;
    [SerializeField] private Projectile rocketPrefab;

    private Dictionary<ProjectileType, List<Projectile>> projectilePool = new();
    private Dictionary<int, Projectile> projectileById = new();

    // Parent GameObjects for each type of projectile
    private Dictionary<ProjectileType, GameObject> projectileParents = new();

    private int idSequence = 0;

    private void Start() {
        INSTANCE = this;

        initializeProjectilePool(ProjectileType.BULLET, bulletPrefab);
        initializeProjectilePool(ProjectileType.CLUSTER_PART, clusterPartPrefab);
        initializeProjectilePool(ProjectileType.GRENADE, grenadePrefab);
        initializeProjectilePool(ProjectileType.ROCKET, rocketPrefab);
    }

    private void initializeProjectilePool(ProjectileType projectileType, Projectile prefab) {
        projectilePool[projectileType] = new List<Projectile>(poolSizePerType);

        var parentObject = new GameObject($"{projectileType.ToString()} Pool");
        parentObject.transform.parent = transform;
        projectileParents[projectileType] = parentObject;

        for (int i = 0; i < poolSizePerType; i++) {
            var projectile = Instantiate(prefab, parentObject.transform);
            projectile.id = idSequence++;
            projectile.gameObject.SetActive(false);
            projectile.gameObject.name = prefab.name + " (" + projectile.id + ")";

            projectileById[projectile.id] = projectile;
            projectilePool[projectileType].Add(projectile);
        }
    }

    public Projectile releaseProjectileFromPool(ProjectileType projectileType) {
        for (var i = 0; i < projectilePool[projectileType].Count; i++) {
            var projectile = projectilePool[projectileType][i];
            if (projectile.gameObject.activeInHierarchy) continue;
            return release(projectile);
        }

        throw new InvalidOperationException("No object could be found. It could be that all objects are used.");
    }

    public void returnToPool(Projectile projectile) {
        projectile.reset();
        projectile.transform.parent = projectileParents[projectile.type].transform;
        projectile.gameObject.SetActive(false);
    }

    public void returnToPool(int projectileId) {
        returnToPool(getProjectile(projectileId));
    }

    public void blowUp(Projectile projectile) {
        performBlowUp(projectile);
        simulateBlowUpServerRpc(projectile.id);
    }

    private void performBlowUp(Projectile projectile) {
        var explosiveForceEmitter = projectile.GetComponent<ExplosiveForceEmitter>();
        explosiveForceEmitter.blowUp();
        returnToPool(projectile);
    }

    [ServerRpc]
    private void simulateBlowUpServerRpc(int projectileId) {
        simulateBlowUpClientRpc(projectileId);
    }

    [ClientRpc]
    private void simulateBlowUpClientRpc(int projectileId) {
        if (TurnManager.INSTANCE.isLocalPlayersTurn()) return;

        var projectile = getProjectile(projectileId);

        ExplosionManager.INSTANCE.spawnExplosion(projectile.transform.position);
        returnToPool(projectile);
    }

    public Projectile getProjectile(int projectileId) {
        return projectileById[projectileId];
    }

    public Projectile releaseFromPool(int projectileId) {
        return release(getProjectile(projectileId));
    }

    private Projectile release(Projectile projectile) {
        projectile.transform.parent = null;
        projectile.gameObject.SetActive(true);
        return projectile;
    }

    public Projectile releaseProjectileFromPool(WeaponType projectileType) {
        return projectileType switch {
            WeaponType.GRENADE => releaseProjectileFromPool(ProjectileType.GRENADE),
            WeaponType.BAZOOKA => releaseProjectileFromPool(ProjectileType.ROCKET),
            WeaponType.RIFLE => releaseProjectileFromPool(ProjectileType.BULLET),
            WeaponType.NONE => null,
            _ => null
        };
    }
}