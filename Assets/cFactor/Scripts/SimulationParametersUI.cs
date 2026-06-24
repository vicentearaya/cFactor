using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Panel de parámetros para explorar órbitas newtonianas.
/// Se crea automáticamente al cargar la escena.
/// </summary>
[DisallowMultipleComponent]
public class SimulationParametersUI : MonoBehaviour
{
    const float DefaultStarMass = 1f;
    const float DefaultOrbitRadius = 8f;
    const float DefaultTimeScale = 5f;

    const float MinStarMass = 0.2f;
    const float MaxStarMass = 5f;
    const float MinOrbitRadius = 3f;
    const float MaxOrbitRadius = 20f;
    const float MinSpeed = 0.05f;
    const float MaxSpeed = 1.2f;
    const float MinTimeScale = 1f;
    const float MaxTimeScale = 15f;

    NewtonianOrbit orbit;

    Slider starMassSlider;
    Slider orbitRadiusSlider;
    Slider speedSlider;
    Slider timeScaleSlider;
    Toggle circularVelocityToggle;

    Text starMassLabel;
    Text orbitRadiusLabel;
    Text speedLabel;
    Text timeScaleLabel;
    Text statusLabel;
    Text previewLabel;

    float gravitationalConstant = 1f;

    void Start()
    {
        orbit = FindOrbit();
        if (orbit == null)
        {
            Debug.LogWarning("SimulationParametersUI: no se encontró NewtonianOrbit.");
            enabled = false;
            return;
        }

        gravitationalConstant = orbit.GravitationalParameter / Mathf.Max(orbit.CentralMass, 0.01f);
        BuildUI();
        SyncLabels();
        UpdatePreview();
    }

    void Update()
    {
        if (orbit == null)
            return;

        float mu = orbit.GravitationalParameter;
        string orbitType = NewtonianOrbit.ClassifyOrbit(orbit.Speed, orbit.DistanceToStar, mu);
        float energy = NewtonianOrbit.SpecificOrbitalEnergy(orbit.Speed, orbit.DistanceToStar, mu);

        statusLabel.text =
            $"Estado en vivo\n" +
            $"r = {orbit.DistanceToStar:F2}\n" +
            $"v = {orbit.Speed:F3}\n" +
            $"ε = {energy:F3}\n" +
            $"Tipo: {orbitType}";
    }

    static NewtonianOrbit FindOrbit()
    {
        var simulation = GameObject.Find("Simulation");
        if (simulation == null)
            return FindAnyObjectByType<NewtonianOrbit>();

        var planet = simulation.transform.Find("Planet");
        return planet != null ? planet.GetComponent<NewtonianOrbit>() : FindAnyObjectByType<NewtonianOrbit>();
    }

    void BuildUI()
    {
        EnsureEventSystem();

        var canvasObject = new GameObject("ParametersCanvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        var panel = CreatePanel(canvasObject.transform, "Panel",
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(16f, 16f), new Vector2(340f, -16f));

        float y = -20f;

        CreateTitle(panel.transform, "cFactor — Parámetros", ref y);
        y -= 8f;

        starMassSlider = CreateSliderRow(panel.transform, "Masa estrella (M)", MinStarMass, MaxStarMass,
            orbit.CentralMass, ref y, out starMassLabel);
        orbitRadiusSlider = CreateSliderRow(panel.transform, "Distancia (r)", MinOrbitRadius, MaxOrbitRadius,
            DefaultOrbitRadius, ref y, out orbitRadiusLabel);

        float defaultSpeed = NewtonianOrbit.CircularOrbitalSpeed(DefaultOrbitRadius, orbit.GravitationalParameter);
        speedSlider = CreateSliderRow(panel.transform, "Velocidad (v)", MinSpeed, MaxSpeed,
            defaultSpeed, ref y, out speedLabel);
        timeScaleSlider = CreateSliderRow(panel.transform, "Escala de tiempo", MinTimeScale, MaxTimeScale,
            orbit.TimeScale, ref y, out timeScaleLabel);

        y -= 8f;
        circularVelocityToggle = CreateToggle(panel.transform, "Usar velocidad circular", true, ref y);

        y -= 12f;
        previewLabel = CreateInfoText(panel.transform, "", ref y, 70f);
        y -= 8f;
        statusLabel = CreateInfoText(panel.transform, "", ref y, 100f);
        y -= 16f;

        CreateButton(panel.transform, "Aplicar y reiniciar", ref y, ApplyAndReset);
        y -= 12f;
        CreateButton(panel.transform, "Limpiar trayectoria", ref y, ClearTrail);

        starMassSlider.onValueChanged.AddListener(_ => OnParameterChanged());
        orbitRadiusSlider.onValueChanged.AddListener(_ => OnParameterChanged());
        speedSlider.onValueChanged.AddListener(_ => OnParameterChanged());
        timeScaleSlider.onValueChanged.AddListener(value =>
        {
            orbit.TimeScale = value;
            SyncLabels();
        });
        circularVelocityToggle.onValueChanged.AddListener(_ => OnParameterChanged());
    }

