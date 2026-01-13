using System.Collections.Generic;
using UnityEngine;

public static class AircraftSelectionApplier
{
    public const string SelectedAircraftKey = "SelectedAircraft";

    private static readonly Dictionary<string, Vector3> AircraftCameraOffsets = new Dictionary<
        string,
        Vector3
    >
    {
        { "F15", new Vector3(0, 6, -20) },
        { "Su27", new Vector3(0, 8, -26) },
        { "Mig29", new Vector3(0, 8, -26) },
        { "fa18e", new Vector3(0, 7, -22) },
        { "Hawk_200", new Vector3(0, 5, -16) },
        { "mig21", new Vector3(0, 5, -18) },
        { "panavia-tornado", new Vector3(0, 7, -24) },
        { "rafalemf3", new Vector3(0, 6, -20) },
        { "Typhoon", new Vector3(0, 7, -22) },
    };

    public static Vector3 GetCameraOffsetForAircraft(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
            return Vector3.zero;

        if (AircraftCameraOffsets.TryGetValue(prefabName, out Vector3 offset))
            return offset;

        return Vector3.zero;
    }

    public static void ApplySelectedAircraft(Plane plane)
    {
        if (plane == null)
            return;

        string prefabName = PlayerPrefs.GetString(SelectedAircraftKey, "F15");
        ApplySelectedAircraft(plane, prefabName);
    }

    public static void ApplySelectedAircraft(Plane plane, string prefabName)
    {
        if (plane == null)
            return;

        string actualAircraftType = string.IsNullOrWhiteSpace(prefabName) ? "F15" : prefabName;
        plane.SetAircraftType(actualAircraftType);

        AircraftTuningApplier.Apply(plane, prefabName);
        if (string.IsNullOrWhiteSpace(prefabName) || prefabName == "F15")
            return;

        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
        if (prefab == null)
        {
            Debug.LogWarning(
                $"[AircraftSelectionApplier] Prefab not found at Resources/Prefabs/{prefabName}"
            );
            return;
        }

        Transform planeTransform = plane.transform;
        Transform existingSwap = planeTransform.Find("SwappedModel");

        Bounds baseBounds = CalculateLocalBounds(planeTransform, planeTransform, existingSwap);

        if (existingSwap != null)
        {
            Object.Destroy(existingSwap.gameObject);
        }

        GameObject modelInstance = Object.Instantiate(prefab, planeTransform);
        modelInstance.name = "SwappedModel";
        Transform modelTransform = modelInstance.transform;
        modelTransform.localPosition = Vector3.zero;
        modelTransform.localRotation = Quaternion.identity;
        modelTransform.localScale = Vector3.one;

        int planeLayer = planeTransform.gameObject.layer;
        SetLayerRecursively(modelInstance, planeLayer);
        Debug.Log(
            $"[AircraftSelectionApplier] Set swapped model to layer {planeLayer} ({LayerMask.LayerToName(planeLayer)})"
        );

        Bounds modelBounds = CalculateLocalBounds(modelTransform, planeTransform, null);
        float scaleFactor = 1f;
        if (baseBounds.size.z > 0.01f && modelBounds.size.z > 0.01f)
        {
            scaleFactor = baseBounds.size.z / modelBounds.size.z;
        }
        modelTransform.localScale = Vector3.one * scaleFactor;
        Debug.Log(
            $"[AircraftSelectionApplier] Model scale factor: {scaleFactor}, baseBounds: {baseBounds.size}, modelBounds: {modelBounds.size}"
        );

        modelBounds = CalculateLocalBounds(modelTransform, planeTransform, null);

        if (baseBounds.size.sqrMagnitude > 0.0001f && modelBounds.size.sqrMagnitude > 0.0001f)
        {
            Vector3 offset = baseBounds.center - modelBounds.center;
            modelTransform.localPosition = offset;
            modelBounds = CalculateLocalBounds(modelTransform, planeTransform, null);
        }

        DisableNonModelRenderers(planeTransform, modelTransform);
        ApplyAutoMounts(plane, planeTransform, modelBounds);
        DisableAfterburners(planeTransform);
    }

