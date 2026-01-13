using UnityEngine;
using UnityEngine.UIElements;

public class AircraftModelPreview : MonoBehaviour
{
    [Header("Preview Settings")]
    [SerializeField]
    private GameObject aircraftPrefab;

    [SerializeField]
    private Vector3 modelOffset = new Vector3(0, -0.5f, 10);

    [SerializeField]
    private Vector3 modelRotation = new Vector3(5, -30, 0);

    [SerializeField]
    private float modelScale = 0.7f;

    [SerializeField]
    private float rotationSpeed = 20f;

    [SerializeField]
    private bool autoRotate = true;

    [Header("Zoom Settings")]
    [SerializeField]
    private float minZoom = 5f;

    [SerializeField]
    private float maxZoom = 20f;

    [SerializeField]
    private float zoomSpeed = 2f;

    private float currentZoom = 10f;

    [Header("Camera Settings")]
    [SerializeField]
    private int renderTextureWidth = 2048;

    [SerializeField]
    private int renderTextureHeight = 1536;

    [Header("Background Settings")]
    [SerializeField]
    private string backgroundTexturePath = "Hangar/hangar_bg";

    [SerializeField]
    private float backgroundDistance = 60f;

    [Header("UI Reference")]
    [SerializeField]
    private UIDocument hangarUIDocument;

    [SerializeField]
    private UnityEngine.UI.RawImage previewRawImage;

    private Camera previewCamera;
    private RenderTexture renderTexture;
    private GameObject previewModel;
    private GameObject previewRoot;
    private GameObject backgroundQuad;
    private Material backgroundMaterial;

    private float currentRotationY = 0f;
    private float currentRotationX = 5f;
    private bool isDragging = false;
    private Vector2 lastMousePos;
    private string currentAircraftName = "F15";

    private VisualElement modelPreviewContainer;

    void Start()
    {
        CreatePreviewSetup();
    }

    void OnEnable()
    {
        if (hangarUIDocument != null)
        {
            var root = hangarUIDocument.rootVisualElement;
            if (root != null)
            {
                modelPreviewContainer = root.Q<VisualElement>("model-preview-container");
                if (modelPreviewContainer != null)
                {
                    modelPreviewContainer.RegisterCallback<MouseDownEvent>(OnMouseDown);
                    modelPreviewContainer.RegisterCallback<MouseMoveEvent>(OnMouseMove);
                    modelPreviewContainer.RegisterCallback<MouseUpEvent>(OnMouseUp);
                    modelPreviewContainer.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
                    modelPreviewContainer.RegisterCallback<WheelEvent>(OnWheel);
                }
            }
        }
    }

    void OnDisable()
    {
        if (modelPreviewContainer != null)
        {
            modelPreviewContainer.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            modelPreviewContainer.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            modelPreviewContainer.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            modelPreviewContainer.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
            modelPreviewContainer.UnregisterCallback<WheelEvent>(OnWheel);
        }
    }

    void CreatePreviewSetup()
    {
        Debug.Log("[AircraftModelPreview] CreatePreviewSetup() called");

        previewRoot = new GameObject("AircraftPreviewRoot");
        previewRoot.transform.position = new Vector3(1000, 1000, 1000);
        previewRoot.SetActive(true);
        Debug.Log(
            $"[AircraftModelPreview] PreviewRoot created at {previewRoot.transform.position}"
        );

        renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 24);
        renderTexture.antiAliasing = 4;
        renderTexture.Create();
        Debug.Log(
            $"[AircraftModelPreview] RenderTexture created: {renderTexture.width}x{renderTexture.height}"
        );

        GameObject camObj = new GameObject("PreviewCamera");
        camObj.transform.SetParent(previewRoot.transform);
        camObj.transform.localPosition = Vector3.zero;
        camObj.transform.localRotation = Quaternion.identity;

