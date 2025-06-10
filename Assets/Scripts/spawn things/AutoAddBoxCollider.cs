using UnityEngine;

public class AutoAddBoxCollider : MonoBehaviour
{
    void Start()
    {
        // Si aucun BoxCollider n'est déjà présent
        if (GetComponent<Collider>() == null)
        {
            MeshRenderer rend = GetComponentInChildren<MeshRenderer>();
            if (rend != null)
            {
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.center = rend.bounds.center - transform.position;
                box.size = rend.bounds.size;
                Debug.Log($"[AutoAddBoxCollider] Box ajouté à {name}");
            }
        }
    }
}
