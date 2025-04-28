using UnityEngine;
using System.Collections;

public class ViewFilters : MonoBehaviour
{
    public Material filterMaterial; // Ton matériau rouge (avec transparence possible)
    public float distanceFromCamera = 0.5f;
    public float yOffset = -0.05f; // Légère baisse du filtre
    public float fadeDuration = 2.0f; // Durée du fondu (en secondes)
    public float minInterval = 20f; // Délai min entre effets
    public float maxInterval = 120f; // Délai max entre effets

    private GameObject filterPlane;
    private Camera mainCamera;
    private Material runtimeMaterial;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found.");
            return;
        }

        // Créer le plane
        filterPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        filterPlane.name = "ViewFilterPlane";

        // Supprimer collider
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
