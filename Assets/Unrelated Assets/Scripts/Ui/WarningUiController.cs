using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class WarningUiController : NetworkBehaviour {
    [SerializeField] private GameObject warning;
    private bool isVisible;
    private Vector3 originalScale;

    private void Start() {
        warning.SetActive(false);
        originalScale = warning.transform.localScale;
    }

    void Update() {
        if (!NetworkManager.Singleton.IsConnectedClient) return;

        var otherPlayersTurn = TurnManager.INSTANCE.turn.Value != NetworkManager.Singleton.LocalClientId;

        if (Input.anyKey && otherPlayersTurn) {
            showWarning();
        }
    }

    public void showWarning() {
        warning.SetActive(true);
        if (isVisible) return;


        StartCoroutine(WarningScaleAnimation(2.0f));
    }

    IEnumerator WarningScaleAnimation(float time) {
        isVisible = true;

        for (int i = 0; i < 3; i++) {
            yield return StartCoroutine(ScaleOverTime(warning, originalScale * 1.05f, time / 6));
            yield return StartCoroutine(ScaleOverTime(warning, originalScale, time / 6));
        }

        isVisible = false;
        warning.SetActive(false);
    }

    IEnumerator ScaleOverTime(GameObject targetObj, Vector3 targetScale, float duration) {
        float time = 0;
        Vector3 startScale = targetObj.transform.localScale;

        while (time < duration) {
            time += Time.deltaTime;
            targetObj.transform.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
            yield return null;
        }

        targetObj.transform.localScale = targetScale;
    }
}