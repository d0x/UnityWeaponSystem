using Unity.Netcode;

public class NetworkGameObjectNameFixer : NetworkBehaviour {
    public override void OnNetworkSpawn() {
        if (gameObject.name.Contains("(Clone)")) {
            gameObject.name = gameObject.name.Replace("(Clone)", $" (CID:{NetworkObject.OwnerClientId})");
        }
        else {
            gameObject.name += $" (CID:{NetworkObject.OwnerClientId})";
        }
    }
}