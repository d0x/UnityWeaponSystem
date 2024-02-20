using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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
        FollowTargetsCameraManager.INSTANCE.targets.Add(transform);
        TurnManager.INSTANCE.addPlayerController(this);
        TurnManager.INSTANCE.turn.OnValueChanged += TurnManager_TurnChanged;
        WeaponUiController.INSTANCE.addPlayerController(this);

        var clientId = (int)NetworkObject.OwnerClientId;

        if (IsOwner) {
            var spawnPoint = SpawnManager.INSTANCE.GetSpawnPoint(clientId);
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            equip(defaultWeapon);
        }

        bodyRenderer.material.color = colors[clientId];
    }

    private void TurnManager_TurnChanged(ulong previousValue, ulong newValue) {
        var isPlayerObjectsTurn = OwnerClientId == newValue;

        if (isPlayerObjectsTurn) {
            playerWeaponController.startTurn();
        }
        else {
            playerWeaponController.endTurn();
        }
    }

    void Update() {
        if (!IsOwner || !TurnManager.INSTANCE.isLocalPlayersTurn()) return;

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
        if (!IsOwner) return;

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

    public void equip(WeaponType weaponType) {
        if (!TurnManager.INSTANCE.isLocalPlayersTurn()) return;

        playerWeaponController.equip(weaponType);
        WeaponUiController.INSTANCE.updateCurrentWeapon(weaponType);
    }

    public void fire() {
        if (!TurnManager.INSTANCE.isLocalPlayersTurn()) return;

        playerWeaponController.fire();
    }
}