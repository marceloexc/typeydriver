using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using Random = UnityEngine.Random;


public class typeHandler : MonoBehaviour
{
    public TMP_Text inputStringDisplay;
    public TMP_Text targetWordDisplayText;
    public GameObject panel;
    public followTarget followTargetScript;
    public SimpleRigidbodyCar carController;
    public cameraBehavior cameraBehavior;
    public gunHandler gunHandlerScript;
    public TMP_Text inNoticeText;
    public GameObject inNotice;


    //instantiate for letter drop updationating.
    public static typeHandler Instance;

        void Awake()
    {
        Instance = this;
    }

    bool isDamaged = false;
    string inputString = string.Empty;
    string targetWord = string.Empty;
    string shotType = "pistol";

    string[] wordBank = new string[] { "door", "trunk", "fender", "window", "tire", "hood", "mirror", "grill" };
    string[] shotBank = new string[] { "pistol", "scatter", "gatling", "rocket", "beam" };
    string[] letters = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
    int[] quantities = new int[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };
    public TMP_Text[] letterTexts = new TMP_Text[26];
    public TMP_Text[] quantityTexts = new TMP_Text[26];
    const int maxQuantity = 5;
    const float letterOutlineWidth = 0.2f;
    const float letterOutlineMaxAlpha = 0.5f;
    const float shakeDuration = 0.25f;
    const float shakeAmplitude = 4f;
    const float shakeFrequency = 30f;

    Dictionary<char, int> letterIndexMap;
    RectTransform[] letterRects;
    Vector2[] letterRestPositions;
    Coroutine[] shakeCoroutines;

    void Start()
    {
        InitializeLetterIndexMap();
        UpdateInputDisplay();
        UpdateTargetWordDisplay();
        UpdateQuantityUI();
        CacheLetterShakeState();
    }

    void CacheLetterShakeState()
    {
        letterRects = new RectTransform[letterTexts.Length];
        letterRestPositions = new Vector2[letterTexts.Length];
        shakeCoroutines = new Coroutine[letterTexts.Length];
        for (int i = 0; i < letterTexts.Length; i++)
        {
            if (letterTexts[i] == null) continue;
            letterRects[i] = letterTexts[i].rectTransform;
            letterRestPositions[i] = letterRects[i].anchoredPosition;
        }
    }

    void ShakeLetter(int index)
    {
        if (letterRects == null || index < 0 || index >= letterRects.Length) return;
        if (letterRects[index] == null) return;
        if (shakeCoroutines[index] != null) StopCoroutine(shakeCoroutines[index]);
        shakeCoroutines[index] = StartCoroutine(ShakeRoutine(index));
    }

