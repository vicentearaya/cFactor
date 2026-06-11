using UnityEngine;

/// <summary>
/// Integración RK4 de una órbita kepleriana alrededor de una masa central puntual.
/// Unidades: G·M = 1, distancias en unidades del mundo, tiempo en segundos.
/// </summary>
[DisallowMultipleComponent]
public class NewtonianOrbit : MonoBehaviour
{
    [SerializeField] Vector3 centralPosition = Vector3.zero;
    [SerializeField] float gravitationalParameter = 1f;
    [SerializeField] float minDistance = 0.05f;
    [SerializeField] float timeScale = 5f;

    Vector3 position;
    Vector3 velocity;
    bool initialized;

    public void Initialize(Vector3 startPosition, Vector3 startVelocity)
    {
        position = startPosition;
        velocity = startVelocity;
        transform.position = position;
        initialized = true;
    }

    void Start()
    {
        if (!initialized)
        {
            position = transform.position;
            velocity = Vector3.zero;
            initialized = true;
        }

        var trail = GetComponent<TrailRenderer>();
        if (trail != null)
            trail.Clear();
    }

    void FixedUpdate()
    {
        if (!initialized)
            return;

        float dt = Time.fixedDeltaTime * timeScale;
        IntegrateRK4(dt);
        transform.position = position;
    }

    void IntegrateRK4(float dt)
    {
        Vector3 k1v = Acceleration(position);
        Vector3 k1p = velocity;

        Vector3 k2v = Acceleration(position + k1p * (dt * 0.5f));
        Vector3 k2p = velocity + k1v * (dt * 0.5f);

        Vector3 k3v = Acceleration(position + k2p * (dt * 0.5f));
        Vector3 k3p = velocity + k2v * (dt * 0.5f);

        Vector3 k4v = Acceleration(position + k3p * dt);
        Vector3 k4p = velocity + k3v * dt;

        velocity += (k1v + 2f * k2v + 2f * k3v + k4v) * (dt / 6f);
        position += (k1p + 2f * k2p + 2f * k3p + k4p) * (dt / 6f);
    }

    Vector3 Acceleration(Vector3 pos)
    {
        Vector3 r = pos - centralPosition;
        float dist = r.magnitude;
        if (dist < minDistance)
            return Vector3.zero;

        return -gravitationalParameter / (dist * dist * dist) * r;
    }
}
