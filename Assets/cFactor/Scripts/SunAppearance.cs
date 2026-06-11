using UnityEngine;

/// <summary>
/// Mejoras visuales del Sol (corona, luz, material). No modifica la simulación orbital.
/// </summary>
public static class SunAppearance
{
    const float CoronaScale = 1.32f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void ScheduleEnhancement()
    {
        var runner = new GameObject("SunAppearanceRunner");
        runner.hideFlags = HideFlags.HideAndDontSave;
        runner.AddComponent<SunAppearanceRunner>();
    }

    sealed class SunAppearanceRunner : MonoBehaviour
    {
        float elapsed;

        void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            if (elapsed < 0.05f)
                return;

            var simulation = GameObject.Find("Simulation");
            if (simulation == null)
            {
                if (elapsed > 3f)
                    Destroy(gameObject);
                return;
            }

            var sun = simulation.transform.Find("Sun");
            if (sun == null)
            {
                if (elapsed > 3f)
                    Destroy(gameObject);
                return;
            }

            Apply(sun.gameObject);
            Destroy(gameObject);
        }
    }

    static void Apply(GameObject sun)
    {
        if (sun.GetComponent<SunAppearanceMarker>() != null)
            return;

        sun.AddComponent<SunAppearanceMarker>();

        var renderer = sun.GetComponent<MeshRenderer>();
        var sunMaterial = Resources.Load<Material>("Materials/Sun");
        if (sunMaterial != null && renderer != null)
            renderer.sharedMaterial = sunMaterial;

        EnsureCorona(sun.transform);
        EnsureSunLight(sun);
    }

    static void EnsureCorona(Transform sun)
    {
        if (sun.Find("Corona") != null)
            return;

        var coronaObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coronaObject.name = "Corona";
        coronaObject.transform.SetParent(sun, false);
        coronaObject.transform.localScale = Vector3.one * CoronaScale;

        var collider = coronaObject.GetComponent<Collider>();
        if (collider != null)
            Object.Destroy(collider);

        var coronaMaterial = Resources.Load<Material>("Materials/SunCorona");
        var coronaRenderer = coronaObject.GetComponent<MeshRenderer>();
        if (coronaMaterial != null)
        {
            coronaRenderer.sharedMaterial = coronaMaterial;
        }
        else
        {
            var fallback = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            fallback.SetColor("_BaseColor", new Color(1.4f, 0.95f, 0.35f, 0.28f));
            coronaRenderer.sharedMaterial = fallback;
        }
    }

    static void EnsureSunLight(GameObject sun)
    {
        if (sun.GetComponent<Light>() != null)
            return;

        var light = sun.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.9f, 0.72f);
        light.intensity = 3.2f;
        light.range = 70f;
        light.shadows = LightShadows.Soft;
    }
}

sealed class SunAppearanceMarker : MonoBehaviour { }
