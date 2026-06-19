using UnityEngine;

/// <summary>
/// Crea el Sol y el Planeta al cargar la escena. No requiere configuración manual en el Inspector.
/// </summary>
[DisallowMultipleComponent]
public class SimulationSetup : MonoBehaviour
{
    const float SunRadius = 1.5f;
    const float PlanetRadius = 0.35f;
    const float OrbitRadius = 8f;

    // Masas y G en unidades internas (μ = G·M = 1 → órbita estable y predecible).
    const float StarMass = 1f;
    const float PlanetMass = 1f;
    const float GravitationalConstant = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (FindAnyObjectByType<SimulationSetup>() != null)
            return;

        var simulationObject = new GameObject("Simulation");
        simulationObject.AddComponent<SimulationSetup>();
    }

    void Awake()
    {
        EnsureSun();
        EnsurePlanet();
    }

    void EnsureSun()
    {
        if (transform.Find("Sun") != null)
            return;

        var sunObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sunObject.name = "Sun";
        sunObject.transform.SetParent(transform, false);
        sunObject.transform.localPosition = Vector3.zero;
        sunObject.transform.localScale = Vector3.one * (SunRadius * 2f);
        Destroy(sunObject.GetComponent<Collider>());

        var renderer = sunObject.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = LoadMaterial("Sun", new Color(1f, 0.78f, 0.15f), emission: true);
    }

    void EnsurePlanet()
    {
        if (transform.Find("Planet") != null)
            return;

        var planetObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        planetObject.name = "Planet";
        planetObject.transform.SetParent(transform, false);
        planetObject.transform.localPosition = new Vector3(OrbitRadius, 0f, 0f);
        planetObject.transform.localScale = Vector3.one * (PlanetRadius * 2f);
        Destroy(planetObject.GetComponent<Collider>());

        var renderer = planetObject.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = LoadMaterial("Planet", new Color(0.22f, 0.52f, 0.92f), emission: false);

        var orbit = planetObject.AddComponent<NewtonianOrbit>();
        orbit.Configure(StarMass, PlanetMass, GravitationalConstant);

        float mu = GravitationalConstant * StarMass;
        float orbitalSpeed = NewtonianOrbit.CircularOrbitalSpeed(OrbitRadius, mu);

        // Posición en +X, velocidad tangencial en +Z → órbita circular en el plano XZ.
        orbit.Initialize(
            planetObject.transform.position,
            new Vector3(0f, 0f, orbitalSpeed));

        var trail = planetObject.AddComponent<TrailRenderer>();
        trail.time = 120f;
        trail.startWidth = 0.12f;
        trail.endWidth = 0.02f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = new Color(0.4f, 0.75f, 1f, 0.9f);
        trail.endColor = new Color(0.4f, 0.75f, 1f, 0.05f);
    }

    static Material LoadMaterial(string name, Color color, bool emission)
    {
        var material = Resources.Load<Material>($"Materials/{name}");
        if (material != null)
            return material;

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        material = new Material(shader);
        material.name = name;
        material.color = color;

        if (emission && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2f);
        }

        return material;
    }
}
