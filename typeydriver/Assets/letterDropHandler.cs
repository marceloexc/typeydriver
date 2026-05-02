using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class letterDropHandler : MonoBehaviour
{
    private char droppedLetter;

    [SerializeField] public TextMeshPro textMesh;

    public void SetLetter(char letter)
    {
        droppedLetter = letter;
        textMesh.text = letter.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (typeHandler.Instance != null)
            {
                typeHandler.Instance.AddLetter(droppedLetter);
            }
            else
            {
                Debug.LogWarning("typeHandler instance not found!");
            }

            Destroy(gameObject);
        }
    }
}