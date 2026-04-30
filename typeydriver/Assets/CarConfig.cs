using UnityEngine;

[CreateAssetMenu(menuName = "Cars/Car Config", fileName = "CarConfig")]
public class CarConfig : ScriptableObject
{
    [Header("Body")]
    public float mass = 1400f;
    public Vector3 centerOfMassOffset = new Vector3(0f, -0.4f, 0f);
    public float linearDrag = 0.05f;
    public float angularDrag = 2f;

    [Header("Suspension")]
    public float suspensionRestLength = 0.35f;
    public float suspensionMaxLength = 0.5f;
    public float wheelRadius = 0.35f;
    public float springStrength = 25000f;
    public float damperStrength = 3000f;

    [Header("Drive")]
    public float driveForce = 4000f;
    public float maxSpeed = 30f;

    [Header("Steering")]
    public float maxSteerAngle = 32f;
    public float highSpeedSteerAngle = 12f;
    public float steerFadeSpeed = 25f;
    public float steerResponse = 8f;

    [Header("Grip (Phase 1: simple lateral clamp)")]
    public float lateralGrip = 1500f;
    public float maxGrip = 9000f;
}
