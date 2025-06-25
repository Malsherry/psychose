using System.Collections;
using UnityEngine;

public class CameraBlink : MonoBehaviour
{
    public string blinkingObjectName = "clignotant";
    public string blinkingMaterialName = "red_light";

    private Material blinkingMat;
    private float timer = 0f;
    private bool isRed = true;

    private static readonly Color onColor = new Color(1.0f, 0.1f, 0.0f);
    private static readonly Color onEmission = new Color(1.0f, 0.1f, 0.0f) * 2.0f;
    private static readonly Color offColor = Color.black;
    private static readonly Color offEmission = Color.black;

    void Start()
    {
        var blinkingObj = transform.Find(blinkingObjectName);
        if (blinkingObj != null)
        {
            var renderer = blinkingObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat.name.StartsWith(blinkingMaterialName))
                    {
                        blinkingMat = mat;
                        break;
                    }
                }
            }
        }

        if (blinkingMat == null)
            Debug.LogWarning("Matériau 'red_light' non trouvé sur l'objet clignotant.");

        // Start rotation coroutine
        StartCoroutine(RotateZEvery20Seconds());
    }

    void Update()
    {
        if (blinkingMat == null) return;

        timer += Time.deltaTime;
        if (timer >= 2f)
        {
            timer = 0f;
            isRed = !isRed;

            if (isRed)
            {
                blinkingMat.color = onColor;
                blinkingMat.SetColor("_EmissionColor", onEmission);
            }
            else
            {
                blinkingMat.color = offColor;
                blinkingMat.SetColor("_EmissionColor", offEmission);
            }
        }
    }

    /// <summary>
    /// Coroutine that performs a smooth 360-degree rotation around the Z axis every 20 seconds.
    /// </summary>
    private IEnumerator RotateZEvery20Seconds()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);
            yield return StartCoroutine(SmoothZRotation(4f)); // Rotate over 1 second (adjust as needed)
        }
    }

    /// <summary>
    /// Smoothly rotates the object 360 degrees around the local Z axis over duration seconds.
    /// </summary>
    private IEnumerator SmoothZRotation(float duration)
    {//corriger axe de spin
        float totalRotation = 0f;
        float rotationSpeed = 360f / duration;

        while (totalRotation < 360f)
        {
            float step = rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, step, 0f , Space.Self);
            totalRotation += step;
            yield return null;
        }

        // Fix any overshoot
        float overshoot = totalRotation - 360f;
        if (overshoot > 0f)
            transform.Rotate(0f, -overshoot, 0f, Space.Self);
    }

}
