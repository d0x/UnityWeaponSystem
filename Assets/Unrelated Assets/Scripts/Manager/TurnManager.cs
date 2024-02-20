using System.Collections.Generic;
using Unity.Netcode;

public class TurnManager : NetworkBehaviour {
    public static TurnManager INSTANCE;

    // Current active Network Client Id
    public NetworkVariable<ulong> turn = new();
    private int turnIndex = 0;

    private List<PlayerController> players = new();

    private void Awake() {
        INSTANCE = this;
    }

    public override void OnNetworkSpawn() {
        if (IsServer) {
            nextTurnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void nextTurnServerRpc() {
        if (players.Count == 0) return;

        turnIndex = (turnIndex + 1) % players.Count;
        turn.Value = players[turnIndex].OwnerClientId;
    }

    public void addPlayerController(PlayerController playerController) {
        players.Add(playerController);
    }

    public void endTurn() {
        if (turn.Value == NetworkManager.LocalClientId) {
            nextTurnServerRpc();
        }
    }

    public bool isLocalPlayersTurn() {
        return NetworkManager.Singleton.IsConnectedClient && turn.Value == NetworkManager.LocalClientId;
    }
}