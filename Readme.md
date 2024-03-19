# Turn-Based Networked Weapon System Reference Project

This GitHub repository serves as a reference project for developing a turn-based,
networked weapon system with an emphasis on client authority.
The primary goal is to achieve a 100% client-authoritative model,
ensuring a lag-free experience for the player taking their turn.

> This system is designed so that all network objects are transferred to the active player,
> who interacts with the game as if in a single-player mode,
> while other participants essentially observe the actions without direct interaction.

See https://www.youtube.com/watch?v=-Y20iiZXzuU ,

to understand the initial lag challenges this project overcame. 
The video was originally intended for finding help on Fiverr.
Despite early setbacks, the repository now achieves my goal of zero delay
for the active player, laying the foundation for a smooth gaming experience.

### Key Features:

- **Networking Solution:** Utilizes Netcode for GameObjects to handle network operations.
- **Client-Host Architecture:** Employs a client-host setup to manage game states and interactions.
- **Client Authority:** Designed to be fully client-authoritative, eliminating delays for the active player and ensuring
  a seamless gameplay experience.
- See [TurnManager.cs](Assets%2FUnrelated%20Assets%2FScripts%2FManager%2FTurnManager.cs).transferOwnership(ulong
  activeClientId) and be suprised how easy this solution is.
- Major events (such as firing a weapon, explosions, etc.) are calculated instantly on the active client, 
and then a simulation event is distributed via `ServerRpc` to other clients. 
Often, a call to `XXX` is divided into `performXXX` (on the active player's side) 
and `simulateXXX` (on the observing player's side).

### Overview of the Current Implementation:

- **Networking Solution:** Focus on Netcode for GameObjects.
- **Client-Host Architecture:** Ensures that one client (the active player) takes precedence in gameplay decisions,
  minimizing latency and promoting a responsive game environment. Note that it will be very easy to cheat with this concept!

### Visual Overview:

The included screenshot illustrates the significant delay experienced by the client relative to the host, an issue this
project aims to eliminate for the active player's turn.
![Overview.png](docs%2FOverview.png)

### Additional Notes:

- Unrelated assets and scripts are organized under [Unrelated Assets](Assets%2FUnrelated%20Assets).
- The [NetworkController.cs](Assets%2FUnrelated%20Assets%2FScripts%2FNetworkController.cs) script features
  an `Auto Connect` option for simplified hosting and client connection during development.

This repository lays the groundwork for a client-authoritative,
turn-based networked weapon system,
providing a solid foundation for further development and refinement.