using System;
using Unity.Netcode;
using UnityEngine;


public class Weapon : NetworkBehaviour {
    /**
     * true if projectiles should be spawned when the weapon
     * gets equipped. Like the rocket of a bazooka.
     */
    public Boolean spawnProjectile;

    public Transform projectileAnchor;
    
    public FollowTransform followTransform;
    
    public WeaponType type;

    public override void OnNetworkSpawn() {
       ProjectilePool.INSTANCE.addToPool(this);
    }
}