    private static Bounds CalculateLocalBounds(
        Transform root,
        Transform relativeTo,
        Transform excludeRoot
    )
    {
        bool hasBounds = false;
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;

        var renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            if (renderer == null)
                continue;
            if (excludeRoot != null && renderer.transform.IsChildOf(excludeRoot))
                continue;

            if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer))
                continue;

            Bounds b = renderer.bounds;
            Vector3 c = b.center;
            Vector3 e = b.extents;

            for (int xi = -1; xi <= 1; xi += 2)
            {
                for (int yi = -1; yi <= 1; yi += 2)
                {
                    for (int zi = -1; zi <= 1; zi += 2)
                    {
                        Vector3 corner = new Vector3(
                            c.x + e.x * xi,
                            c.y + e.y * yi,
                            c.z + e.z * zi
                        );
                        Vector3 local =
                            relativeTo != null ? relativeTo.InverseTransformPoint(corner) : corner;

                        if (!hasBounds)
                        {
                            min = local;
                            max = local;
                            hasBounds = true;
                        }
                        else
                        {
                            min = Vector3.Min(min, local);
                            max = Vector3.Max(max, local);
                        }
                    }
                }
            }
        }

        if (!hasBounds)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        Bounds result = new Bounds();
        result.SetMinMax(min, max);
        return result;
    }

    private static void DisableNonModelRenderers(Transform root, Transform modelRoot)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            if (renderer == null)
                continue;
            if (modelRoot != null && renderer.transform.IsChildOf(modelRoot))
                continue;
            renderer.enabled = false;
        }

        if (modelRoot != null)
        {
            FixModelMaterials(modelRoot);
        }
    }

    private static void FixModelMaterials(Transform modelRoot)
    {
        var renderers = modelRoot.GetComponentsInChildren<Renderer>(true);

        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        Shader urpSimpleLit = Shader.Find("Universal Render Pipeline/Simple Lit");
        Shader standardShader = Shader.Find("Standard");

        Shader targetShader =
            urpLitShader != null
                ? urpLitShader
                : (urpSimpleLit != null ? urpSimpleLit : standardShader);

        Debug.Log(
            $"[AircraftSelectionApplier] Using shader: {(targetShader != null ? targetShader.name : "NULL")}"
        );

        foreach (var renderer in renderers)
        {
            if (renderer == null)
                continue;

            var materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                var mat = materials[i];
                if (mat == null)
                    continue;

                Color mainColor = Color.white;
                Texture mainTex = null;

                if (mat.HasProperty("_Color"))
                    mainColor = mat.GetColor("_Color");
                else if (mat.HasProperty("_BaseColor"))
                    mainColor = mat.GetColor("_BaseColor");

                if (mat.HasProperty("_MainTex"))
                    mainTex = mat.GetTexture("_MainTex");
                else if (mat.HasProperty("_BaseMap"))
                    mainTex = mat.GetTexture("_BaseMap");

                if (targetShader != null && mat.shader != targetShader)
                {
                    mat.shader = targetShader;
                }

                if (mat.shader.name.Contains("Universal Render Pipeline"))
                {
                    if (mat.HasProperty("_Surface"))
                        mat.SetFloat("_Surface", 0);

                    if (mat.HasProperty("_BaseColor"))
                        mat.SetColor("_BaseColor", mainColor);
                    if (mat.HasProperty("_BaseMap") && mainTex != null)
                        mat.SetTexture("_BaseMap", mainTex);

                    if (mat.HasProperty("_AlphaClip"))
                        mat.SetFloat("_AlphaClip", 0);

                    if (mat.HasProperty("_Blend"))
                        mat.SetFloat("_Blend", 0);

                    mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.DisableKeyword("_ALPHAMODULATE_ON");
                    mat.EnableKeyword("_SURFACE_TYPE_OPAQUE");
                }
                else
                {
                    mat.SetFloat("_Mode", 0);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.SetFloat("_ZTest", 4);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                    if (mat.HasProperty("_Color"))
                        mat.SetColor("_Color", mainColor);
                    if (mat.HasProperty("_MainTex") && mainTex != null)
                        mat.SetTexture("_MainTex", mainTex);
                }

                mat.renderQueue = 2000;
            }

            renderer.materials = materials;
        }

        Debug.Log(
            $"[AircraftSelectionApplier] Fixed materials on {renderers.Length} renderers in swapped model"
        );
    }

    private static void ApplyAutoMounts(Plane plane, Transform planeTransform, Bounds bounds)
    {
        Transform hardpointRoot = planeTransform.Find("AutoHardpoints");
        if (hardpointRoot != null)
        {
            Object.Destroy(hardpointRoot.gameObject);
        }

        GameObject hardpointsGO = new GameObject("AutoHardpoints");
        hardpointsGO.transform.SetParent(planeTransform, false);

        float x = Mathf.Abs(bounds.extents.x) * 0.6f;
        float y = bounds.min.y - (bounds.size.y * 0.05f);
        float z = Mathf.Lerp(bounds.min.z, bounds.max.z, 0.25f);
        float centerX = bounds.center.x;

        Transform left = new GameObject("AutoHardpoint_L").transform;
        left.SetParent(hardpointsGO.transform, false);
        left.localPosition = new Vector3(centerX - x, y, z);

        Transform right = new GameObject("AutoHardpoint_R").transform;
        right.SetParent(hardpointsGO.transform, false);
        right.localPosition = new Vector3(centerX + x, y, z);

        plane.SetHardpoints(new List<Transform> { left, right });

        Transform cannonPoint = planeTransform.Find("AutoCannonSpawn");
        if (cannonPoint != null)
        {
            Object.Destroy(cannonPoint.gameObject);
        }

        Transform cannon = new GameObject("AutoCannonSpawn").transform;
        cannon.SetParent(planeTransform, false);
        float cannonY = bounds.center.y - (bounds.size.y * 0.05f);
        float cannonZ = bounds.max.z - (bounds.size.z * 0.05f);
        cannon.localPosition = new Vector3(bounds.center.x, cannonY, cannonZ);
        plane.SetCannonSpawnPoint(cannon);

        SetupMissileGraphics(plane, planeTransform);
    }

    private static void SetupMissileGraphics(Plane plane, Transform planeTransform)
    {
        var animation = plane.GetComponent<PlaneAnimation>();
        if (animation == null)
            return;

        Transform swappedModel = planeTransform.Find("SwappedModel");
        if (swappedModel == null)
            return;

        var missileGraphics = new List<GameObject>();
        FindMissileGraphicsRecursive(swappedModel, missileGraphics);

        if (missileGraphics.Count > 0)
        {
            Debug.Log(
                $"[AircraftSelectionApplier] Found {missileGraphics.Count} missile graphics in swapped model"
            );
            animation.SetMissileGraphics(missileGraphics);
        }
        else
        {
            animation.SetMissileGraphics(new List<GameObject>());
        }
    }

    private static void FindMissileGraphicsRecursive(Transform parent, List<GameObject> results)
    {
        foreach (Transform child in parent)
        {
            string nameLower = child.name.ToLower();

            if (
                nameLower.Contains("missile")
                || nameLower.Contains("rocket")
                || nameLower.Contains("aim")
                || nameLower.Contains("r-27")
                || nameLower.Contains("r-73")
                || nameLower.Contains("r27")
                || nameLower.Contains("r73")
                || nameLower.Contains("weapon")
                || nameLower.Contains("ordnance")
                || nameLower.Contains("munition")
                || (nameLower.StartsWith("cylinder") && !nameLower.Contains("rotator"))
            )
            {
                if (
                    child.GetComponent<Renderer>() != null
                    || child.GetComponentInChildren<Renderer>() != null
                )
                {
                    results.Add(child.gameObject);
                }
            }

            FindMissileGraphicsRecursive(child, results);
        }
    }

    private static void DisableAfterburners(Transform root)
    {
        var animations = root.GetComponentsInChildren<PlaneAnimation>(true);
        foreach (var animation in animations)
        {
            if (animation != null)
            {
                animation.DisableAfterburners();
            }
        }

        var effects = root.GetComponentsInChildren<AfterburnerEffect>(true);
        foreach (var effect in effects)
        {
            if (effect != null)
            {
                effect.enabled = false;
            }
        }
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