    void OnParameterChanged()
    {
        speedSlider.interactable = !circularVelocityToggle.isOn;
        SyncLabels();
        UpdatePreview();
    }

    void SyncLabels()
    {
        starMassLabel.text = $"{starMassSlider.value:F2}";
        orbitRadiusLabel.text = $"{orbitRadiusSlider.value:F2}";
        timeScaleLabel.text = $"{timeScaleSlider.value:F1}×";

        float speed = GetSelectedSpeed();
        speedLabel.text = $"{speed:F3}";
    }

    void UpdatePreview()
    {
        float mu = gravitationalConstant * starMassSlider.value;
        float r = orbitRadiusSlider.value;
        float v = GetSelectedSpeed();
        float vCirc = NewtonianOrbit.CircularOrbitalSpeed(r, mu);
        float vEsc = NewtonianOrbit.EscapeSpeed(r, mu);

        previewLabel.text =
            $"Vista previa (al aplicar)\n" +
            $"μ = {mu:F2} | v_circ = {vCirc:F3}\n" +
            $"v_escape = {vEsc:F3}\n" +
            $"Tipo: {NewtonianOrbit.ClassifyOrbit(v, r, mu)}";
    }

    float GetSelectedSpeed()
    {
        float mu = gravitationalConstant * starMassSlider.value;
        float r = orbitRadiusSlider.value;

        if (circularVelocityToggle.isOn)
            return NewtonianOrbit.CircularOrbitalSpeed(r, mu);

        return speedSlider.value;
    }

    void ApplyAndReset()
    {
        orbit.SetCentralMass(starMassSlider.value);
        orbit.TimeScale = timeScaleSlider.value;
        orbit.ResetOrbit(orbitRadiusSlider.value, GetSelectedSpeed());
        UpdatePreview();
    }

    void ClearTrail()
    {
        orbit.ClearTrail();
    }

    static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
            return;

        var eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    static RectTransform CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchor,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        var rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchor;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        var image = panelObject.GetComponent<Image>();
        image.color = new Color(0.06f, 0.08f, 0.14f, 0.88f);

