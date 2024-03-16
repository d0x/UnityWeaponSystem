using System;
using UnityEngine;


public class Weapon : MonoBehaviour {
    /**
     * true if projectiles should be spawned when the weapon
     * gets equipped. Like the rocket of a bazooka.
     */
    public Boolean spawnProjectile;

    public Projectile projectilePrefab;
    public Transform projectileAnchor;
}