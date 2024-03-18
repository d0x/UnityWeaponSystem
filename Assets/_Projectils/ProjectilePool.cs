using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour {
    public static ProjectilePool INSTANCE;

    [SerializeField] private int poolSizePerType;

    [SerializeField] private Projectile bulletPrefab;
    [SerializeField] private Projectile clusterPartPrefab;
    [SerializeField] private Projectile grenadePrefab;
    [SerializeField] private Projectile rocketPrefab;

    private Dictionary<ProjectileType, List<Projectile>> projectilePool = new();
    private Dictionary<int, Projectile> projectileById = new();

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

    public Projectile release(int projectileId) {
        return release(get(projectileId));
    }

    private Projectile release(Projectile projectile) {
        if (projectile.gameObject.activeSelf) {
            Debug.LogWarning($"{GetType().logName()}: release got called on an already active object. Id: {projectile.id} - Name: {projectile.gameObject.name}");
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

    public void returnToPool(Projectile projectile) {
        projectile.reset();
        projectile.transform.parent = projectileParents[projectile.type].transform;
        projectile.gameObject.SetActive(false);
    }

    public void returnToPool(int projectileId) {
        returnToPool(get(projectileId));
    }

    public Projectile get(int projectileId) {
        return projectileById[projectileId];
    }
}