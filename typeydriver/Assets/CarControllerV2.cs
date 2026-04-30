using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarControllerV2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CarConfig config;
    [SerializeField] private WheelState[] wheels;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Gameplay Toggles")]
    public bool canMove = true;
    public bool autoDriveEnabled = false;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool logDiagnostics = true;
    private float diagTimer;

    private float throttleInput;
    private float steerInput;
    private float currentSteerAngle;

    public void EnableAutoDrive()
    {
        canMove = true;
        autoDriveEnabled = true;
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (config == null)
        {
            Debug.LogError("CarControllerV2: CarConfig is not assigned.", this);
            enabled = false;
            return;
        }

        rb.mass = config.mass;
        rb.centerOfMass = config.centerOfMassOffset;
        rb.drag = config.linearDrag;
        rb.angularDrag = config.angularDrag;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        foreach (WheelState wheel in wheels)
        {
            wheel.currentSuspensionLength = config.suspensionRestLength;
            wheel.previousSuspensionLength = config.suspensionRestLength;
        }
    }

    private void Update()
    {
        if (!canMove)
        {
            throttleInput = 0f;
            steerInput = 0f;
            return;
        }

        bool manualInput =
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f ||
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f;

        if (autoDriveEnabled && !manualInput)
        {
            throttleInput = 1f;
            steerInput = 0f;
            return;
        }

        if (manualInput) autoDriveEnabled = false;

        throttleInput = Input.GetAxisRaw("Vertical");
        steerInput = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate()
    {
        float speed = rb.velocity.magnitude;
        float targetSteer = GetTargetSteerAngle(steerInput, speed);
        currentSteerAngle = Mathf.MoveTowards(
            currentSteerAngle,
            targetSteer,
            config.steerResponse * 100f * Time.fixedDeltaTime);

        foreach (WheelState wheel in wheels)
        {
            if (wheel.steer) wheel.steerAngle = currentSteerAngle;
            else wheel.steerAngle = 0f;

            UpdateWheelContact(wheel);
            SimulateWheel(wheel);
            UpdateWheelVisual(wheel);
        }

        if (logDiagnostics)
        {
            diagTimer += Time.fixedDeltaTime;
            if (diagTimer >= 0.5f)
            {
                diagTimer = 0f;
                int groundedCount = 0;
                foreach (WheelState w in wheels) if (w.grounded) groundedCount++;
                Debug.Log(
                    $"[CarV2] canMove={canMove} autoDrive={autoDriveEnabled} " +
                    $"throttle={throttleInput:F2} steer={steerInput:F2} " +
                    $"speed={speed:F2} grounded={groundedCount}/{wheels.Length} " +
                    $"pos={transform.position} fwd={transform.forward}");
            }
        }
    }

    private void UpdateWheelContact(WheelState wheel)
    {
        wheel.previousSuspensionLength = wheel.currentSuspensionLength;

        float castLength = config.suspensionMaxLength + config.wheelRadius;
        Vector3 origin = wheel.hardpoint.position;
        Vector3 direction = -wheel.hardpoint.up;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, castLength, groundMask, QueryTriggerInteraction.Ignore))
        {
            wheel.grounded = true;
            wheel.hit = hit;
            wheel.currentSuspensionLength = hit.distance - config.wheelRadius;
        }
        else
        {
            wheel.grounded = false;
            wheel.currentSuspensionLength = config.suspensionMaxLength;
        }
    }

    private void SimulateWheel(WheelState wheel)
    {
        if (!wheel.grounded) return;

        float compression = config.suspensionRestLength - wheel.currentSuspensionLength;
        float springVelocity =
            (wheel.previousSuspensionLength - wheel.currentSuspensionLength) /
            Mathf.Max(Time.fixedDeltaTime, 0.0001f);

        float springForce = compression * config.springStrength;
        float damperForce = springVelocity * config.damperStrength;
        Vector3 suspensionForce = wheel.hardpoint.up * (springForce + damperForce);
        rb.AddForceAtPosition(suspensionForce, wheel.hit.point);

        Quaternion steerRot = Quaternion.AngleAxis(wheel.steerAngle, transform.up);
        Vector3 wheelForward = steerRot * transform.forward;
        Vector3 wheelRight = steerRot * transform.right;

        Vector3 pointVelocity = rb.GetPointVelocity(wheel.hit.point);
        float forwardSpeed = Vector3.Dot(pointVelocity, wheelForward);
        float lateralSpeed = Vector3.Dot(pointVelocity, wheelRight);

        float drive = 0f;
        if (wheel.drive && rb.velocity.magnitude < config.maxSpeed)
            drive = throttleInput * config.driveForce;

        float lateral = -lateralSpeed * config.lateralGrip;

        Vector2 desired = new Vector2(drive, lateral);
        Vector2 clamped = Vector2.ClampMagnitude(desired, config.maxGrip);

        Vector3 tireForce = wheelForward * clamped.x + wheelRight * clamped.y;
        rb.AddForceAtPosition(tireForce, wheel.hit.point);

        wheel.wheelAngularSpeed = forwardSpeed / Mathf.Max(config.wheelRadius, 0.01f);
    }

    private void UpdateWheelVisual(WheelState wheel)
    {
        if (wheel.visualWheel == null) return;

        Vector3 pos = wheel.grounded
            ? wheel.hit.point + wheel.hardpoint.up * config.wheelRadius
            : wheel.hardpoint.position - wheel.hardpoint.up * config.suspensionRestLength;

        wheel.visualWheel.position = pos;
        wheel.visualWheel.rotation =
            transform.rotation *
            Quaternion.AngleAxis(wheel.steerAngle, Vector3.up) *
            Quaternion.AngleAxis(wheel.wheelAngularSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime, Vector3.right) *
            wheel.visualWheel.localRotation;
    }

    private float GetTargetSteerAngle(float rawInput, float speed)
    {
        float t = Mathf.InverseLerp(0f, config.steerFadeSpeed, speed);
        float maxAngle = Mathf.Lerp(config.maxSteerAngle, config.highSpeedSteerAngle, t);
        return rawInput * maxAngle;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || wheels == null || config == null) return;

        Gizmos.color = Color.yellow;
        foreach (WheelState wheel in wheels)
        {
            if (wheel == null || wheel.hardpoint == null) continue;
            Vector3 start = wheel.hardpoint.position;
            Vector3 end = start - wheel.hardpoint.up * (config.suspensionMaxLength + config.wheelRadius);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, 0.05f);
        }
    }
}
