using Meta.XR.MRUtilityKit;
using UnityEngine;
using System.Collections;
using System.Reflection;

public class Spawner : MonoBehaviour
{
    private IEnumerator WaitForRoomInitialization()
    {
        // Attendre que MRUK soit initialis�
        while (!MRUK.Instance || !MRUK.Instance.IsInitialized)
            yield return null;

        // Attendre que la room soit cr��e et qu'il y ait au moins une ancre d�tect�e
        MRUKRoom room = null;
        while (room == null || room.Anchors == null || room.Anchors.Count == 0)
        {
            room = MRUK.Instance.GetCurrentRoom();
            yield return null;
        }

        Debug.Log("[Spawner] Room et anchors initialis�s, lancement des scripts de spawn.");

        // Lancer toutes les m�thodes SpawnXXX des scripts attach�s au m�me GameObject
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
            if (script == this) continue; // Ignorer le script Spawner lui-m�me

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
