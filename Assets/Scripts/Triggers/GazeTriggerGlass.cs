using UnityEngine;
using System.Collections;

public class GazeTriggerGlass : MonoBehaviour
{
    private int layerMask;
    public string targetTag = "glass";
    public float gazeTimeRequired = 2f;
    public float delayBeforeAnimation = 5f;
    public float cooldownTime = 15f;
    public string animationName = "GlassAction";
    public float delayAfterLookAway = 3f;

    private float gazeTimer = 0f;
    private Transform currentTarget = null;
    private bool isActivated = false;
    private bool animationPlayed = false;
    private Camera mainCamera;
    private Coroutine lookAwayRoutine;

    private void Start()
    {
        mainCamera = Camera.main;
        layerMask = LayerMask.GetMask("Interractable");

        if (mainCamera == null)
            Debug.LogError("No Main Camera found.");

        Debug.Log("GazeTriggerGlass initialized with targetTag: " + targetTag);
    }

    private void Update()
    {
        if (mainCamera == null)
            return;

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.green, 0.1f);

        if (Physics.Raycast(ray, out hit, 10f, layerMask))
        {
            if (hit.transform.CompareTag(targetTag))
            {
                if (!isActivated)
                {
                    if (hit.transform == currentTarget)
                    {
                        gazeTimer += Time.deltaTime;
                        //Debug.Log("GazeTrigger: " + gazeTimer + " on: " + hit.transform.name);

                        if (gazeTimer >= gazeTimeRequired)
                        {
                            //Debug.Log("GazeTriggerGlass activated on: " + hit.transform.name);
                            StartCoroutine(HandleGlassAnimation(hit.transform));
                            isActivated = true;
                            animationPlayed = false;
                        }
                    }
                    else
                    {
                        currentTarget = hit.transform;
                        gazeTimer = 0f;
                    }
                }

                // Si on re-regarde l'objet, annule le retour � frame 1
                if (lookAwayRoutine != null)
                {
                    StopCoroutine(lookAwayRoutine);
                    lookAwayRoutine = null;
                }
                return;
            }
        }

        // Gaze is not on the object anymore
        if (currentTarget != null)
        {
            gazeTimer = 0f;

            if (isActivated && animationPlayed && lookAwayRoutine == null)
            {
                lookAwayRoutine = StartCoroutine(ResetToFrame1AfterDelay(currentTarget));
            }

            // Reset pour prochaine d�tection
            if (!isActivated)
            {
                currentTarget = null;
            }
        }
    }

    private IEnumerator HandleGlassAnimation(Transform target)
    {
        yield return new WaitForSeconds(delayBeforeAnimation);

        Animator anim = target.GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = true; // Si d�sactiv� � la frame 1, le r�activer ici
            anim.Play(animationName, 0, 0f);
            anim.Update(0f);
            float clipLength = anim.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(clipLength);
        }

        animationPlayed = true;
        yield return new WaitForSeconds(cooldownTime);
        isActivated = false;
    }


    private IEnumerator ResetToFrame1AfterDelay(Transform target)
    {
        yield return new WaitForSeconds(delayAfterLookAway);

        Animator anim = target.GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play(animationName, 0, 0f); // Va � la frame 0
            anim.Update(0f);                 // Force l��valuation � cette frame
            anim.enabled = false;           // Stoppe l�Animator ici

            Debug.Log("GazeTriggerGlass reset to frame 1 and paused animation on: " + target.name);
        }

        // R�initialiser les flags
        currentTarget = null;
        gazeTimer = 0f;
        lookAwayRoutine = null;
        animationPlayed = false;
    }

}
