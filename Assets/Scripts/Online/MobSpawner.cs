using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class MobSpawner : NetworkBehaviour
{
    [Header("Spawner Settings")]
    public List<GameObject> monsterPrefabs; // Liste de prefabs de monstres
    public float spawnDelay = 2f; // Délai entre chaque spawn
    public float spawnRadius = 5f; // Rayon de la zone de spawn
    public int maxMonsters = 10; // Nombre maximum de monstres

    private int currentMonsterCount = 0; // Nombre actuel de monstres

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        StartCoroutine(SpawnMonsters());
    }

    // Coroutine pour gérer le spawn des monstres avec un délai
    IEnumerator SpawnMonsters()
    {
        yield return new WaitForSeconds(10);


        while (currentMonsterCount < maxMonsters)
        {
            yield return new WaitForSeconds(spawnDelay); // Attendre le délai spécifié

            // Sélectionner un prefab de monstre au hasard
            int monsterIndex = Random.Range(0, monsterPrefabs.Count);
            GameObject monsterPrefab = monsterPrefabs[monsterIndex];

            // Générer une position de spawn aléatoire dans un cercle
            Vector2 spawnPosition = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnLocation = new Vector3(spawnPosition.x, 0, spawnPosition.y) + transform.position;

            // Générer une rotation aléatoire
            float randomYRotation = Random.Range(0f, 360f);
            Quaternion spawnRotation = Quaternion.Euler(0, randomYRotation, 0);

            // Instancier le monstre à la position générée
            GameObject spawnedMonster = Instantiate(monsterPrefab, spawnLocation, spawnRotation, transform.parent.parent);

            // Définir une taille aléatoire entre 0.8 et 1.2
            float randomScale = Random.Range(0.8f, 1.2f);
            spawnedMonster.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

            NetworkServer.Spawn(spawnedMonster);

            SetMonsterParentRpc(spawnedMonster);

            // Incrémenter le compteur de monstres actuels
            currentMonsterCount++;
        }
    }

    [ClientRpc]
    void SetMonsterParentRpc(GameObject monster)
    {
        if (monster == null)
        {
            return;
        }

        monster.transform.SetParent(transform.parent.parent);

        if (isClient && !isServer)
        {
            //monster.transform.position += monster.transform.parent.parent.position;
        }
    }
}
