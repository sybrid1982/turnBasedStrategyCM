using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    private CinemachineImpulseSource cinemachineImpulseSouce;
    public static ScreenShake Instance { get; private set; }

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There's more than one UnitAction System! " + transform + "-" + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cinemachineImpulseSouce = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float intensity = 1f)
    {
        cinemachineImpulseSouce.GenerateImpulse(intensity);
    }
}
