using UnityEngine;

[System.Serializable]
public class WheelState
{
    public string name = "Wheel";
    public Transform hardpoint;
    public Transform visualWheel;
    public bool steer;
    public bool drive;

    [HideInInspector] public bool grounded;
    [HideInInspector] public RaycastHit hit;
    [HideInInspector] public float currentSuspensionLength;
    [HideInInspector] public float previousSuspensionLength;
    [HideInInspector] public float steerAngle;
    [HideInInspector] public float wheelAngularSpeed;
}
