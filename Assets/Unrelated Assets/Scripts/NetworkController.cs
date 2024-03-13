using System;
#if UNITY_EDITOR
using ParrelSync;
#endif
using TMPro;
using Unity.Netcode;
using UnityEngine;

/**
 * Manages the Network by starting host or client
 */
public class NetworkController : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI status;
    [SerializeField] private GameObject hostButton;
    [SerializeField] private GameObject clientButton;

    // if true,
    // for unity playmode a host is started automatically
    // for a normal build a client is started automatically
    [SerializeField] private Boolean autoConnect = false;


    public void StartHost() {
        NetworkManager.Singleton.StartHost();
        status.text = "Host";
        hostButton.SetActive(false);
        clientButton.SetActive(false);
    }

    public void StartClient() {
        NetworkManager.Singleton.StartClient();
        status.text = "Client";
        hostButton.SetActive(false);
        clientButton.SetActive(false);
    }

    private void Start() {
        if (autoConnect) {
#if UNITY_EDITOR
            if (ClonesManager.IsClone()) {
                StartClient();
            }
            else {
                StartHost();
            }
#else
            StartClient();
#endif
        }
    }
}