using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 5.0f;
    public GameObject prefab;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            print("appui sur la manette!");
            GameObject ball = Instantiate(prefab,transform.position, transform.rotation);
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            print(rb.name);
            if(rb != null)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
    }
}
