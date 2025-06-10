using UnityEngine;

public class CameraBlink : MonoBehaviour
{
    public string blinkingObjectName = "clignotant"; // Nom du GameObject enfant à clignoter
    public string blinkingMaterialName = "red_light"; // Nom du matériau à modifier

    private Material blinkingMat;
    private float timer = 0f;
    private bool isRed = true;

    // Couleur rouge-orangée avec émission (adapte si besoin)
    private static readonly Color onColor = new Color(1.0f, 0.1f, 0.0f); // rouge orangé
    private static readonly Color onEmission = new Color(1.0f, 0.1f, 0.0f) * 2.0f; // emission boostée
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
}
