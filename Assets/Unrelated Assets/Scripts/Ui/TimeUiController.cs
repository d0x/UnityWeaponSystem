
using TMPro;
using UnityEngine;

public class TimeUiController : MonoBehaviour {
    public TextMeshProUGUI text;
    
    // Update is called once per frame
    void Update() {
        text.text = $"Time: {Time.time}";
    }
}
