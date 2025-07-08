    using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.UI;
using System.Collections.Generic;

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
    public AudioClip secondarySound; // New sound to play after 1 second

    private AudioSource audioSource;
    private AudioSource secondaryAudioSource;

    [Header("Mesh de surimpression")]
    public GameObject overlayMeshPrefab;
    private GameObject activeOverlayMesh;

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
        SoundManager.Instance?.RegisterPrioritySoundStart();
        audioSource.Play();
        // Quand tu joues un son prioritaire
        StartCoroutine(EndSoundAfter(audioSource.clip.length));




        secondaryAudioSource = filterPlane.AddComponent<AudioSource>();
        secondaryAudioSource.clip = secondarySound;
        secondaryAudioSource.loop = false;
        secondaryAudioSource.playOnAwake = false;
        secondaryAudioSource.spatialBlend = 0f;
        secondaryAudioSource.volume = 1f;

        // Adapter la taille
        UpdateFilterSize();

        // Démarrer le cycle des filtres
        //StartCoroutine(FilterCycle());
    }
    private IEnumerator EndSoundAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        SoundManager.Instance?.RegisterPrioritySoundEnd();
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
    private IEnumerator FadeInOverlayMesh(GameObject obj, float duration)
    {
        if (obj == null) yield break;

        // Récupérer tous les MeshRenderer et SkinnedMeshRenderer enfants (et sur obj)
        var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>(true);
        var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        var allRenderers = new List<Renderer>();
        allRenderers.AddRange(meshRenderers);
        allRenderers.AddRange(skinnedMeshRenderers);

        if (allRenderers.Count == 0) yield break;

        // Valeurs d'alpha en [0..1]
        float alphaMin = 20f / 255f;   // ~0.08
        float alphaMax = 100f / 255f;  // ~0.39

        float halfDuration = duration / 2f;
        float time = 0f;

        // Initialiser alpha à alphaMin sur tous les mats
        foreach (var rend in allRenderers)
        {
            Material mat = rend.material;
            Color c = mat.color;
            c.a = alphaMin;
            mat.color = c;
        }

        // Phase 1 : alphaMin -> alphaMax
        while (time < halfDuration)
        {
            time += Time.deltaTime;
            float t = time / halfDuration;

            foreach (var rend in allRenderers)
            {
                Material mat = rend.material;
                Color c = mat.color;
                c.a = Mathf.Lerp(alphaMin, alphaMax, t);
                mat.color = c;
            }
            yield return null;
        }

        time = 0f;

        // Phase 2 : alphaMax -> alphaMin
        while (time < halfDuration)
        {
            time += Time.deltaTime;
            float t = time / halfDuration;

            foreach (var rend in allRenderers)
            {
                Material mat = rend.material;
                Color c = mat.color;
                c.a = Mathf.Lerp(alphaMax, alphaMin, t);
                mat.color = c;
            }
            yield return null;
        }

        // S'assurer qu'on finit bien à alphaMin
        foreach (var rend in allRenderers)
        {
            Material mat = rend.material;
            Color c = mat.color;
            c.a = alphaMin;
            mat.color = c;
        }
    }



    private IEnumerator TriggerFilterEffect()
    {
        isFiltering = true;

        Debug.Log("Filter effect triggered!");
        yield return FadeTo(0.7f);

        // Afficher un mesh par-dessus le mesh déclencheur
        if (overlayMeshPrefab != null && lastHitTransform != null)
        {
            activeOverlayMesh = Instantiate(overlayMeshPrefab);

            activeOverlayMesh.transform.SetParent(null);

            activeOverlayMesh.transform.position = lastHitTransform.position;
            activeOverlayMesh.transform.rotation = lastHitTransform.rotation;
            // TEST : change ici l'axe selon ton modèle
            Vector3 localOffset = new Vector3(0, 0, 4f);
            Debug.Log("mesh of the local offste:: " + activeOverlayMesh);
            activeOverlayMesh.transform.position += activeOverlayMesh.transform.TransformVector(localOffset);

            activeOverlayMesh.transform.localScale = Vector3.one * 0.2f;

            activeOverlayMesh.transform.SetParent(lastHitTransform, worldPositionStays: true);

            AlignChildren(activeOverlayMesh.transform, activeOverlayMesh.transform.rotation);

            Debug.Log("FadeInOverlayMesh started");
            StartCoroutine(FadeInOverlayMesh(activeOverlayMesh, fadeDuration));
        }





        float elapsed = 0f;
        float duration = 5f;

        while (elapsed < duration)
        {
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, 10f, layerMask) || hit.transform != lastHitTransform)
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Supprimer le mesh une fois le filtre terminé
        if (activeOverlayMesh != null)
        {
            Destroy(activeOverlayMesh);
            activeOverlayMesh = null;
        }

        yield return FadeTo(0f);

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

        // Start coroutine to delay and play the second sound
        if (targetAlpha > 0f && secondaryAudioSource != null && secondarySound != null)
        {
            StartCoroutine(PlaySecondarySoundWithDelay(1f));
        }

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            // Alpha and volume changes
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            runtimeMaterial.color = color;

            audioSource.volume = Mathf.Lerp(startVolume, targetAlpha, t);

            yield return null;
        }

        color.a = targetAlpha;
        runtimeMaterial.color = color;
        audioSource.volume = targetAlpha;
    }
    private IEnumerator PlaySecondarySoundWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (secondaryAudioSource != null && secondarySound != null)
        {
            Debug.Log("Playing secondary sound after delay.");
            secondaryAudioSource.Play();
        }
    }
    private void AlignChildren(Transform root, Quaternion rotation)
    {
        foreach (Transform child in root)
        {
            child.rotation = rotation;
            AlignChildren(child, rotation); // récursif pour sous-enfants
        }
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
           // Debug.Log("Hit: " + hit.transform.name + " on layer " + hit.transform.gameObject.layer);


            if (hit.transform.CompareTag(targetTag))
            {
               // Debug.Log("Filter hitting something interesting!");
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
