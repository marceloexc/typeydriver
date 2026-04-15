using UnityEngine;

public class cameraBehavior : MonoBehaviour
{
    [Header("Targets")]
    public Transform target;              
    public Transform char1;

    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0f, 3f, -6f);
    public float followSmoothSpeed = 5f;

    [Header("Lookahead Settings")]
    public float lookaheadAmount = 2f;    
    public float lookaheadSmooth = 5f;    // smoothing speed essentially
    public float maxSteerAngle = 30f;     

    [Header("First Person Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookUpAngle = 90f;
    public float maxLookDownAngle = 90f;
    public float headHeightOffset = 1.6f;  

    private float currentLookahead = 0f;
    private float xRotation = 0f;
    public Rigidbody carRigidbody;
    public followTarget followTargetScript;
    public GameObject currentGameObject;
    public GameObject arms;
    public GameObject character;


    public bool canSwing = true;
   
    void LateUpdate()
    {
        if (target == null) return;
        
        if (followTargetScript == null || followTargetScript.target != null)
        {
        arms.SetActive(false);

        // get steering input for lookahead
        float steer = canSwing ? GetSteeringValue() : 0f;

        // lookahead calc
        float targetLookahead = steer * lookaheadAmount; 

        // lookahead smoothing
        currentLookahead = Mathf.Lerp(currentLookahead, targetLookahead, Time.deltaTime * lookaheadSmooth);

        // position calculation
        Vector3 rightOffset = target.right * currentLookahead;
        Vector3 desiredPosition = target.position + target.TransformDirection(offset) + rightOffset;

        // camera smoothing
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSmoothSpeed);

        // follow car
        Vector3 lookTarget = target.position
                   + target.forward * 5f
                   + target.right * currentLookahead * 0.5f
                   + Vector3.up * 1.5f;

        transform.LookAt(lookTarget);
        }
        else if (followTargetScript == null || followTargetScript.target == null)
        {
            // switch to first person on dismount
            Vector3 firstPersonPos = char1.position 
                                   + Vector3.up * headHeightOffset 
                                   + character.transform.forward * 0.5f;
            currentGameObject.transform.position = firstPersonPos;
            arms.SetActive(true);
            HandleFirstPersonInput();
        }
    }

    void HandleFirstPersonInput()
    {

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // rotate around y
        transform.Rotate(Vector3.up * mouseX);

        // rotate around x w/ clamping
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookDownAngle, maxLookUpAngle);

        // apply rotation
        transform.localRotation = Quaternion.Euler(xRotation, transform.localEulerAngles.y, 0f);

        // rotate character to face the same direction as camera
        if (character != null)
        {
            character.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }
    }

    float GetSteeringValue()
    {
        // what do you think this does nigga
        float input = Input.GetAxisRaw("Horizontal");
        return input;
    }
}