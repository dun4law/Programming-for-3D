using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UIButton = UnityEngine.UIElements.Button;

[System.Serializable]
public class AircraftData
{
    public string id;
    public string name;
    public string role;
    public string origin;
    public string description;
    public float speedPercent;
    public string speedValue;
    public float mobilityPercent;
    public string mobilityValue;
    public float armorPercent;
    public string armorValue;
    public float firepowerPercent;
    public string firepowerValue;
    public string[] weaponNames;
    public string[] weaponDescs;
    public string[] weaponIconClasses;
    public string prefabName;
}

public class HangarController : MonoBehaviour
{
    [SerializeField]
    private UIDocument uiDocument;

    [SerializeField]
    private AircraftModelPreview modelPreview;

    [SerializeField]
    private UnityEngine.UI.RawImage modelPreviewRawImage;

    [SerializeField]
    private bool autoCreateRawImage = true;

    private VisualElement root;
    private UIButton backButton;
    private UIButton launchButton;
    private VisualElement modelPreviewContainer;
    private VisualElement aircraft3DPreview;

    private UIButton tabF15;
    private UIButton tabSu27;
    private UIButton tabMig29;
    private UIButton tabFA18E;
    private UIButton tabHawk200;
    private UIButton tabMig21;
    private UIButton tabTornado;
    private UIButton tabRafale;
    private UIButton tabTyphoon;
    private VisualElement contentAircraft;

    private Label aircraftNameLabel;
    private Label aircraftRoleLabel;
    private Label aircraftOriginLabel;
    private Label aircraftDescriptionLabel;
    private VisualElement speedBar;
    private Label speedValueLabel;
    private VisualElement mobilityBar;
    private Label mobilityValueLabel;
    private VisualElement armorBar;
    private Label armorValueLabel;
    private VisualElement firepowerBar;
    private Label firepowerValueLabel;
    private VisualElement armamentSection;

    private UIButton setDefaultButton;
    private Label defaultIndicatorLabel;
    public const string DefaultAircraftKey = "DefaultAircraft";

    private Canvas rawImageCanvas;
    private bool rawImageCreated = false;

    private UIButton fullscreenButton;
    private UIButton closeFullscreenButton;
    private VisualElement fullscreenOverlay;
    private VisualElement fullscreenPreviewContainer;
    private bool isFullscreen = false;
    private Canvas fullscreenRawImageCanvas;
    private UnityEngine.UI.RawImage fullscreenRawImage;

    private bool isFullscreenDragging = false;
    private Vector2 lastFullscreenMousePos;
    private float launchButtonPulseTime = 0f;

    private int lastScreenWidth;
    private int lastScreenHeight;
    private Rect lastContainerRect;

    private int selectedAircraftIndex = 0;
    private List<AircraftData> aircraftList;

    void Awake()
    {
        InitializeAircraftData();
    }

