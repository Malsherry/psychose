using UnityEngine;
using UnityEngine.AI;

public class HallucinationNaviguator : MonoBehaviour
{
    public NavMeshAgent agent;
    public float speed;
    public float wanderRadius = 5f; // Rayon de déplacement aléatoire

    void Start()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        agent.speed = speed;
        SetNewDestination();
    }

    void Update()
    {
        if (agent.isOnNavMesh)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                SetNewDestination();
            }

            // Corriger la direction en inversant l'axe Z
            if (agent.desiredVelocity.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-agent.desiredVelocity.normalized); // Ajout du '-'
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
    }



    void SetNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection.y = 0.2f;
        randomDirection += transform.position; // Ajoute à la position actuelle pour rester dans une zone proche

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            if(agent.isOnNavMesh) agent.SetDestination(hit.position);
        }
    }
}
