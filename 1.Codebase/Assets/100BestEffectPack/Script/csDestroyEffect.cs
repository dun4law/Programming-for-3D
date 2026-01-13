using UnityEngine;
using UnityEngine.InputSystem;

public class csDestroyEffect : MonoBehaviour {

    void Update () {
        var keyboard = Keyboard.current;
        if (keyboard != null &&
            (keyboard.xKey.wasPressedThisFrame || keyboard.zKey.wasPressedThisFrame || keyboard.cKey.wasPressedThisFrame))
        {
            Destroy(gameObject);
        }
    }
}
