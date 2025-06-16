using Meta.XR.MRUtilityKit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SpawnMenu : MonoBehaviour
{
    public static bool menu = true;
    public SpawnThings spawnThings; // à assigner dans l’inspecteur ou dynamiquement
    public GameObject wallPrefab; // Le prefab à instancier

    private IEnumerator Start()
    {

        if (menu) { 
            Debug.Log("SpawnMenu: Waiting for MRUK.Instance...");
            yield return new WaitUntil(() => MRUK.Instance != null && MRUK.Instance.IsInitialized);

            Debug.Log("SpawnMenu: Waiting for GetCurrentRoom() to return a valid room...");
            yield return new WaitUntil(() => MRUK.Instance.GetCurrentRoom() != null);

            Debug.Log("SpawnMenu: Room is ready, calling SpawnWallDecoration...");
            SpawnWallDecoration();
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
    public void SpawnWallDecoration()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("Menu: MRUKRoom introuvable !");
            return;
        }

        LabelFilter placementLabels = new LabelFilter(spawnThings.spawnLabelsWall);

        List<string> avoidTags = new List<string>
    {
        "Frame",
        "wall_avoid"
    };

        for (int i = 0; i < 2; i++)
        {
            Debug.Log($"Menu: Tentative de spawn de la décoration {i + 1}...");
            if (spawnThings.TrySpawnVerticalPrefab(
                wallPrefab,
                room,
                placementLabels,
                avoidTags,
                spawnThings.minEdgeDistance,
                spawnThings.wallOffset,
                1.5f, // vertical offset for wall frame
                spawnThings.maxWallAttempts,
                out GameObject decoration))
            {
                /*Animator animator = decoration.GetComponentInChildren<Animator>(true);
                if (animator != null)
                {
                    animator.enabled = true;
                    animator.gameObject.SetActive(true);
                }*/

                Debug.Log($"Menu: Décoration {i + 1} instanciée.");
            }
            else
            {
                Debug.LogWarning($"Menu: Échec pour la décoration {i + 1}.");
            }
        }
    }


}
