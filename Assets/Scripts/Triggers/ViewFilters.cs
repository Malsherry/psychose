    using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.UI;

public class ViewFilters : MonoBehaviour
{
    private int layerMask;
    public string targetTag = "FilterTarget";
    public float gazeTimeRequired = 2f;
    private float gazeTimer = 0f;
    private Transform lastHitTransform = null;

    private Coroutine currentFilterCoroutine = null;
    private bool isFiltering = false;

    public static bool isActive = true; // Indique si le filtre est actif ou non
    public Material filterMaterial; // Matériau du filtre   
    public float distanceFromCamera = 0.5f; // Distance entre la caméra et le plane
    public float yOffset = -0.05f; // Décalage vertical
    public float fadeDuration = 2.0f; // Durée du fondu (en secondes)
    public float minInterval = 20f; // délai min entre effets
    public float maxInterval = 120f; // délai max entre effets


    public AudioClip filterSound; // Son à jouer pendant le filtre
    private AudioSource audioSource;

    private GameObject filterPlane;     // Plane pour le filtre
    private Camera mainCamera;         // Référence à la caméra principale
    private Material runtimeMaterial;   // Matériau instancié pour le filtre

    private void Start()
    {
        layerMask = LayerMask.GetMask("Interractable");
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

        // Ajouter un AudioSource au filtre
        audioSource = filterPlane.AddComponent<AudioSource>();
        audioSource.clip = filterSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // Son 2D (non spatial)
        audioSource.volume = 0f; // Commence silencieux
        audioSource.Play();

        // Adapter la taille
        UpdateFilterSize();

        // Démarrer le cycle des filtres
        //StartCoroutine(FilterCycle());
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
    private IEnumerator TriggerFilterEffect()
    {
        isFiltering = true;

        Debug.Log("Filter effect triggered!");
        yield return FadeTo(0.7f);

        float elapsed = 0f;
        float duration = 5f;

        // Pendant l'affichage du filtre, on vérifie que l'utilisateur continue de regarder
        while (elapsed < duration)
        {
            // Si le regard est perdu, on interrompt l'effet
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, 10f, layerMask) || hit.transform != lastHitTransform)
            {
                Debug.Log("Gaze lost, cancelling filter.");
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return FadeTo(0f);

        // Réinitialisation
        gazeTimer = 0f;
        lastHitTransform = null;
        isFiltering = false;
        currentFilterCoroutine = null;
    }


    private IEnumerator FadeTo(float targetAlpha)
    {
        if (runtimeMaterial == null || audioSource == null)
            yield break;

        Color color = runtimeMaterial.color;
        float startAlpha = color.a;
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            // Changer alpha
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            runtimeMaterial.color = color;

            // Changer volume (lié à alpha)
            audioSource.volume = Mathf.Lerp(startVolume, targetAlpha, t);

            yield return null;
        }

        color.a = targetAlpha;
        runtimeMaterial.color = color;
        audioSource.volume = targetAlpha;
    }

    private void Update()
    {
        if (!isActive || mainCamera == null)
            return;
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10f, layerMask))
        {
            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red);
            Debug.Log("Hit: " + hit.transform.name + " on layer " + hit.transform.gameObject.layer);


            if (hit.transform.CompareTag(targetTag))
            {
                Debug.Log("Filter hitting something interesting!");
                if (lastHitTransform == hit.transform)
                {
                    gazeTimer += Time.deltaTime;
                }
                else
                {
                    lastHitTransform = hit.transform;
                    gazeTimer = 0f;
                }

                if (gazeTimer >= gazeTimeRequired)
                {
                    if (!isFiltering)
                    {
                        currentFilterCoroutine = StartCoroutine(TriggerFilterEffect());
                    }

                    gazeTimer = -Mathf.Infinity; // Pour éviter de relancer tant que l'effet est en cours
                }
            }
            else
            {
                lastHitTransform = null;
                gazeTimer = 0f;
            }
        }
        else
        {
            lastHitTransform = null;
            gazeTimer = 0f;
        }
    }


}
