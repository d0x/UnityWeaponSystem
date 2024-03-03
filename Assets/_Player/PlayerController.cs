using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour {
    [SerializeField] private WeaponType defaultWeapon = WeaponType.NONE;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 100.0f;
    [SerializeField] private Transform weaponAnchor;
    [SerializeField] private PlayerWeaponController playerWeaponController;
    [SerializeField] private List<Color> colors;

    public Renderer bodyRenderer;
    private Rigidbody rb;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn() {
        var clientId = (int)NetworkObject.OwnerClientId;
        bodyRenderer.material.color = colors[clientId];
        NetworkUtils.ReplaceCloneWithClientId(NetworkObject);

        if (IsServer) {
            TurnManager.INSTANCE.addPlayerController(this);
        }

        TurnManager.INSTANCE.turn.OnValueChanged += (_, _) => TurnManager_TurnChanged();
        TurnManager_TurnChanged();

        if (!IsOwner) {
            enabled = false;
            return;
        }

        WeaponUiController.INSTANCE.addPlayerController(this);

        var spawnPoint = SpawnManager.INSTANCE.GetSpawnPoint(clientId);
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        equip(defaultWeapon, false);
    }

    private void TurnManager_TurnChanged() {
        var isPlayerObjectsTurn = TurnManager.INSTANCE.isLocalPlayersTurn();

        enabled = isPlayerObjectsTurn;

        if (isPlayerObjectsTurn) {
            playerWeaponController.startTurn();
        }
        else {
            playerWeaponController.endTurn();
        }
    }

    void Update() {
        handleMovement();
        handleRotation();
        handleWeapon();
        handleEndTurn();
    }

    private void handleEndTurn() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            TurnManager.INSTANCE.endTurn();
        }
    }

    private void handleMovement() {
        var rotationHorizontal = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, rotationHorizontal * rotationSpeed * Time.deltaTime);

        if (isSpecialKeyDown()) {
            rb.velocity = Vector3.zero;
            return;
        }

        var acceleration = Input.GetAxis("Vertical");
        if (acceleration != 0) {
            rb.velocity = transform.forward * acceleration * speed;
        }
    }

    private void handleRotation() {
        if (!isSpecialKeyDown()) return;

        var weaponRotation = Input.GetAxis("Vertical");
        weaponAnchor.Rotate(Vector3.right, weaponRotation * -rotationSpeed * Time.deltaTime);
    }

    private bool isSpecialKeyDown() {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ||
               Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    private void handleWeapon() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            fire();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            equip(WeaponType.BAZOOKA);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            equip(WeaponType.GRENADE);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            equip(WeaponType.RIFLE);
        }
    }

    public void equip(WeaponType weaponType, bool checkCurrentTurn = true) {
        if (checkCurrentTurn && !TurnManager.INSTANCE.isLocalPlayersTurn()) return;

        playerWeaponController.equip(weaponType);
        WeaponUiController.INSTANCE.updateCurrentWeapon(weaponType);
    }

    public void fire() {
        if (!TurnManager.INSTANCE.isLocalPlayersTurn()) return;

        playerWeaponController.fire();
    }
}