    IEnumerator ShakeRoutine(int index)
    {
        RectTransform rt = letterRects[index];
        Vector2 origin = letterRestPositions[index];
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float damp = 1f - (elapsed / shakeDuration);
            float offset = Mathf.Sin(elapsed * shakeFrequency * 2f * Mathf.PI) * shakeAmplitude * damp;
            rt.anchoredPosition = origin + new Vector2(offset, 0f);
            yield return null;
        }
        rt.anchoredPosition = origin;
        shakeCoroutines[index] = null;
    }

    void Update()
    {
        bool isInCar = followTargetScript != null && followTargetScript.target != null;


        // handle panel hide toggle
        if (isInCar){
            if (Input.GetKeyDown(KeyCode.Tab))
            {

                Debug.Log("togle");
                panel.SetActive(!panel.activeSelf);
                                InputLock.IsTyping = panel.activeSelf;

                TooltipManager.Instance.ShowTooltip(
                "enemy_death",
                "Using Letters",
                "Type using collected letters to repair your car and change ammo type.",
                5f
                );
                StartCoroutine(WaitForTip(6f, "ammo_types", "Ammo Types", "If you have spare letters, try typing: scatter, rocket, gatling, or beam.", 10f, false));
            }
        }

        bool shouldShowPanel = isInCar && panel.activeSelf;
        Debug.Log("isInCar: " + isInCar);

        // disable car movement and camera swing when panel is active
        if (carController != null)
        {
            carController.canMove = !shouldShowPanel;
        }

        if (cameraBehavior != null)
        {
            cameraBehavior.canSwing = !shouldShowPanel;
        }

        if (!isInCar || !panel.activeSelf)
        {
            return;
        }

        HandleDebugToggle();

        if (isDamaged && string.IsNullOrEmpty(targetWord))
        {
            SetRandomTargetWord();
        }

        HandleTypingInput();
    }

    void HandleDebugToggle()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Debug.Log(isDamaged);
            ToggleDamageState();
        }
    }

    void ToggleDamageState()
    {
        isDamaged = !isDamaged;

        if (isDamaged)
        {
            SetRandomTargetWord();
        }
        else
        {
            ClearTargetWord();
        }
    }

    void SetRandomTargetWord()
    {
        targetWord = wordBank[Random.Range(0, wordBank.Length)];
        UpdateTargetWordDisplay();
    }

    void ClearTargetWord()
    {
        targetWord = string.Empty;
        UpdateTargetWordDisplay();
    }

    void HandleTypingInput()
    {
        string typedInput = Input.inputString;
        bool hasChanged = false;

        foreach (char c in typedInput)
        {
            if (c == '\b')
            {
                HandleBackspace();
                UpdateQuantityUI();
                hasChanged = true;
            }
            else if (c == '\n' || c == '\r')
            {
                HandleSubmit();
                UpdateQuantityUI();
                hasChanged = true;
            }
            else
            {
                bool typed = TryTypeLetter(c);
                UpdateQuantityUI();
                hasChanged |= typed;
            }
        }

        if (hasChanged)
        {
            UpdateInputDisplay();
            UpdateQuantityUI();
            inNotice.SetActive(false);
        }
    }

    bool TryTypeLetter(char inputChar)
    {
        char lower = char.ToLower(inputChar);

        if (lower < 'a' || lower > 'z')
        {
            return false;
        }

        if (!letterIndexMap.TryGetValue(lower, out int letterIndex))
        {
            return false;
        }

        if (quantities[letterIndex] <= 0)
        {
            Debug.Log("insufficient quantity of " + lower);
            inNotice.SetActive(true);
            inNoticeText.text = "INSUFFICIENT " + char.ToUpper(lower);
            return false;
        }

        inputString += lower;
        quantities[letterIndex]--;
        ShakeLetter(letterIndex);
        return true;
    }

    void HandleBackspace()
    {
        if (inputString.Length == 0)
        {
            return;
        }

        char removed = inputString[inputString.Length - 1];
        inputString = inputString.Substring(0, inputString.Length - 1);

        if (letterIndexMap.TryGetValue(removed, out int letterIndex))
        {
            quantities[letterIndex]++;
        }
    }

    void HandleSubmit()
    {
        if (string.IsNullOrEmpty(inputString))
        {
            return;
        }

        string matchedShotType = GetShotTypeForInput();

        if (!string.IsNullOrEmpty(matchedShotType))
        {
            shotType = matchedShotType;
            GunType currentShotType = (GunType)Enum.Parse(typeof(GunType), shotType);
            gunHandlerScript.currentGun = currentShotType;
            Debug.Log("Shot type set to " + shotType);
            ClearInputAndShotType();
            return;
        }

        if (isDamaged)
        {
            if (inputString == targetWord)
            {
                isDamaged = false;
                Debug.Log("Repair successful: " + targetWord);
                ClearTargetWord();
                ClearInputAndShotType();
            }
            else
            {
                Debug.Log("Incorrect repair code. Returning letters.");
                ReturnInputLetters();
                ClearInputAndShotType();
            }

            return;
        }

        Debug.Log("Word does not match a shot or repair target. Returning letters.");
        ReturnInputLetters();
        ClearInputAndShotType();
    }

    void ReturnInputLetters()
    {
        foreach (char c in inputString)
        {
            if (letterIndexMap.TryGetValue(c, out int letterIndex))
            {
                quantities[letterIndex]++;
            }
        }
    }

    void ClearInputAndShotType()
    {
        ClearInput();
        shotType = string.Empty;
    }

    void ClearInput()
    {
        inputString = string.Empty;
        UpdateInputDisplay();
    }

    string GetShotTypeForInput()
    {
        foreach (string shot in shotBank)
        {
            if (inputString == shot)
            {
                return shot;
            }
        }

        return string.Empty;
    }

    void UpdateInputDisplay()
    {
        if (inputStringDisplay != null)
        {
            inputStringDisplay.text = inputString;
        }
    }

    void UpdateTargetWordDisplay()
    {
        if (targetWordDisplayText != null)
        {
            targetWordDisplayText.text = targetWord;
        }
    }

    void InitializeLetterIndexMap()
    {
        letterIndexMap = new Dictionary<char, int>();

        for (int i = 0; i < letters.Length; i++)
        {
            if (!string.IsNullOrEmpty(letters[i]) && letters[i].Length == 1)
            {
                char letter = letters[i][0];
                letterIndexMap[letter] = i;
            }
        }
    }

    void UpdateQuantityUI()
    {
        for (int i = 0; i < quantities.Length; i++)
        {
            float ratio = Mathf.Clamp01((float)quantities[i] / maxQuantity);

            if (letterTexts[i] != null)
            {
                letterTexts[i].color = quantities[i] <= 0
                    ? new Color(1f, 0f, 0f, 0.1f)
                    : new Color(1f, 1f, 1f, ratio);

                Material mat = letterTexts[i].fontMaterial;
                mat.EnableKeyword("OUTLINE_ON");
                float outlineAlpha = (1f - ratio) * letterOutlineMaxAlpha;
                mat.SetColor(ShaderUtilities.ID_OutlineColor, new Color(1f, 1f, 1f, outlineAlpha));
                mat.SetFloat(ShaderUtilities.ID_OutlineWidth, letterOutlineWidth);
                letterTexts[i].UpdateMeshPadding();
            }

            if (quantityTexts[i] != null)
            {
                quantityTexts[i].text = quantities[i].ToString();
            }
        }
    }

    public void AddLetter(char letter)
    {
        letter = char.ToLower(letter);
        if (letterIndexMap.TryGetValue(letter, out int index))
        {
            quantities[index]++;
            UpdateQuantityUI();
        }
    }

            IEnumerator WaitForTip(float time, string tag, string head, string body, float fadeWait, bool inputWait)
    {
        yield return new WaitForSeconds(time);
            TooltipManager.Instance.ShowTooltip(
            tag,
            head,
            body,
            fadeWait,
            inputWait
            );
    }
}
