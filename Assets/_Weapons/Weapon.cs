using System;
using UnityEngine;


// TODO Should be NetworkBehaviour 
public class Weapon : MonoBehaviour {
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
}