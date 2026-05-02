using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform camTransform;

    void Start()
    {
        
    }

    void LateUpdate()
    {
        // Option A: Look directly at the camera (can cause tilting)
        // transform.LookAt(camTransform);

        // Option B: Match camera rotation (keeps text flat to the screen)
        transform.rotation = camTransform.rotation;
    }
}
