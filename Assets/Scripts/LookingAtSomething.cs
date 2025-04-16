using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LookingAtSomething : MonoBehaviour
{
    public string destroyableTags = "EyeCorner";
    //public Material visualMaterial; //pour instancier le matériau dans l'éditeur

    private GameObject visionCone;

    void Start()
    {
        // Rigidbody requis pour Trigger
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        // Crée un GameObject pour le champ de vision
        visionCone = new GameObject("VisionCone");
        visionCone.transform.parent = transform;
        visionCone.transform.localPosition = Vector3.zero;
        visionCone.transform.localRotation = Quaternion.identity;

        // Génère une pyramide ou cône de vision
        MeshFilter mf = visionCone.AddComponent<MeshFilter>();
        MeshRenderer mr = visionCone.AddComponent<MeshRenderer>();
        MeshCollider mc = visionCone.AddComponent<MeshCollider>();
        mc.convex = true;
        mc.isTrigger = true;

        mf.mesh = CreateVisionFrustum();
        visionCone.GetComponent<MeshRenderer>().enabled = false;
        //mr.material = visualMaterial; //pour voir le cône de vision
        mc.sharedMesh = mf.mesh;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(destroyableTags))
        {
            Debug.Log("Hallucination détectée et détruite : " + other.name);
            // Si le tag correspond, on détruit l'objet
            Destroy(other.gameObject);
        }
    }

    Mesh CreateVisionFrustum()
    {
        // Frustum/pyramide simplifiée pointant vers -Z
        Mesh mesh = new Mesh();

        float near = 0.1f;
        float far = 10f;
        float width = 3f;
        float height = 2f;

        Vector3[] vertices = new Vector3[8];

        // Near plane (plus petit rectangle)
        vertices[0] = new Vector3(-width * 0.2f, -height * 0.2f, near);
        vertices[1] = new Vector3(width * 0.2f, -height * 0.2f, near);
        vertices[2] = new Vector3(-width * 0.2f, height * 0.2f, near);
        vertices[3] = new Vector3(width * 0.2f, height * 0.2f, near);

        // Far plane (plus large rectangle)
        vertices[4] = new Vector3(-width * 0.8f, -height * 0.8f, far);
        vertices[5] = new Vector3(width * 0.8f, -height * 0.8f, far);
        vertices[6] = new Vector3(-width * 0.8f, height * 0.8f, far);
        vertices[7] = new Vector3(width * 0.8f, height * 0.8f, far);

        mesh.vertices = vertices;

        // Triangles du frustum
        mesh.triangles = new int[]
        {
            // Near face
            0, 2, 1, 1, 2, 3,
            // Far face
            4, 5, 6, 5, 7, 6,
            // Sides
            0, 1, 5, 0, 5, 4,
            1, 3, 7, 1, 7, 5,
            3, 2, 6, 3, 6, 7,
            2, 0, 4, 2, 4, 6
        };

        mesh.RecalculateNormals();
        return mesh;
    }
}
