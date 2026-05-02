using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthHandler : MonoBehaviour
{
    public float hitPoints;
    public bool isPlayer;

    private GameObject parentObj;
    private bool isDead = false;

    public GameObject letterDropPrefab;

    void Start()
    {
        parentObj = transform.parent != null ? transform.parent.gameObject : gameObject;
    }

    void Update()
    {
        if (!isDead && hitPoints <= 0)
        {
            isDead = true;
            SpawnLetterDrop();
            TooltipManager.Instance.ShowTooltip(
            "enemy_death",
            "you killed him",
            "dude what the hell",
            5f
            );
            Destroy(parentObj, 0.01f);
        }
    }

    void SpawnLetterDrop()
    {
        enemyController enemy = parentObj.GetComponent<enemyController>();
        if (enemy == null || letterDropPrefab == null) return;
        GameObject drop = Instantiate(letterDropPrefab, parentObj.transform.position, Quaternion.identity);
        letterDropHandler dropHandler = drop.GetComponent<letterDropHandler>();
        if (dropHandler != null)
        {
            dropHandler.SetLetter(enemy.currentLetter);
        }
    }
}
