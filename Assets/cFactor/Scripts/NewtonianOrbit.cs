using UnityEngine;

/// <summary>
/// Simula la órbita de un planeta alrededor de una estrella aplicando las leyes de Newton.
/// Unidades internas: G en unidades del mundo³/(kg·s²), masas en kg arbitrarios, tiempo en segundos.
/// </summary>
[DisallowMultipleComponent]
public class NewtonianOrbit : MonoBehaviour
{
    [Header("Cuerpos")]
    [SerializeField] Vector3 centralPosition = Vector3.zero;
    [SerializeField] float centralMass = 1f;
    [SerializeField] float planetMass = 1f;

    [Header("Constantes")]
    [SerializeField] float gravitationalConstant = 1f;
    [SerializeField] float minDistance = 0.05f;
    [SerializeField] float timeScale = 5f;

    Vector3 position;
    Vector3 velocity;
    bool initialized;

    /// <summary>Parámetro gravitacional μ = G·M (útil para órbitas circulares).</summary>
    public float GravitationalParameter => gravitationalConstant * centralMass;

    public Vector3 Position => position;
    public Vector3 Velocity => velocity;

    public void Configure(float starMass, float bodyMass, float gravConstant)
    {
        centralMass = starMass;
        planetMass = bodyMass;
        gravitationalConstant = gravConstant;
    }

    public void Initialize(Vector3 startPosition, Vector3 startVelocity)
    {
        position = startPosition;
        velocity = startVelocity;
        transform.position = position;
        initialized = true;
    }

    /// <summary>Velocidad tangencial para una órbita circular a la distancia dada.</summary>
    public static float CircularOrbitalSpeed(float distance, float gravitationalParameter)
    {
        return Mathf.Sqrt(gravitationalParameter / distance);
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

        // Ley 1 (inercia): sin fuerza neta, v y r permanecen constantes.
        // Ley 2 (F = ma): la aceleración se obtiene de la fuerza gravitatoria.
        IntegrateRK4(dt);

        transform.position = position;
    }

    /// <summary>
    /// Integración RK4 del sistema ẋ = v, v̇ = a(x).
    /// Mantiene estabilidad numérica en órbitas elípticas prolongadas.
    /// </summary>
    void IntegrateRK4(float dt)
    {
        Vector3 k1v = AccelerationFromForce(position);
        Vector3 k1p = velocity;

        Vector3 k2v = AccelerationFromForce(position + k1p * (dt * 0.5f));
        Vector3 k2p = velocity + k1v * (dt * 0.5f);

        Vector3 k3v = AccelerationFromForce(position + k2p * (dt * 0.5f));
        Vector3 k3p = velocity + k2v * (dt * 0.5f);

        Vector3 k4v = AccelerationFromForce(position + k3p * dt);
        Vector3 k4p = velocity + k3v * dt;

        velocity += (k1v + 2f * k2v + 2f * k3v + k4v) * (dt / 6f);
        position += (k1p + 2f * k2p + 2f * k3p + k4p) * (dt / 6f);
    }

    /// <summary>
    /// Ley de gravitación universal (Newton): F = G·M·m / r², dirigida hacia la estrella.
    /// </summary>
    Vector3 GravitationalForceOnPlanet(Vector3 planetPosition)
    {
        Vector3 toStar = centralPosition - planetPosition;
        float distance = toStar.magnitude;

        if (distance < minDistance)
            return Vector3.zero;

        float forceMagnitude = gravitationalConstant * centralMass * planetMass
                               / (distance * distance);

        return forceMagnitude * (toStar / distance);
    }

    /// <summary>
    /// Segunda ley de Newton: a = F / m.
    /// La masa del planeta cancela en órbitas keplerianas (a = G·M / r²), pero se mantiene
    /// explícita para coherencia física y futuras extensiones multi-cuerpo.
    /// </summary>
    Vector3 AccelerationFromForce(Vector3 planetPosition)
    {
        Vector3 force = GravitationalForceOnPlanet(planetPosition);
        return force / planetMass;
    }

    /// <summary>
    /// Tercera ley de Newton: la estrella recibe F_star = −F_planet.
    /// Aquí la estrella se trata como fija (M_estrella ≫ m_planeta).
    /// </summary>
    public Vector3 GravitationalForceOnStar(Vector3 planetPosition)
    {
        return -GravitationalForceOnPlanet(planetPosition);
    }
}
