using System.Collections;
using UnityEngine;

public class menuSequencer : MonoBehaviour
{
    [Header("Menu UI")]
    public GameObject menuUIRoot;
    public CanvasGroup menuCanvasGroup;
    public GameObject letterTray;


    [Header("Cameras")]
    public Camera menuCamera;
    public GameObject gameplayCameraRoot;

    [Header("Transition")]
    public float fadeDuration = 1f;
    public float cameraMoveDuration = 1f;

    public CarControllerV2 carController;
    private bool isStarting;
    private Camera gameplayCamera;

    void Start()
    {
        if (menuCanvasGroup == null && menuUIRoot != null)
            menuCanvasGroup = menuUIRoot.GetComponent<CanvasGroup>();

        if (gameplayCameraRoot != null)
        {
            gameplayCamera = gameplayCameraRoot.GetComponentInChildren<Camera>();
            if (gameplayCamera != null)
                gameplayCamera.enabled = false;

            gameplayCameraRoot.SetActive(false);
        }

        if (menuCamera != null)
            menuCamera.tag = "MainCamera";
    }

    public void StartGame()
    {
        if (isStarting)
            return;

        isStarting = true;
        Debug.Log("StartGame called - starting transition");

        if (carController != null)
            carController.EnableAutoDrive();

        StartCoroutine(PlayStartTransition());
    }

    IEnumerator PlayStartTransition()
    {
        if (menuCamera == null || gameplayCameraRoot == null)
        {
            if (menuUIRoot != null)
                menuUIRoot.SetActive(false);

            if (gameplayCameraRoot != null)
                gameplayCameraRoot.SetActive(true);

            yield break;
        }

        float elapsed = 0f;
        float fadeTime = Mathf.Max(0.01f, fadeDuration);

        // fade out menu before moving camera
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float fadeT = Mathf.Clamp01(elapsed / fadeTime);

            if (menuCanvasGroup != null)
                menuCanvasGroup.alpha = 1f - fadeT;

            yield return null;
        }

        if (menuCanvasGroup != null)
            menuCanvasGroup.alpha = 0f;

        if (menuUIRoot != null)
            menuUIRoot.SetActive(false);

        if (gameplayCamera != null)
            gameplayCamera.enabled = false;

        gameplayCameraRoot.SetActive(true);
        StartCoroutine(showHud());

        Vector3 startPosition = menuCamera.transform.position;
        Quaternion startRotation = menuCamera.transform.rotation;

        float moveElapsed = 0f;
        float moveTime = Mathf.Max(0.01f, cameraMoveDuration);

        while (moveElapsed < moveTime)
        {
            moveElapsed += Time.deltaTime;
            float moveT = Mathf.Clamp01(moveElapsed / moveTime);

            Vector3 targetPosition = gameplayCameraRoot.transform.position;
            Quaternion targetRotation = gameplayCameraRoot.transform.rotation;

            menuCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, moveT);
            menuCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, moveT);

            yield return null;
        }

        if (menuCamera != null)
            menuCamera.gameObject.SetActive(false);

        if (gameplayCamera != null)
        {
            gameplayCamera.enabled = true;
            gameplayCamera.tag = "MainCamera";
        }
    }

    IEnumerator showHud()
    {
        letterTray.SetActive(true);
        yield break;
    }
}
