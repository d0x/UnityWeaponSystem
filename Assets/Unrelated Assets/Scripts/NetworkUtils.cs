using Unity.Netcode;

public class NetworkUtils {
    public static void ReplaceCloneWithClientId(NetworkObject networkObject) {
        var gameObject = networkObject.gameObject;

        if (gameObject.name.Contains("(Clone)")) {
            gameObject.name = gameObject.name.Replace("(Clone)", $" (CID:{networkObject.OwnerClientId})");
        }
        else {
            gameObject.name += $" (CID:{networkObject.OwnerClientId})";
        }
    }
}