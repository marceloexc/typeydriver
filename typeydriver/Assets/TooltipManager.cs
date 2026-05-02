using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI References")]
    public CanvasGroup tooltipCanvasGroup;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI bodyText;

    [Header("Settings")]
    public float fadeDuration = 0.5f;

    private HashSet<string> shownTooltips = new HashSet<string>();
    private Coroutine currentRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowTooltip(string id, string header, string body, float duration = 3f, bool waitForInput = false)
    {
        if (shownTooltips.Contains(id))
            return;

        shownTooltips.Add(id);

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(header, body, duration, waitForInput));
    }

    private IEnumerator ShowRoutine(string header, string body, float duration, bool waitForInput)
    {
        headerText.text = header;
        bodyText.text = body;

        yield return Fade(0f, 1f);

        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.anyKeyDown);
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float start, float end)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            tooltipCanvasGroup.alpha = Mathf.Lerp(start, end, time / fadeDuration);
            yield return null;
        }

        tooltipCanvasGroup.alpha = end;
    }
}