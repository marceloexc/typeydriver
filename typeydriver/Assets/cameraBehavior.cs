using UnityEngine;

public class cameraBehavior : MonoBehaviour
{
    [Header("Targets")]
    public Transform target;              
    public Transform char1;

    [Header("Position Settings - Car")]
    public Vector3 carOffset = new Vector3(0f, 3f, -6f);
    public float followSmoothSpeed = 5f;

    [Header("Position Settings - Third Person")]
    public Vector3 thirdPersonOffset = new Vector3(2f, 1.5f, -3f);  // Over right shoulder
    public float thirdPersonSmoothSpeed = 5f;
    public float thirdPersonMouseSensitivity = 2f;
    public float thirdPersonDistance = 3f;  // Distance from character

    [Header("Lookahead Settings")]
    public float lookaheadAmount = 2f;    
    public float lookaheadSmooth = 5f;    // smoothing speed essentially
    public float maxSteerAngle = 30f;     

    [Header("Character Settings")]
    public float characterLookSmoothSpeed = 5f;
    public Animator characterAnimator;  // Animator component from character
    public Transform handIKTarget;  // IK target for aiming hand (from Blender rig)
    public Transform headBone;  // Head bone for FK rotation
    public float aimDistance = 10f;  // How far ahead to place aiming point

    private float currentLookahead = 0f;
    private float thirdPersonCameraY = 0f;  // Vertical angle around character
    private float thirdPersonCameraX = 0f;  // Horizontal angle around character
    public Rigidbody carRigidbody;
    public followTarget followTargetScript;
    public GameObject currentGameObject;
    public GameObject character;


    public bool canSwing = true;
   
    void LateUpdate()
    {
        if (target == null) return;
        
        if (followTargetScript == null || followTargetScript.target != null)
        {
            // Car camera
            HandleCarCamera();
        }
        else if (followTargetScript == null || followTargetScript.target == null)
        {
            // Third person over-the-shoulder camera
            HandleThirdPersonCamera();
        }
    }

    void HandleCarCamera()
    {
        // get steering input for lookahead
        float steer = canSwing ? GetSteeringValue() : 0f;

        // lookahead calc
        float targetLookahead = steer * lookaheadAmount; 

        // lookahead smoothing
        currentLookahead = Mathf.Lerp(currentLookahead, targetLookahead, Time.deltaTime * lookaheadSmooth);

        // position calc
        Vector3 rightOffset = target.right * currentLookahead;
        Vector3 desiredPosition = target.position + target.TransformDirection(carOffset) + rightOffset;

        // camera smoothing
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSmoothSpeed);

        // follow car
        Vector3 lookTarget = target.position
                   + target.forward * 5f
                   + target.right * currentLookahead * 0.5f
                   + Vector3.up * 1.5f;

        transform.LookAt(lookTarget);
    }

    void HandleThirdPersonCamera()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * thirdPersonMouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * thirdPersonMouseSensitivity;

        // Update camera rotation angles
        thirdPersonCameraX += mouseX;
        thirdPersonCameraY -= mouseY;
        thirdPersonCameraY = Mathf.Clamp(thirdPersonCameraY, -30f, 60f);  // Limit vertical look

        // Calculate rotated camera offset
        Vector3 baseOffset = new Vector3(2f, 1.5f, -3f);
        Quaternion cameraRotation = Quaternion.Euler(thirdPersonCameraY, thirdPersonCameraX, 0f);
        Vector3 rotatedOffset = cameraRotation * baseOffset;
        
        // Position camera by orbiting around character - NO smoothing
        transform.position = char1.position + rotatedOffset;
        
        // Camera looks outward/beyond the character
        transform.rotation = cameraRotation;

        // Make character body look towards camera's forward direction
        if (character != null)
        {
            Vector3 cameraForward = transform.forward;
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(cameraForward.x, 0f, cameraForward.z).normalized);
            character.transform.rotation = Quaternion.Lerp(character.transform.rotation, targetRotation, Time.deltaTime * characterLookSmoothSpeed);
        }
    
    }

    float GetSteeringValue()
    {
        // what do you think this does nigga
        float input = Input.GetAxisRaw("Horizontal");
        return input;
    }
}
    