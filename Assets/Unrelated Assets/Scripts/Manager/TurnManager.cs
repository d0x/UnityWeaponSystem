using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour {
    public static TurnManager INSTANCE;

    // Current active Network Client Id
    public NetworkVariable<ulong> turn = new();
    private int turnIndex = 0;

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
        var players = NetworkManager.Singleton.ConnectedClientsList.Select(client => client.ClientId).ToList();
        if (players.Count == 0) return;

        turnIndex = (turnIndex + 1) % players.Count;
        turn.Value = players[turnIndex];

        transferOwnership(turn.Value);
    }

    private void transferOwnership(ulong activeClientId) {
        if (!IsServer) return;

        FindObjectsByType<NetworkObject>(FindObjectsSortMode.None)
            .Where(n => n.IsSpawned)
            .ToList()
            .ForEach(o => o.ChangeOwnership(activeClientId));
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