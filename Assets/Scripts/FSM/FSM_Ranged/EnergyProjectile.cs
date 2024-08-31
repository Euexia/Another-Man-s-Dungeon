using UnityEngine;

public class EnergyProjectile : MonoBehaviour
{
    private Vector3 startPosition;
    public float speed = 10f;
    public float maxDistance = 20f;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
}
