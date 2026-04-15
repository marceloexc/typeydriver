using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class typeHandler : MonoBehaviour
{
    public TMP_Text inputStringDisplay;
    public TMP_Text targetWordDisplayText;
    public GameObject panel;
    public followTarget followTargetScript;
    public SimpleRigidbodyCar carController;
    public cameraBehavior cameraBehavior;

    bool isDamaged = false;
    bool isPanelHidden = false;
    string inputString = string.Empty;
    string targetWord = string.Empty;
    string shotType = string.Empty;

    string[] wordBank = new string[] { "door", "trunk", "fender", "window", "tire", "hood", "mirror", "grill" };
    string[] shotBank = new string[] { "pistol", "scatter", "gatling", "rocket", "beam" };
    string[] letters = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
    int[] quantities = new int[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };

    Dictionary<char, int> letterIndexMap;

    void Start()
    {
        InitializeLetterIndexMap();
        UpdateInputDisplay();
        UpdateTargetWordDisplay();
    }

    void Update()
    {
        bool isInCar = followTargetScript != null && followTargetScript.target != null;

        // Handle panel hide toggle
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            isPanelHidden = !isPanelHidden;
        }

        bool shouldShowPanel = isInCar && !isPanelHidden;

        if (panel != null)
        {
            panel.SetActive(shouldShowPanel);
        }

        // Disable car movement and camera swing when panel is active
        if (carController != null)
        {
            carController.canMove = !shouldShowPanel;
        }

        if (cameraBehavior != null)
        {
            cameraBehavior.canSwing = !shouldShowPanel;
        }

        if (!isInCar || isPanelHidden)
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
                hasChanged = true;
            }
            else if (c == '\n' || c == '\r')
            {
                HandleSubmit();
                hasChanged = true;
            }
            else
            {
                bool typed = TryTypeLetter(c);
                hasChanged |= typed;
            }
        }

        if (hasChanged)
        {
            UpdateInputDisplay();
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
            return false;
        }

        inputString += lower;
        quantities[letterIndex]--;
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
}