        return rect;
    }

    static void CreateTitle(Transform parent, string text, ref float y)
    {
        var title = CreateTextObject(parent, text, 20, FontStyle.Bold);
        var rect = title.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(-32f, 32f);
        y -= 36f;
    }

    static Slider CreateSliderRow(Transform parent, string label, float min, float max, float value,
        ref float y, out Text valueText)
    {
        var labelObject = CreateTextObject(parent, label, 14, FontStyle.Normal);
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0f, 1f);
        labelRect.anchoredPosition = new Vector2(16f, y);
        labelRect.sizeDelta = new Vector2(-32f, 20f);

        var valueObject = CreateTextObject(parent, "0", 14, FontStyle.Bold);
        valueText = valueObject.GetComponent<Text>();
        valueText.alignment = TextAnchor.MiddleRight;
        var valueRect = valueObject.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0f, 1f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.pivot = new Vector2(1f, 1f);
        valueRect.anchoredPosition = new Vector2(-16f, y);
        valueRect.sizeDelta = new Vector2(-32f, 20f);

        y -= 24f;

        var slider = CreateSlider(parent, min, max, value, y);
        y -= 48f;
        return slider;
    }

    static Slider CreateSlider(Transform parent, float min, float max, float value, float y)
    {
        var sliderObject = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);

        var rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(-32f, 20f);

        var background = CreateImage(sliderObject.transform, "Background", new Color(0.15f, 0.18f, 0.25f, 1f));
        Stretch(background.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>(), new Vector2(8f, 0f), new Vector2(-8f, 0f));

        var fill = CreateImage(fillArea.transform, "Fill", new Color(0.28f, 0.62f, 0.95f, 1f));
        Stretch(fill.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObject.transform, false);
        Stretch(handleArea.GetComponent<RectTransform>(), new Vector2(8f, 0f), new Vector2(-8f, 0f));

        var handle = CreateImage(handleArea.transform, "Handle", new Color(0.92f, 0.95f, 1f, 1f));
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(16f, 16f);

        var slider = sliderObject.GetComponent<Slider>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handleRect;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        return slider;
    }

    static Toggle CreateToggle(Transform parent, string label, bool isOn, ref float y)
    {
        var toggleObject = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
        toggleObject.transform.SetParent(parent, false);

        var rect = toggleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, y);
        rect.sizeDelta = new Vector2(-32f, 24f);

        var box = CreateImage(toggleObject.transform, "Box", new Color(0.15f, 0.18f, 0.25f, 1f));
        var boxRect = box.GetComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0f, 0.5f);
        boxRect.anchorMax = new Vector2(0f, 0.5f);
        boxRect.pivot = new Vector2(0f, 0.5f);
        boxRect.anchoredPosition = Vector2.zero;
        boxRect.sizeDelta = new Vector2(20f, 20f);

        var check = CreateImage(box.transform, "Check", new Color(0.35f, 0.78f, 1f, 1f));
        Stretch(check.GetComponent<RectTransform>(), new Vector2(4f, 4f), new Vector2(-4f, -4f));

        var labelObject = CreateTextObject(toggleObject.transform, label, 14, FontStyle.Normal);
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(28f, 0f);
        labelRect.offsetMax = Vector2.zero;

        var toggle = toggleObject.GetComponent<Toggle>();
        toggle.targetGraphic = box.GetComponent<Image>();
        toggle.graphic = check.GetComponent<Image>();
        toggle.isOn = isOn;

        y -= 32f;
        return toggle;
    }

    static Text CreateInfoText(Transform parent, string text, ref float y, float height)
    {
        var textObject = CreateTextObject(parent, text, 13, FontStyle.Normal);
        var textComponent = textObject.GetComponent<Text>();
        var rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, y);
        rect.sizeDelta = new Vector2(-32f, height);
        y -= height;
        return textComponent;
    }

    static void CreateButton(Transform parent, string label, ref float y, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(-32f, 36f);

        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.22f, 0.48f, 0.82f, 1f);

        var textObject = CreateTextObject(buttonObject.transform, label, 14, FontStyle.Bold);
        Stretch(textObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        y -= 44f;
    }

    static GameObject CreateTextObject(Transform parent, string text, int fontSize, FontStyle style)
    {
        var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        var textComponent = textObject.GetComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = style;
        textComponent.color = new Color(0.92f, 0.95f, 1f, 1f);
        textComponent.alignment = TextAnchor.MiddleLeft;
        textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        textComponent.verticalOverflow = VerticalWrapMode.Overflow;
        return textObject;
    }

    static GameObject CreateImage(Transform parent, string name, Color color)
    {
        var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        imageObject.GetComponent<Image>().color = color;
        return imageObject;
    }

    static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
