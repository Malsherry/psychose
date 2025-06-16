using Meta.XR.MRUtilityKit;
using UnityEngine;
using System.Collections;
using System.Reflection;

public class Spawner : MonoBehaviour
{
    private IEnumerator WaitForRoomInitialization()
    {
        // Attendre que MRUK soit initialisé
        while (!MRUK.Instance || !MRUK.Instance.IsInitialized)
            yield return null;

        // Attendre que la room soit créée et qu'il y ait au moins une ancre détectée
        MRUKRoom room = null;
        while (room == null || room.Anchors == null || room.Anchors.Count == 0)
        {
            room = MRUK.Instance.GetCurrentRoom();
            yield return null;
        }

        Debug.Log("[Spawner] Room et anchors initialisés, lancement des scripts de spawn.");

        // Lancer toutes les méthodes SpawnXXX des scripts attachés au même GameObject
        CallAllSpawnMethods();
    }

    void Start()
    {
        StartCoroutine(WaitForRoomInitialization());
    }

    private void CallAllSpawnMethods()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();

        foreach (var script in scripts)
        {
            if (script == this) continue; // Ignorer le script Spawner lui-même

            MethodInfo[] methods = script.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                if (method.Name.StartsWith("Spawn") && method.GetParameters().Length == 0)
                {
                    Debug.Log($"[Spawner] Appel de {method.Name} sur {script.GetType().Name}");
                    method.Invoke(script, null);
                }
            }
        }
    }
}