        previewCamera = camObj.AddComponent<Camera>();
        previewCamera.targetTexture = renderTexture;
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0.02f, 0.05f, 0.12f, 1f);
        previewCamera.fieldOfView = 40f;
        previewCamera.nearClipPlane = 0.1f;
        previewCamera.farClipPlane = 100f;
        previewCamera.cullingMask = -1;
        previewCamera.enabled = true;

        camObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        camObj.transform.LookAt(previewRoot.transform.position + modelOffset);

        Debug.Log(
            $"[AircraftModelPreview] Camera created at {camObj.transform.position}, looking at {previewRoot.transform.position + modelOffset}"
        );

        CreatePreviewBackground();
        CreatePreviewModel();

        if (previewRawImage != null)
        {
            previewRawImage.texture = renderTexture;
        }

        Debug.Log("[AircraftModelPreview] Preview setup created");
    }

    void CreatePreviewBackground()
    {
        if (previewCamera == null || string.IsNullOrEmpty(backgroundTexturePath))
        {
            return;
        }

        Texture2D backgroundTexture = Resources.Load<Texture2D>(backgroundTexturePath);
        if (backgroundTexture == null)
        {
            Debug.LogWarning(
                $"[AircraftModelPreview] Background texture not found: Resources/{backgroundTexturePath}"
            );
            return;
        }

        if (backgroundQuad == null)
        {
            backgroundQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backgroundQuad.name = "PreviewBackground";
            backgroundQuad.transform.SetParent(previewCamera.transform, false);

            var collider = backgroundQuad.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
        }

        var renderer = backgroundQuad.GetComponent<MeshRenderer>();
        if (backgroundMaterial == null)
        {
            Shader shader = Shader.Find("Unlit/Texture");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            backgroundMaterial = new Material(shader);
        }

        backgroundMaterial.mainTexture = backgroundTexture;
        renderer.sharedMaterial = backgroundMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        float distance = Mathf.Min(backgroundDistance, previewCamera.farClipPlane - 0.1f);
        backgroundQuad.transform.localPosition = new Vector3(0f, 0f, distance);
        backgroundQuad.transform.localRotation = Quaternion.identity;

        float aspect = 1f;
        if (renderTexture != null && renderTexture.height > 0)
        {
            aspect = (float)renderTexture.width / renderTexture.height;
        }
        else if (renderTextureHeight > 0)
        {
            aspect = (float)renderTextureWidth / renderTextureHeight;
        }

        float height = 2f * distance * Mathf.Tan(previewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * aspect;
        backgroundQuad.transform.localScale = new Vector3(width, height, 1f);
    }

    void CreatePreviewModel()
    {
        Debug.Log("[AircraftModelPreview] CreatePreviewModel() called");

        if (aircraftPrefab == null)
        {
            Debug.Log(
                "[AircraftModelPreview] No prefab assigned, trying to load from Resources..."
            );

            aircraftPrefab = Resources.Load<GameObject>("Prefabs/F15");
            if (aircraftPrefab == null)
            {
                Debug.Log(
                    "[AircraftModelPreview] No prefab found in Resources, creating placeholder..."
                );

                CreatePlaceholderModel();
                return;
            }
        }

        Debug.Log($"[AircraftModelPreview] Using prefab: {aircraftPrefab.name}");

        if (previewModel != null)
        {
            Destroy(previewModel);
        }

        previewModel = Instantiate(aircraftPrefab, previewRoot.transform);

        Vector3 adjustedOffset = modelOffset;
        float adjustedScale = modelScale;
        ApplyAircraftAdjustments(currentAircraftName, ref adjustedOffset, ref adjustedScale);

        previewModel.transform.localPosition = adjustedOffset;
        previewModel.transform.localRotation = Quaternion.Euler(modelRotation);
        previewModel.transform.localScale = Vector3.one * adjustedScale;

        Debug.Log(
            $"[AircraftModelPreview] Model created at localPos={previewModel.transform.localPosition}, worldPos={previewModel.transform.position}, scale={adjustedScale}"
        );

        DisableAllScripts(previewModel);

        SetLayerRecursively(previewModel, LayerMask.NameToLayer("Default"));

        currentRotationY = modelRotation.y;

        currentZoom = adjustedOffset.z;
    }

    void ApplyAircraftAdjustments(string aircraftName, ref Vector3 offset, ref float scale)
    {
        switch (aircraftName)
        {
            case "F15":

                break;
            case "Su27":

                offset.y = -1.5f;

                scale = 0.5f;
                break;
            case "Mig29":

                offset.z = 6f;
                scale = 1.2f;
                break;
            default:

                break;
        }

        Debug.Log(
            $"[AircraftModelPreview] Applied adjustments for {aircraftName}: offset={offset}, scale={scale}"
        );
    }

    void CreatePlaceholderModel()
    {
        Debug.Log("[AircraftModelPreview] Creating placeholder model...");

        previewModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        previewModel.name = "PlaceholderAircraft";
        previewModel.transform.SetParent(previewRoot.transform);
        previewModel.transform.localPosition = modelOffset;
        previewModel.transform.localRotation = Quaternion.Euler(modelRotation);
        previewModel.transform.localScale = new Vector3(3f, 0.5f, 2f) * modelScale;

        var leftWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWing.transform.SetParent(previewModel.transform);
        leftWing.transform.localPosition = new Vector3(-1.5f, 0, 0);
        leftWing.transform.localScale = new Vector3(2f, 0.1f, 0.8f);

        var rightWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWing.transform.SetParent(previewModel.transform);
        rightWing.transform.localPosition = new Vector3(1.5f, 0, 0);
        rightWing.transform.localScale = new Vector3(2f, 0.1f, 0.8f);

        var material = new Material(Shader.Find("Unlit/Color"));
        if (material.shader == null || material.shader.name == "Hidden/InternalErrorShader")
        {
            material = new Material(Shader.Find("Standard"));
        }
        material.color = new Color(0.4f, 0.5f, 0.6f);
        previewModel.GetComponent<Renderer>().material = material;
        leftWing.GetComponent<Renderer>().material = material;
        rightWing.GetComponent<Renderer>().material = material;

        GameObject lightObj = new GameObject("PreviewLight");
        lightObj.transform.SetParent(previewRoot.transform);
        lightObj.transform.localPosition = new Vector3(5, 5, -5);
        Light previewLight = lightObj.AddComponent<Light>();
        previewLight.type = LightType.Directional;
        previewLight.intensity = 1.5f;
        previewLight.color = Color.white;
        lightObj.transform.LookAt(previewRoot.transform.position + modelOffset);

        currentRotationY = modelRotation.y;
        Debug.Log(
            $"[AircraftModelPreview] Placeholder model created at {previewModel.transform.position}"
        );
    }

    void DisableAllScripts(GameObject obj)
    {
        var components = obj.GetComponentsInChildren<MonoBehaviour>();
        foreach (var comp in components)
        {
            if (comp != null && comp != this)
            {
                comp.enabled = false;
            }
        }

        var rigidbodies = obj.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = true;
        }

        var colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void Update()
    {
        if (previewModel == null)
            return;

        if (autoRotate && !isDragging)
        {
            currentRotationY += rotationSpeed * Time.unscaledDeltaTime;
        }

        previewModel.transform.localRotation = Quaternion.Euler(
            currentRotationX,
            currentRotationY,
            modelRotation.z
        );
    }

    void OnMouseDown(MouseDownEvent evt)
    {
        isDragging = true;
        lastMousePos = evt.localMousePosition;
        evt.StopPropagation();
    }

    void OnMouseMove(MouseMoveEvent evt)
    {
        if (!isDragging)
            return;

        Vector2 delta = evt.localMousePosition - lastMousePos;

        currentRotationY += delta.x * 0.5f;

        currentRotationX -= delta.y * 0.5f;
        currentRotationX = Mathf.Clamp(currentRotationX, -80f, 80f);

        lastMousePos = evt.localMousePosition;
        evt.StopPropagation();
    }

    void OnMouseUp(MouseUpEvent evt)
    {
        isDragging = false;
        evt.StopPropagation();
    }

    void OnMouseLeave(MouseLeaveEvent evt)
    {
        isDragging = false;
    }

    void OnWheel(WheelEvent evt)
    {
        currentZoom += evt.delta.y * zoomSpeed * 0.1f;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        if (previewModel != null)
        {
            modelOffset.z = currentZoom;
            previewModel.transform.localPosition = modelOffset;
        }

        evt.StopPropagation();
    }

    public void RotateModel(float deltaX, float deltaY)
    {
        currentRotationY += deltaX * 0.5f;

        currentRotationX -= deltaY * 0.5f;
        currentRotationX = Mathf.Clamp(currentRotationX, -80f, 80f);
    }

    public void ZoomModel(float delta)
    {
        currentZoom += delta * zoomSpeed * 0.1f;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        if (previewModel != null)
        {
            modelOffset.z = currentZoom;
            previewModel.transform.localPosition = modelOffset;
        }
    }

    public void SetDragging(bool dragging)
    {
        isDragging = dragging;
    }

    public bool IsDragging()
    {
        return isDragging;
    }

    public void SetAircraftPrefab(GameObject prefab)
    {
        aircraftPrefab = prefab;
        CreatePreviewModel();
    }

    public void LoadAircraftByName(string prefabName)
    {
        Debug.Log($"[AircraftModelPreview] Loading aircraft: {prefabName}");

        currentAircraftName = prefabName;

        GameObject loadedPrefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
        if (loadedPrefab != null)
        {
            aircraftPrefab = loadedPrefab;
            CreatePreviewModel();
        }
        else
        {
            Debug.LogWarning(
                $"[AircraftModelPreview] Could not find prefab: Prefabs/{prefabName}, using placeholder"
            );
            aircraftPrefab = null;
            CreatePreviewModel();
        }
    }

    public RenderTexture GetRenderTexture()
    {
        return renderTexture;
    }

    public void ShowPreview()
    {
        if (previewCamera != null)
            previewCamera.enabled = true;
    }

    public void HidePreview()
    {
        if (previewCamera != null)
            previewCamera.enabled = false;
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        if (backgroundMaterial != null)
        {
            Destroy(backgroundMaterial);
        }

        if (previewRoot != null)
        {
            Destroy(previewRoot);
        }
    }
}