    void InitializeAircraftData()
    {
        aircraftList = new List<AircraftData>
        {
            new AircraftData
            {
                id = "f15",
                name = "F-15C EAGLE",
                role = "Air Superiority Fighter",
                origin = "United States - McDonnell Douglas",
                description =
                    "The F-15C Eagle is an all-weather tactical fighter designed to gain and maintain air superiority in aerial combat. It is considered among the most successful modern fighters with over 100 victories and no losses in aerial combat. Its superior maneuverability and acceleration, weapons systems, and avionics have made it a formidable air-to-air platform.",
                speedPercent = 90f,
                speedValue = "Mach 2.5",
                mobilityPercent = 80f,
                mobilityValue = "9G",
                armorPercent = 65f,
                armorValue = "Medium",
                firepowerPercent = 95f,
                firepowerValue = "Excellent",
                weaponNames = new[] { "M61A1 VULCAN", "AIM-9X SIDEWINDER", "AIM-120C AMRAAM" },
                weaponDescs = new[]
                {
                    "20mm Rotary Cannon - 940 rounds",
                    "IR-guided AAM - x4",
                    "Radar-guided AAM - x4",
                },
                weaponIconClasses = new[] { "cannon-m61", "missile-aim9", "missile-aim120" },
                prefabName = "F15",
            },
            new AircraftData
            {
                id = "su27",
                name = "Su-27 FLANKER",
                role = "Air Superiority Fighter",
                origin = "Russia - Sukhoi",
                description =
                    "The Sukhoi Su-27 is a twin-engine supermaneuverable fighter aircraft. Designed as a direct competitor to the F-15 Eagle, the Flanker features exceptional range and heavy weapons load. Famous for pioneering the Pugachev's Cobra maneuver, it remains one of the most agile fighters ever built.",
                speedPercent = 88f,
                speedValue = "Mach 2.35",
                mobilityPercent = 92f,
                mobilityValue = "9G+",
                armorPercent = 70f,
                armorValue = "Medium-High",
                firepowerPercent = 90f,
                firepowerValue = "Excellent",
                weaponNames = new[] { "GSh-30-1 CANNON", "R-73 ARCHER", "R-27 ALAMO" },
                weaponDescs = new[]
                {
                    "30mm Autocannon - 150 rounds",
                    "IR-guided AAM - x6",
                    "Radar-guided AAM - x4",
                },
                weaponIconClasses = new[] { "cannon-gsh30", "missile-r73", "missile-r27" },
                prefabName = "Su27",
            },
            new AircraftData
            {
                id = "mig29",
                name = "MiG-29 FULCRUM",
                role = "Multirole Fighter",
                origin = "Russia - Mikoyan",
                description =
                    "The MiG-29 is a twin-engine jet fighter designed for air superiority and ground attack missions. Known for its exceptional close-combat capabilities and helmet-mounted sight system, the Fulcrum combines agility with robust construction. It serves as the backbone of many air forces worldwide.",
                speedPercent = 85f,
                speedValue = "Mach 2.25",
                mobilityPercent = 88f,
                mobilityValue = "9G",
                armorPercent = 75f,
                armorValue = "High",
                firepowerPercent = 82f,
                firepowerValue = "Very Good",
                weaponNames = new[] { "GSh-30-1 CANNON", "R-73 ARCHER", "R-77 ADDER" },
                weaponDescs = new[]
                {
                    "30mm Autocannon - 150 rounds",
                    "IR-guided AAM - x6",
                    "Radar-guided AAM - x2",
                },
                weaponIconClasses = new[] { "cannon-gsh30", "missile-r73", "missile-r77" },
                prefabName = "Mig29",
            },
            new AircraftData
            {
                id = "fa18e",
                name = "F/A-18E SUPER HORNET",
                role = "Multirole Strike Fighter",
                origin = "United States - Boeing",
                description =
                    "The F/A-18E Super Hornet is a twin-engine, carrier-capable, multirole fighter aircraft. An evolution of the original Hornet, the Super Hornet is larger, more powerful, and features improved avionics. It serves as the backbone of U.S. Navy carrier air wings with exceptional versatility in both air-to-air and air-to-ground missions.",
                speedPercent = 82f,
                speedValue = "Mach 1.8",
                mobilityPercent = 85f,
                mobilityValue = "7.5G",
                armorPercent = 70f,
                armorValue = "Medium-High",
                firepowerPercent = 92f,
                firepowerValue = "Excellent",
                weaponNames = new[] { "M61A2 VULCAN", "AIM-9X SIDEWINDER", "AIM-120D AMRAAM" },
                weaponDescs = new[]
                {
                    "20mm Rotary Cannon - 578 rounds",
                    "IR-guided AAM - x2",
                    "Radar-guided AAM - x6",
                },
                weaponIconClasses = new[] { "cannon-m61", "missile-aim9", "missile-aim120" },
                prefabName = "fa18e",
            },
            new AircraftData
            {
                id = "hawk200",
                name = "HAWK 200",
                role = "Light Combat Aircraft",
                origin = "United Kingdom - BAE Systems",
                description =
                    "The BAE Hawk 200 is a single-seat light multirole combat aircraft derived from the Hawk trainer. Compact and agile, it offers excellent maneuverability at a lower operating cost. Ideal for air defense and ground attack missions in smaller air forces requiring a capable yet economical platform.",
                speedPercent = 70f,
                speedValue = "Mach 1.2",
                mobilityPercent = 82f,
                mobilityValue = "8G",
                armorPercent = 50f,
                armorValue = "Light",
                firepowerPercent = 60f,
                firepowerValue = "Moderate",
                weaponNames = new[] { "ADEN 25 CANNON", "AIM-9L SIDEWINDER", "AGM-65 MAVERICK" },
                weaponDescs = new[]
                {
                    "25mm Cannon - 100 rounds",
                    "IR-guided AAM - x2",
                    "Air-to-ground missile - x2",
                },
                weaponIconClasses = new[] { "cannon-m61", "missile-aim9", "missile-aim120" },
                prefabName = "Hawk_200",
            },
            new AircraftData
            {
                id = "mig21",
                name = "MiG-21 FISHBED",
                role = "Interceptor Fighter",
                origin = "Russia - Mikoyan-Gurevich",
                description =
                    "The MiG-21 is a legendary supersonic jet fighter and interceptor. One of the most produced supersonic aircraft in history, the Fishbed combines high speed with simple, rugged construction. Its delta wing design provides excellent high-speed performance and has seen combat in numerous conflicts worldwide.",
                speedPercent = 88f,
                speedValue = "Mach 2.05",
                mobilityPercent = 75f,
                mobilityValue = "8G",
                armorPercent = 55f,
                armorValue = "Light-Medium",
                firepowerPercent = 65f,
                firepowerValue = "Good",
                weaponNames = new[] { "GSh-23L CANNON", "R-60 APHID", "R-3S ATOLL" },
                weaponDescs = new[]
                {
                    "23mm Twin-barrel Cannon - 200 rounds",
                    "IR-guided AAM - x4",
                    "IR-guided AAM - x2",
                },
                weaponIconClasses = new[] { "cannon-gsh30", "missile-r73", "missile-r73" },
                prefabName = "mig21",
            },
            new AircraftData
            {
                id = "tornado",
                name = "PANAVIA TORNADO",
                role = "Multirole Combat Aircraft",
                origin = "Europe - Panavia (UK/Germany/Italy)",
                description =
                    "The Panavia Tornado is a twin-engine, variable-sweep wing multirole combat aircraft jointly developed by the UK, Germany, and Italy. Known for its terrain-following capability and all-weather precision strike abilities, the Tornado excels in ground attack and air defense suppression missions.",
                speedPercent = 86f,
                speedValue = "Mach 2.2",
                mobilityPercent = 78f,
                mobilityValue = "7.5G",
                armorPercent = 72f,
                armorValue = "Medium-High",
                firepowerPercent = 88f,
                firepowerValue = "Very Good",
                weaponNames = new[] { "MAUSER BK-27", "AIM-9L SIDEWINDER", "STORM SHADOW" },
                weaponDescs = new[]
                {
                    "27mm Revolver Cannon - 180 rounds",
                    "IR-guided AAM - x4",
                    "Cruise missile - x2",
                },
                weaponIconClasses = new[] { "cannon-m61", "missile-aim9", "missile-aim120" },
                prefabName = "panavia-tornado",
            },
            new AircraftData
            {
                id = "rafale",
                name = "DASSAULT RAFALE",
                role = "Omnirole Fighter",
                origin = "France - Dassault Aviation",
                description =
                    "The Dassault Rafale is a French twin-engine, canard delta wing, multirole fighter aircraft. Designed to perform air supremacy, interdiction, reconnaissance, and nuclear strike deterrence missions, the Rafale is one of the most advanced and versatile combat aircraft in the world with full operational capability across all mission types.",
                speedPercent = 87f,
                speedValue = "Mach 1.8",
                mobilityPercent = 90f,
                mobilityValue = "9G",
                armorPercent = 68f,
                armorValue = "Medium",
                firepowerPercent = 94f,
                firepowerValue = "Excellent",
                weaponNames = new[] { "GIAT 30/M791", "MICA IR", "METEOR BVRAAM" },
                weaponDescs = new[]
                {
                    "30mm Revolver Cannon - 125 rounds",
                    "IR-guided AAM - x4",
                    "Radar-guided AAM - x4",
                },
                weaponIconClasses = new[] { "cannon-gsh30", "missile-r73", "missile-r27" },
                prefabName = "rafalemf3",
            },
            new AircraftData
            {
                id = "typhoon",
                name = "EUROFIGHTER TYPHOON",
                role = "Air Superiority Fighter",
                origin = "Europe - Eurofighter (UK/Germany/Italy/Spain)",
                description =
                    "The Eurofighter Typhoon is a twin-engine, canard-delta wing, multirole fighter developed by a consortium of European nations. Featuring advanced fly-by-wire controls and powerful engines, the Typhoon offers exceptional agility and climb rate. It is one of the most capable air superiority fighters currently in service.",
                speedPercent = 89f,
                speedValue = "Mach 2.0",
                mobilityPercent = 92f,
                mobilityValue = "9G+",
                armorPercent = 65f,
                armorValue = "Medium",
                firepowerPercent = 91f,
                firepowerValue = "Excellent",
                weaponNames = new[] { "MAUSER BK-27", "AIM-132 ASRAAM", "METEOR BVRAAM" },
                weaponDescs = new[]
                {
                    "27mm Revolver Cannon - 150 rounds",
                    "IR-guided AAM - x4",
                    "Radar-guided AAM - x4",
                },
                weaponIconClasses = new[] { "cannon-m61", "missile-aim9", "missile-aim120" },
                prefabName = "Typhoon",
            },
        };
    }

