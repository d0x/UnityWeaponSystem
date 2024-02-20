using Unity.Netcode;
using UnityEngine;

// TODO Should be NetworkBehaviour
public class Projectile : MonoBehaviour {
    [ServerRpc]
    public void activateServerRpc() {
        activateClientRpc();
    }

    [ClientRpc]
    private void activateClientRpc() {
        Destroy(gameObject, 3f);
    }
}