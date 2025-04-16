using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Meta.XR.MRUtilityKit;
using System.Collections;
public class RuntimeNavmeshBuilder : MonoBehaviour
{
    private NavMeshSurface navmeshSurface;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("RuntimeNavmeshBuilder started!");
        navmeshSurface = GetComponent<NavMeshSurface>();
        MRUK.Instance.RegisterSceneLoadedCallback(BuildNavmesh);
      
        /*GameObject floor = GameObject.Find("WalkableFloor");
        if (floor != null)
        {
            floor.transform.position = new Vector3(floor.transform.position.x, 0, floor.transform.position.z);
            Debug.Log("Repositioning floor to ground level.");
        }*/
      
    }

    public void BuildNavmesh()
    {
        StartCoroutine(BuildNavmeshRoutine());
    }
    public IEnumerator BuildNavmeshRoutine()
    {
        yield return new WaitForSeconds(2f); // Attendre un peu plus
        Debug.Log("Building NavMesh at position: " + navmeshSurface.transform.position);
        navmeshSurface.BuildNavMesh();
        Debug.Log("NavMeshSurface Position: " + navmeshSurface.transform.position);

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
