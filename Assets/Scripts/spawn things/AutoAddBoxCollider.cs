using UnityEngine;

public class AutoAddBoxCollider : MonoBehaviour
{
    void Start()
    {
        // Si aucun BoxCollider n'est d�j� pr�sent
        if (GetComponent<Collider>() == null)
        {
            MeshRenderer rend = GetComponentInChildren<MeshRenderer>();
            if (rend != null)
            {
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.center = rend.bounds.center - transform.position;
                box.size = rend.bounds.size;
                Debug.Log($"[AutoAddBoxCollider] Box ajout� � {name}");
            }
        }
    }
}
