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
    public EnemySpawner spawner;

    void Start()
    {
        parentObj = transform.parent != null ? transform.parent.gameObject : gameObject;

    }

    void Update()
    {
        if (!isDead && hitPoints <= 0)
        {
            isDead = true;
            Transform screenLetter = parentObj.transform.Find("Armature/Bone/Cube.001/Text (TMP)");
            GameObject screenLetterObj = screenLetter.gameObject;
            Destroy(screenLetterObj);
            TooltipManager.Instance.ShowTooltip(
            "enemy_death",
            "Collecting Letters",
            "Collect dropped letters by touching them.",
            5f
            );
            Destroy(parentObj, 0.5f);
            SpawnLetterDrop();
            SpawnAnother();
        }
    }

    void SpawnLetterDrop()
    {
        enemyController enemy = parentObj.GetComponent<enemyController>();
        if (enemy == null || letterDropPrefab == null) return;
        GameObject drop = Instantiate(letterDropPrefab, parentObj.transform.position + new Vector3(0f, 2f, 0f), Quaternion.identity);
        letterDropHandler dropHandler = drop.GetComponent<letterDropHandler>();
        if (dropHandler != null)
        {
            dropHandler.SetLetter(enemy.currentLetter);
        }
    }

    void SpawnAnother()
    {
        if (spawner != null)
        {
            spawner.OnEnemyKilled();
        }
    }
}