    void OnEnable()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            root = uiDocument.rootVisualElement.Q<VisualElement>("root");
            if (root != null)
            {
                root.style.display = DisplayStyle.None;
            }
        }
    }

    void Start()
    {
        SetupUI();
    }

    private bool isSetup = false;

    void SetupUI()
    {
        if (isSetup)
            return;

        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            Debug.LogError("[HangarController] SetupUI: uiDocument is null!");
            return;
        }

        if (uiDocument.rootVisualElement == null)
        {
            Debug.LogError("[HangarController] SetupUI: rootVisualElement is null!");
            return;
        }

        Debug.Log(
            $"[HangarController] SetupUI: visualTreeAsset = {(uiDocument.visualTreeAsset != null ? uiDocument.visualTreeAsset.name : "NULL")}"
        );
        Debug.Log(
            $"[HangarController] SetupUI: rootVisualElement childCount = {uiDocument.rootVisualElement.childCount}"
        );

        if (uiDocument.rootVisualElement.childCount == 0 && uiDocument.visualTreeAsset != null)
        {
            Debug.Log(
                "[HangarController] SetupUI: rootVisualElement is empty, cloning visualTreeAsset..."
            );

            var tempContainer = new VisualElement();
            uiDocument.visualTreeAsset.CloneTree(tempContainer);
            Debug.Log(
                $"[HangarController] SetupUI: Cloned to temp container, childCount = {tempContainer.childCount}"
            );

            if (tempContainer.childCount > 0)
            {
                foreach (var child in tempContainer.Children())
                {
                    Debug.Log(
                        $"[HangarController] SetupUI: Temp child - name='{child.name}', classes='{string.Join(", ", child.GetClasses())}'"
                    );
                }

                while (tempContainer.childCount > 0)
                {
                    var child = tempContainer[0];
                    child.RemoveFromHierarchy();
                    uiDocument.rootVisualElement.Add(child);
                }
            }

            Debug.Log(
                $"[HangarController] SetupUI: After clone, childCount = {uiDocument.rootVisualElement.childCount}"
            );
        }

        foreach (var child in uiDocument.rootVisualElement.Children())
        {
            Debug.Log(
                $"[HangarController] SetupUI child: name='{child.name}', classes='{string.Join(", ", child.GetClasses())}'"
            );
        }

        root = uiDocument.rootVisualElement.Q<VisualElement>("root");
        Debug.Log(
            $"[HangarController] SetupUI: Q(\"root\") returned {(root != null ? "FOUND" : "NULL")}"
        );

        if (root == null)
        {
            Debug.LogWarning(
                "[HangarController] SetupUI: 'root' element not found, checking if first child has hangar-root class..."
            );
            foreach (var child in uiDocument.rootVisualElement.Children())
            {
                if (child.ClassListContains("hangar-root"))
                {
                    root = child;
                    Debug.Log("[HangarController] SetupUI: Found element with hangar-root class!");
                    break;
                }
            }
        }

        if (root == null)
        {
            Debug.LogWarning(
                "[HangarController] SetupUI: Still no root, using rootVisualElement as fallback"
            );
            root = uiDocument.rootVisualElement;
        }

        if (root == null)
            return;

        backButton = root.Q<Button>("back-button");
        launchButton = root.Q<Button>("launch-button");

        modelPreviewContainer = root.Q<VisualElement>("model-preview-container");
        aircraft3DPreview = root.Q<VisualElement>("aircraft-3d-preview");

        if (modelPreviewContainer != null)
        {
            modelPreviewContainer.RegisterCallback<GeometryChangedEvent>(OnPreviewContainerResized);
        }

        tabF15 = root.Q<Button>("tab-f15");
        tabSu27 = root.Q<Button>("tab-su27");
        tabMig29 = root.Q<Button>("tab-mig29");
        tabFA18E = root.Q<Button>("tab-fa18e");
        tabHawk200 = root.Q<Button>("tab-hawk200");
        tabMig21 = root.Q<Button>("tab-mig21");
        tabTornado = root.Q<Button>("tab-tornado");
        tabRafale = root.Q<Button>("tab-rafale");
        tabTyphoon = root.Q<Button>("tab-typhoon");
        contentAircraft = root.Q<VisualElement>("content-aircraft");

        Debug.Log($"[HangarController] SetupUI element query results:");
        Debug.Log($"  backButton: {(backButton != null ? "FOUND" : "NULL")}");
        Debug.Log($"  tabF15: {(tabF15 != null ? "FOUND" : "NULL")}");
        Debug.Log($"  contentAircraft: {(contentAircraft != null ? "FOUND" : "NULL")}");

        aircraftNameLabel = root.Q<Label>("aircraft-name");
        aircraftRoleLabel = root.Q<Label>("aircraft-role");
        aircraftOriginLabel = root.Q<Label>("aircraft-origin");
        aircraftDescriptionLabel = root.Q<Label>("aircraft-description");
        speedBar = root.Q<VisualElement>("speed-bar");
        speedValueLabel = root.Q<Label>("speed-value");
        mobilityBar = root.Q<VisualElement>("mobility-bar");
        mobilityValueLabel = root.Q<Label>("mobility-value");
        armorBar = root.Q<VisualElement>("armor-bar");
        armorValueLabel = root.Q<Label>("armor-value");
        firepowerBar = root.Q<VisualElement>("firepower-bar");
        firepowerValueLabel = root.Q<Label>("firepower-value");
        armamentSection = root.Q<VisualElement>(className: "armament-section");

        setDefaultButton = root.Q<Button>("set-default-button");
        defaultIndicatorLabel = root.Q<Label>("default-indicator");

        fullscreenButton = root.Q<Button>("fullscreen-button");
        closeFullscreenButton = root.Q<Button>("close-fullscreen-button");
        fullscreenOverlay = root.Q<VisualElement>("fullscreen-overlay");
        fullscreenPreviewContainer = root.Q<VisualElement>("fullscreen-preview-container");

        if (backButton != null)
        {
            backButton.clicked += OnBackClicked;
        }

        if (launchButton != null)
        {
            launchButton.clicked += OnLaunchClicked;
        }

        if (setDefaultButton != null)
        {
            setDefaultButton.clicked += OnSetDefaultClicked;
        }

        if (tabF15 != null)
        {
            tabF15.clicked += () => SwitchToAircraft(0);
        }

        if (tabSu27 != null)
        {
            tabSu27.clicked += () => SwitchToAircraft(1);
        }

        if (tabMig29 != null)
        {
            tabMig29.clicked += () => SwitchToAircraft(2);
        }

        if (tabFA18E != null)
        {
            tabFA18E.clicked += () => SwitchToAircraft(3);
        }

        if (tabHawk200 != null)
        {
            tabHawk200.clicked += () => SwitchToAircraft(4);
        }

        if (tabMig21 != null)
        {
            tabMig21.clicked += () => SwitchToAircraft(5);
        }

        if (tabTornado != null)
        {
            tabTornado.clicked += () => SwitchToAircraft(6);
        }

        if (tabRafale != null)
        {
            tabRafale.clicked += () => SwitchToAircraft(7);
        }

        if (tabTyphoon != null)
        {
            tabTyphoon.clicked += () => SwitchToAircraft(8);
        }

        if (fullscreenButton != null)
        {
            fullscreenButton.clicked += OnFullscreenClicked;
            Debug.Log("[HangarController] Fullscreen button registered");
        }

        if (closeFullscreenButton != null)
        {
            closeFullscreenButton.clicked += OnCloseFullscreenClicked;
            Debug.Log("[HangarController] Close fullscreen button registered");
        }

        if (contentAircraft != null && tabF15 != null)
        {
            isSetup = true;
            Debug.Log("[HangarController] UI Setup complete - all elements found");
        }
        else
        {
            Debug.LogWarning(
                "[HangarController] UI Setup incomplete - critical elements not found, will retry"
            );
        }
    }

    public void Show()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuOpen();
        }

        SetupUI();

        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            Debug.LogError("[HangarController] UIDocument not found!");
            return;
        }

        if (root == null && uiDocument.rootVisualElement != null)
        {
            Debug.Log(
                $"[HangarController] Document root children count: {uiDocument.rootVisualElement.childCount}"
            );
            foreach (var child in uiDocument.rootVisualElement.Children())
            {
                Debug.Log(
                    $"[HangarController] Child: name='{child.name}', classes='{string.Join(", ", child.GetClasses())}'"
                );
            }

            root = uiDocument.rootVisualElement.Q<VisualElement>("root");
            if (root == null)
            {
                Debug.LogWarning(
                    "[HangarController] Could not find element named 'root', using document root"
                );
                root = uiDocument.rootVisualElement;
            }
            else
            {
                Debug.Log(
                    $"[HangarController] Found 'root' element with classes: {string.Join(", ", root.GetClasses())}"
                );
            }
        }

        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
            Debug.Log($"[HangarController] Root found, setting display to Flex");
            Debug.Log(
                $"[HangarController] Root class list: {string.Join(", ", root.GetClasses())}"
            );

            Debug.Log(
                $"[HangarController] contentAircraft: {(contentAircraft != null ? "FOUND" : "NULL")}"
            );
            Debug.Log($"[HangarController] tabF15: {(tabF15 != null ? "FOUND" : "NULL")}");

            if (contentAircraft != null)
            {
                contentAircraft.style.display = DisplayStyle.Flex;
                Debug.Log($"[HangarController] Set contentAircraft display to Flex");
            }

            if (modelPreview == null)
            {
                Debug.Log("[HangarController] modelPreview not assigned, auto-creating...");
                GameObject previewObj = new GameObject("AircraftModelPreview");
                modelPreview = previewObj.AddComponent<AircraftModelPreview>();
                Debug.Log("[HangarController] AircraftModelPreview auto-created");
            }

            if (modelPreview != null)
            {
                modelPreview.ShowPreview();

                if (autoCreateRawImage && modelPreviewContainer != null)
                {
                    CreateOrUpdateRawImage();
                }
            }

            if (modelPreviewRawImage != null)
            {
                modelPreviewRawImage.gameObject.SetActive(true);
            }

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            if (modelPreviewContainer != null)
            {
                lastContainerRect = modelPreviewContainer.worldBound;
            }

            SwitchToDefaultAircraft();

            Debug.Log("[HangarController] Hangar shown");
        }
        else
        {
            Debug.LogError("[HangarController] Root element not found in UXML!");
        }
    }

    void SwitchToDefaultAircraft()
    {
        string defaultAircraft = PlayerPrefs.GetString(DefaultAircraftKey, "F15");
        int defaultIndex = 0;

        if (aircraftList != null)
        {
            for (int i = 0; i < aircraftList.Count; i++)
            {
                if (aircraftList[i].prefabName == defaultAircraft)
                {
                    defaultIndex = i;
                    break;
                }
            }
        }

        SwitchToAircraft(defaultIndex);
    }

    public void Hide()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuClose();
        }

        if (root != null)
        {
            root.style.display = DisplayStyle.None;
        }

        if (modelPreview != null)
        {
            modelPreview.HidePreview();
        }

        if (modelPreviewRawImage != null)
        {
            modelPreviewRawImage.gameObject.SetActive(false);
        }
    }

    void CreateOrUpdateRawImage()
    {
        Debug.Log($"[HangarController] CreateOrUpdateRawImage called");
        Debug.Log(
            $"[HangarController] modelPreviewContainer: {(modelPreviewContainer != null ? "FOUND" : "NULL")}"
        );
        Debug.Log($"[HangarController] modelPreview: {(modelPreview != null ? "FOUND" : "NULL")}");

        if (modelPreviewContainer == null || modelPreview == null)
        {
            Debug.LogWarning("[HangarController] Cannot create RawImage - missing references!");
            return;
        }

        Rect containerRect = modelPreviewContainer.worldBound;
        Debug.Log(
            $"[HangarController] Container worldBound: x={containerRect.x}, y={containerRect.y}, w={containerRect.width}, h={containerRect.height}"
        );

        if (containerRect.width <= 0 || containerRect.height <= 0)
        {
            modelPreviewContainer.RegisterCallback<GeometryChangedEvent>(
                OnContainerGeometryChanged
            );
            return;
        }

        if (modelPreviewRawImage == null && !rawImageCreated)
        {
            rawImageCreated = true;

            GameObject canvasObj = new GameObject("ModelPreviewCanvas");
            rawImageCanvas = canvasObj.AddComponent<Canvas>();
            rawImageCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rawImageCanvas.sortingOrder = 50;

            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;

            Debug.Log("[HangarController] Created new Canvas for 3D preview");

            GameObject rawImageObj = new GameObject("ModelPreviewRawImage");
            rawImageObj.transform.SetParent(rawImageCanvas.transform, false);
            modelPreviewRawImage = rawImageObj.AddComponent<UnityEngine.UI.RawImage>();

            RenderTexture rt = modelPreview.GetRenderTexture();
            Debug.Log(
                $"[HangarController] RenderTexture from modelPreview: {(rt != null ? $"{rt.width}x{rt.height}" : "NULL")}"
            );

            modelPreviewRawImage.texture = rt;
            modelPreviewRawImage.raycastTarget = false;

            Debug.Log("[HangarController] Auto-created RawImage for 3D preview");
        }

        if (modelPreviewRawImage != null)
        {
            RectTransform rt = modelPreviewRawImage.GetComponent<RectTransform>();

            float screenHeight = Screen.height;
            float screenWidth = Screen.width;

            float panelScaleX = 1f;
            float panelScaleY = 1f;
            if (
                modelPreviewContainer.panel != null
                && modelPreviewContainer.panel.visualTree != null
            )
            {
                var panelScale = modelPreviewContainer.panel.visualTree.worldTransform.lossyScale;
                panelScaleX = panelScale.x;
                panelScaleY = panelScale.y;
                Debug.Log($"[HangarController] Panel scale: ({panelScaleX}, {panelScaleY})");
            }

            float left = containerRect.x * panelScaleX;
            float top = containerRect.y * panelScaleY;
            float width = containerRect.width * panelScaleX;
            float height = containerRect.height * panelScaleY;

            float uguiBottom = screenHeight - top - height;
            float uguiTop = screenHeight - top;
            float uguiRight = left + width;

            float normalizedLeft = left / screenWidth;
            float normalizedRight = uguiRight / screenWidth;
            float normalizedBottom = uguiBottom / screenHeight;
            float normalizedTop = uguiTop / screenHeight;

            rt.anchorMin = new Vector2(normalizedLeft, normalizedBottom);
            rt.anchorMax = new Vector2(normalizedRight, normalizedTop);
            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            modelPreviewRawImage.texture = modelPreview.GetRenderTexture();

            Debug.Log($"[HangarController] Screen: {screenWidth}x{screenHeight}");
            Debug.Log(
                $"[HangarController] UIToolkit raw rect: x={containerRect.x}, y={containerRect.y}, w={containerRect.width}, h={containerRect.height}"
            );
            Debug.Log(
                $"[HangarController] UGUI edges: left={left}, right={uguiRight}, bottom={uguiBottom}, top={uguiTop}"
            );
            Debug.Log(
                $"[HangarController] Normalized anchors: min=({normalizedLeft}, {normalizedBottom}), max=({normalizedRight}, {normalizedTop})"
            );
        }
    }

    void OnContainerGeometryChanged(GeometryChangedEvent evt)
    {
        Debug.Log($"[HangarController] Container geometry changed: {evt.newRect}");
        modelPreviewContainer.UnregisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
        CreateOrUpdateRawImage();
    }

    void OnPreviewContainerResized(GeometryChangedEvent evt)
    {
        if (modelPreviewRawImage != null && root != null && root.style.display == DisplayStyle.Flex)
        {
            UpdateRawImagePosition();
        }
    }

    void UpdatePreviewSizeIfNeeded()
    {
        if (root == null || root.style.display == DisplayStyle.None)
            return;

        if (modelPreviewRawImage == null || modelPreviewContainer == null)
            return;

        bool screenSizeChanged = Screen.width != lastScreenWidth || Screen.height != lastScreenHeight;

        Rect currentRect = modelPreviewContainer.worldBound;
        bool containerChanged = currentRect.x != lastContainerRect.x ||
                                currentRect.y != lastContainerRect.y ||
                                currentRect.width != lastContainerRect.width ||
                                currentRect.height != lastContainerRect.height;

        if (screenSizeChanged || containerChanged)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            lastContainerRect = currentRect;

            UpdateRawImagePosition();
        }
    }

    void UpdateRawImagePosition()
    {
        if (modelPreviewRawImage == null || modelPreviewContainer == null)
            return;

        Rect containerRect = modelPreviewContainer.worldBound;

        if (containerRect.width <= 0 || containerRect.height <= 0)
            return;

        RectTransform rt = modelPreviewRawImage.GetComponent<RectTransform>();

        float screenHeight = Screen.height;
        float screenWidth = Screen.width;

        float panelScaleX = 1f;
        float panelScaleY = 1f;
        if (modelPreviewContainer.panel != null && modelPreviewContainer.panel.visualTree != null)
        {
            var panelScale = modelPreviewContainer.panel.visualTree.worldTransform.lossyScale;
            panelScaleX = panelScale.x;
            panelScaleY = panelScale.y;
        }

        float left = containerRect.x * panelScaleX;
        float top = containerRect.y * panelScaleY;
        float width = containerRect.width * panelScaleX;
        float height = containerRect.height * panelScaleY;

        float uguiBottom = screenHeight - top - height;
        float uguiTop = screenHeight - top;
        float uguiRight = left + width;

        float normalizedLeft = left / screenWidth;
        float normalizedRight = uguiRight / screenWidth;
        float normalizedBottom = uguiBottom / screenHeight;
        float normalizedTop = uguiTop / screenHeight;

        rt.anchorMin = new Vector2(normalizedLeft, normalizedBottom);
        rt.anchorMax = new Vector2(normalizedRight, normalizedTop);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        if (modelPreview != null)
        {
            modelPreviewRawImage.texture = modelPreview.GetRenderTexture();
        }
    }

    void OnBackClicked()
    {
        Hide();

        var mainMenu = FindFirstObjectByType<MainMenuController>();
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
            Debug.Log("[HangarController] Returning to main menu");
        }
        else
        {
            Debug.LogWarning("[HangarController] MainMenuController not found!");
        }
    }

    void OnLaunchClicked()
    {
        Hide();

        var mainMenu = FindFirstObjectByType<MainMenuController>();
        if (mainMenu != null)
        {
            mainMenu.StartGameFromHangar();
        }
        else
        {
            Debug.LogWarning("[HangarController] MainMenuController not found for launch");
        }
    }

    void SwitchToAircraft(int index)
    {
        tabF15?.RemoveFromClassList("active");
        tabSu27?.RemoveFromClassList("active");
        tabMig29?.RemoveFromClassList("active");
        tabFA18E?.RemoveFromClassList("active");
        tabHawk200?.RemoveFromClassList("active");
        tabMig21?.RemoveFromClassList("active");
        tabTornado?.RemoveFromClassList("active");
        tabRafale?.RemoveFromClassList("active");
        tabTyphoon?.RemoveFromClassList("active");

        switch (index)
        {
            case 0:
                tabF15?.AddToClassList("active");
                break;
            case 1:
                tabSu27?.AddToClassList("active");
                break;
            case 2:
                tabMig29?.AddToClassList("active");
                break;
            case 3:
                tabFA18E?.AddToClassList("active");
                break;
            case 4:
                tabHawk200?.AddToClassList("active");
                break;
            case 5:
                tabMig21?.AddToClassList("active");
                break;
            case 6:
                tabTornado?.AddToClassList("active");
                break;
            case 7:
                tabRafale?.AddToClassList("active");
                break;
            case 8:
                tabTyphoon?.AddToClassList("active");
                break;
        }

        SelectAircraft(index);

        Debug.Log($"[HangarController] Switched to aircraft tab: {index}");
    }

    void SelectAircraft(int index)
    {
        if (aircraftList == null || index < 0 || index >= aircraftList.Count)
            return;

        selectedAircraftIndex = index;
        var aircraft = aircraftList[index];
        Debug.Log($"[HangarController] Selected aircraft: {aircraft.name}");

        PlayerPrefs.SetString(AircraftSelectionApplier.SelectedAircraftKey, aircraft.prefabName);
        PlayerPrefs.Save();

        UpdateAircraftDisplay(aircraft);
        UpdateDefaultIndicator(aircraft.prefabName);

        if (modelPreview != null)
        {
            modelPreview.LoadAircraftByName(aircraft.prefabName);
        }
    }

    void UpdateDefaultIndicator(string currentPrefabName)
    {
        string defaultAircraft = PlayerPrefs.GetString(DefaultAircraftKey, "F15");
        bool isDefault = currentPrefabName == defaultAircraft;

        if (defaultIndicatorLabel != null)
        {
            defaultIndicatorLabel.text = isDefault ? "DEFAULT AIRCRAFT" : "";
        }

        if (setDefaultButton != null)
        {
            setDefaultButton.style.display = isDefault ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

    void OnSetDefaultClicked()
    {
        if (
            aircraftList == null
            || selectedAircraftIndex < 0
            || selectedAircraftIndex >= aircraftList.Count
        )
            return;

        var aircraft = aircraftList[selectedAircraftIndex];
        PlayerPrefs.SetString(DefaultAircraftKey, aircraft.prefabName);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        UpdateDefaultIndicator(aircraft.prefabName);
        Debug.Log(
            $"[HangarController] Set default aircraft to: {aircraft.name} ({aircraft.prefabName})"
        );
    }

    void UpdateAircraftDisplay(AircraftData aircraft)
    {
        if (aircraftNameLabel != null)
            aircraftNameLabel.text = aircraft.name;

        if (aircraftRoleLabel != null)
            aircraftRoleLabel.text = aircraft.role;

        if (aircraftOriginLabel != null)
            aircraftOriginLabel.text = aircraft.origin;

        if (aircraftDescriptionLabel != null)
            aircraftDescriptionLabel.text = aircraft.description;

        if (speedBar != null)
            speedBar.style.width = new Length(aircraft.speedPercent, LengthUnit.Percent);
        if (speedValueLabel != null)
            speedValueLabel.text = aircraft.speedValue;

        if (mobilityBar != null)
            mobilityBar.style.width = new Length(aircraft.mobilityPercent, LengthUnit.Percent);
        if (mobilityValueLabel != null)
            mobilityValueLabel.text = aircraft.mobilityValue;

        if (armorBar != null)
            armorBar.style.width = new Length(aircraft.armorPercent, LengthUnit.Percent);
        if (armorValueLabel != null)
            armorValueLabel.text = aircraft.armorValue;

        if (firepowerBar != null)
            firepowerBar.style.width = new Length(aircraft.firepowerPercent, LengthUnit.Percent);
        if (firepowerValueLabel != null)
            firepowerValueLabel.text = aircraft.firepowerValue;

        UpdateWeaponsDisplay(aircraft);
    }

    void UpdateWeaponsDisplay(AircraftData aircraft)
    {
        if (armamentSection == null)
            return;

        var weaponRows = armamentSection.Query<VisualElement>(className: "weapon-row").ToList();

        for (int i = 0; i < weaponRows.Count && i < aircraft.weaponNames.Length; i++)
        {
            var row = weaponRows[i];
            var nameLabel = row.Q<Label>(className: "weapon-name");
            var descLabel = row.Q<Label>(className: "weapon-desc");
            var iconElement = row.Q<VisualElement>(className: "weapon-icon");

            if (nameLabel != null)
                nameLabel.text = aircraft.weaponNames[i];
            if (descLabel != null)
                descLabel.text = aircraft.weaponDescs[i];

            if (
                iconElement != null
                && aircraft.weaponIconClasses != null
                && i < aircraft.weaponIconClasses.Length
            )
            {
                iconElement.RemoveFromClassList("cannon-icon");
                iconElement.RemoveFromClassList("missile-icon");
                iconElement.RemoveFromClassList("cannon-m61");
                iconElement.RemoveFromClassList("missile-aim9");
                iconElement.RemoveFromClassList("missile-aim120");
                iconElement.RemoveFromClassList("cannon-gsh30");
                iconElement.RemoveFromClassList("missile-r73");
                iconElement.RemoveFromClassList("missile-r27");
                iconElement.RemoveFromClassList("missile-r77");

                iconElement.AddToClassList(aircraft.weaponIconClasses[i]);
            }
        }
    }

    void UpdateLaunchButtonPulse()
    {
        if (launchButton == null || root == null || root.style.display == DisplayStyle.None)
            return;

        launchButtonPulseTime += Time.unscaledDeltaTime * 2f;

        float pulse = (Mathf.Sin(launchButtonPulseTime) + 1f) / 2f;

        byte r = (byte)Mathf.Lerp(0, 150, pulse);
        byte g = (byte)Mathf.Lerp(200, 255, pulse);
        byte b = (byte)Mathf.Lerp(255, 255, pulse);
        byte a = (byte)Mathf.Lerp(200, 255, pulse);

        var borderColor = new StyleColor(new Color32(r, g, b, a));
        launchButton.style.borderTopColor = borderColor;
        launchButton.style.borderBottomColor = borderColor;
        launchButton.style.borderLeftColor = borderColor;
        launchButton.style.borderRightColor = borderColor;

        float bgAlpha = Mathf.Lerp(0.3f, 0.5f, pulse);
        launchButton.style.backgroundColor = new StyleColor(new Color(0f, 0.78f, 1f, bgAlpha));
    }

    void Update()
    {
        UpdateLaunchButtonPulse();
        UpdatePreviewSizeIfNeeded();

        if (isFullscreen && UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("[HangarController] ESC key pressed - closing fullscreen");
                OnCloseFullscreenClicked();
            }
        }

        if (isFullscreen && modelPreview != null)
        {
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    isFullscreenDragging = true;
                    lastFullscreenMousePos = mouse.position.ReadValue();
                    modelPreview.SetDragging(true);
                    Debug.Log("[HangarController] Fullscreen drag started");
                }

                if (mouse.leftButton.wasReleasedThisFrame)
                {
                    isFullscreenDragging = false;
                    modelPreview.SetDragging(false);
                    Debug.Log("[HangarController] Fullscreen drag ended");
                }

                if (isFullscreenDragging)
                {
                    Vector2 currentPos = mouse.position.ReadValue();
                    Vector2 delta = currentPos - lastFullscreenMousePos;

                    if (delta.sqrMagnitude > 0.01f)
                    {
                        modelPreview.RotateModel(delta.x, delta.y);
                    }

                    lastFullscreenMousePos = currentPos;
                }

                Vector2 scroll = mouse.scroll.ReadValue();
                if (Mathf.Abs(scroll.y) > 0.01f)
                {
                    modelPreview.ZoomModel(scroll.y);
                    Debug.Log($"[HangarController] Fullscreen zoom: {scroll.y}");
                }
            }
        }
    }

    void OnFullscreenClicked()
    {
        Debug.Log(
            $"[HangarController] OnFullscreenClicked called - fullscreenOverlay={(fullscreenOverlay != null ? "EXISTS" : "NULL")}, modelPreview={(modelPreview != null ? "EXISTS" : "NULL")}"
        );

        if (fullscreenOverlay == null || modelPreview == null)
        {
            Debug.LogWarning("[HangarController] Cannot open fullscreen - missing references!");
            return;
        }

        Debug.Log("[HangarController] Opening fullscreen preview");

        if (AudioManager.Instance != null)
        {
            Debug.Log("[HangarController] Playing button click sound");
            AudioManager.Instance.PlayButtonClick();
        }

        isFullscreen = true;
        fullscreenOverlay.style.display = DisplayStyle.Flex;
        Debug.Log($"[HangarController] Fullscreen overlay displayed - isFullscreen={isFullscreen}");

        CreateFullscreenRawImage();
    }

    void OnCloseFullscreenClicked()
    {
        Debug.Log(
            $"[HangarController] OnCloseFullscreenClicked called - fullscreenOverlay={(fullscreenOverlay != null ? "EXISTS" : "NULL")}"
        );

        if (fullscreenOverlay == null)
        {
            Debug.LogWarning("[HangarController] Cannot close fullscreen - overlay is null!");
            return;
        }

        Debug.Log("[HangarController] Closing fullscreen preview");

        if (AudioManager.Instance != null)
        {
            Debug.Log("[HangarController] Playing button click sound");
            AudioManager.Instance.PlayButtonClick();
        }

        isFullscreen = false;
        fullscreenOverlay.style.display = DisplayStyle.None;
        Debug.Log($"[HangarController] Fullscreen overlay hidden - isFullscreen={isFullscreen}");

        if (fullscreenRawImage != null)
        {
            fullscreenRawImage.gameObject.SetActive(false);
            Debug.Log("[HangarController] Fullscreen RawImage deactivated");
        }
    }

    void CreateFullscreenRawImage()
    {
        Debug.Log(
            $"[HangarController] CreateFullscreenRawImage called - modelPreview={(modelPreview != null ? "EXISTS" : "NULL")}, fullscreenPreviewContainer={(fullscreenPreviewContainer != null ? "EXISTS" : "NULL")}"
        );

        if (modelPreview == null || fullscreenPreviewContainer == null)
        {
            Debug.LogWarning(
                "[HangarController] Cannot create fullscreen RawImage - missing references!"
            );
            return;
        }

        if (fullscreenRawImageCanvas == null)
        {
            Debug.Log("[HangarController] Creating new fullscreen canvas and RawImage...");

            GameObject canvasObj = new GameObject("FullscreenModelPreviewCanvas");
            fullscreenRawImageCanvas = canvasObj.AddComponent<Canvas>();
            fullscreenRawImageCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fullscreenRawImageCanvas.sortingOrder = 100;

            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;

            GameObject rawImageObj = new GameObject("FullscreenModelRawImage");
            rawImageObj.transform.SetParent(fullscreenRawImageCanvas.transform, false);
            fullscreenRawImage = rawImageObj.AddComponent<UnityEngine.UI.RawImage>();
            fullscreenRawImage.raycastTarget = false;

            Debug.Log("[HangarController] Created fullscreen RawImage canvas - sortingOrder=100");
        }
        else
        {
            Debug.Log("[HangarController] Reusing existing fullscreen canvas");
        }

        if (fullscreenRawImage != null)
        {
            RectTransform rt = fullscreenRawImage.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.05f, 0.1f);
            rt.anchorMax = new Vector2(0.95f, 0.9f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var renderTexture = modelPreview.GetRenderTexture();
            fullscreenRawImage.texture = renderTexture;
            fullscreenRawImage.gameObject.SetActive(true);

            Debug.Log($"[HangarController] Fullscreen RawImage configured:");
            Debug.Log($"  - Screen size: {Screen.width}x{Screen.height}");
            Debug.Log($"  - Anchors: min=(0.05, 0.1), max=(0.95, 0.9)");
            Debug.Log(
                $"  - RenderTexture: {(renderTexture != null ? $"{renderTexture.width}x{renderTexture.height}" : "NULL")}"
            );
            Debug.Log($"  - RawImage active: {fullscreenRawImage.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogError("[HangarController] fullscreenRawImage is null after creation!");
        }
    }

    void OnDestroy()
    {
        if (fullscreenRawImageCanvas != null)
        {
            Destroy(fullscreenRawImageCanvas.gameObject);
        }

        if (rawImageCanvas != null)
        {
            Destroy(rawImageCanvas.gameObject);
        }

        if (modelPreviewContainer != null)
        {
            modelPreviewContainer.UnregisterCallback<GeometryChangedEvent>(OnPreviewContainerResized);
        }

        if (fullscreenButton != null)
        {
            fullscreenButton.clicked -= OnFullscreenClicked;
        }
        if (closeFullscreenButton != null)
        {
            closeFullscreenButton.clicked -= OnCloseFullscreenClicked;
        }
    }
}
