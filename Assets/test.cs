using UnityEngine;

public class HierarchyDebugger : MonoBehaviour
{
    void Start()
    {
        Transform current = transform;
        while (current != null)
        {
            Debug.Log($"[HierarchyDebugger] {current.name} | ActiveSelf: {current.gameObject.activeSelf} | ActiveInHierarchy: {current.gameObject.activeInHierarchy}");
            current = current.parent;
        }
    }
}
