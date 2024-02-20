# A base project to implement a weapon system

This project is the base to get some help on fiverr.
It should be used to implement those two requirements:

1. Weapons and Projectiles should be `NetworkBehaviours` with `Client Network Transform`.
2. The Player having the active turn, should not have any lags / delay.

All existing physics and features should remain (like apply explosion force to players and objects).

You can have a look to this video: https://www.youtube.com/watch?v=-Y20iiZXzuU

# Overview
* Networking Solution: Netcode for Gameobjects
* Client-Host Architecture
* Client Authoritive
* About the current solution (doesn't meet Requirements)
  * Does De/-Activation of Weapon depending on a NetworkVariable (see: [PlayerWeaponController.cs](Assets%2F_Player%2FPlayerWeaponController.cs)).
  * Fire-Action is processed on host which leads into a delay client side.
  * Weapons ([Weapon.cs](Assets%2F_Weapons%2FWeapon.cs)) and Projectils ([Projectile.cs](Assets%2F_Projectils%2FProjectile.cs)) are MonoBehaviours (but shouldn't).

# Quick look
This screenshot shows, that the Client has a huge delay to the host.
But if it is his turn, he should not have any delay and be the source of truth.
![Overview.png](docs%2FOverview.png)

# Main parts to work with
* [Player**Weapon**Controller.cs](Assets%2F_Player%2FPlayerWeaponController.cs) This is the called from the UI and [PlayerController.cs](Assets%2F_Player%2FPlayerController.cs). 
* [Weapon.cs](Assets%2F_Weapons%2FWeapon.cs) - This should be a NetworkBehaviour with `Client Network Transform`.
* [Projectile.cs](Assets%2F_Projectils%2FProjectile.cs) - This should be a NetworkBehaviour with `Client Network Transform`.

# Other Information
* All unrelated stuff is grouped in [Unrelated Assets](Assets%2FUnrelated%20Assets).
* The [NetworkController.cs](Assets%2FUnrelated%20Assets%2FScripts%2FNetworkController.cs) has an `Auto Connect`-Option. If this is ticked, running in Play mode will start a host immediatly and running a `Build and Run` will connect a client.