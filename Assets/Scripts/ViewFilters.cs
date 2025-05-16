    using UnityEngine;
using System.Collections;

public class ViewFilters : MonoBehaviour
{

    public static bool isActive = true; // Indique si le filtre est actif ou non
    public Material filterMaterial; // Matériau du filtre   
    public float distanceFromCamera = 0.5f; // Distance entre la caméra et le plane
    public float yOffset = -0.05f; // Décalage vertical
    public float fadeDuration = 2.0f; // Durée du fondu (en secondes)
    public float minInterval = 20f; // délai min entre effets
    public float maxInterval = 120f; // délai max entre effets

    private GameObject filterPlane;     // Plane pour le filtre
    private Camera mainCamera;         // Référence à la caméra principale
    private Material runtimeMaterial;   // Matériau instancié pour le filtre

    private void Start()
    {
        mainCamera = Camera.main; // vérifie si la caméra principale existe
        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found.");
            return;
        }
        if (!isActive)
        {
            return; // Si le filtre n'est pas actif, on ne fait rien
        }

        // Créer le plane   
        filterPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        filterPlane.name = "ViewFilterPlane";

        // Supprimer collider qui sert a rien dans notre cas
        Destroy(filterPlane.GetComponent<Collider>());

        // Dupliquer le matériau pour pouvoir le modifier sans affecter l'original
        if (filterMaterial != null)
        {
            runtimeMaterial = new Material(filterMaterial);
            filterPlane.GetComponent<MeshRenderer>().material = runtimeMaterial;
        }
        else
        {
            Debug.LogWarning("No filter material assigned.");
        }

        // Parenté à la caméra
        filterPlane.transform.SetParent(mainCamera.transform);


        // Position devant la caméra
        filterPlane.transform.localPosition = new Vector3(0, yOffset, distanceFromCamera);
        filterPlane.transform.localRotation = Quaternion.identity;

        // Adapter la taille
        UpdateFilterSize();

        // Démarrer le cycle des filtres
        StartCoroutine(FilterCycle());
    }

    private void UpdateFilterSize()
    {
        float height = 2.0f * distanceFromCamera * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * mainCamera.aspect;

        // Petite marge en hauteur pour éviter les trous
        filterPlane.transform.localScale = new Vector3(width, height * 1.1f, 1);
    }

    private IEnumerator FilterCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            yield return FadeTo(0.7f); // Apparition
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            yield return FadeTo(0f); // Disparition

        }
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (runtimeMaterial == null)
            yield break;

        Color color = runtimeMaterial.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            runtimeMaterial.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        runtimeMaterial.color = color;
    }
}
