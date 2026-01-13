using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaneHUD : MonoBehaviour
{
    [SerializeField]
    float updateRate;

    [SerializeField]
    Color normalColor;

    [SerializeField]
    Color lockColor;

    [SerializeField]
    List<GameObject> helpDialogs;

    [SerializeField]
    Compass compass;

    [SerializeField]
    PitchLadder pitchLadder;

    [SerializeField]
    Bar throttleBar;

    [SerializeField]
    Transform hudCenter;

    [SerializeField]
    Transform velocityMarker;

    [SerializeField]
    TMP_Text airspeed;

    [SerializeField]
    TMP_Text aoaIndicator;

    [SerializeField]
    TMP_Text gforceIndicator;

    [SerializeField]
    TMP_Text altitude;

    [SerializeField]
    Bar healthBar;

    [SerializeField]
    TMP_Text healthText;

    [Header("Enemy UI (Optional)")]
    [SerializeField]
    Bar enemyHealthBar;

    [SerializeField]
    TMP_Text enemyHealthText;

    [SerializeField]
    float enemyHealthFillSpeed = 0f;

    [SerializeField]
    Transform targetBox;

    [SerializeField]
    TMP_Text targetName;

    [SerializeField]
    TMP_Text targetRange;

    [SerializeField]
    Transform missileLock;

    [SerializeField]
    Transform reticle;

    [SerializeField]
    RectTransform reticleLine;

    [SerializeField]
    RectTransform targetArrow;

    [SerializeField]
    RectTransform missileArrow;

    [SerializeField]
    float targetArrowThreshold;

    [SerializeField]
    float missileArrowThreshold;

    [SerializeField]
    float cannonRange;

    [SerializeField]
    float bulletSpeed;

    [SerializeField]
    GameObject aiMessage;

    [SerializeField]
    List<Graphic> missileWarningGraphics;

    Plane plane;
    AIController aiController;
    Target selfTarget;
    Transform planeTransform;
    new Camera camera;
    Transform cameraTransform;

    GameObject hudCenterGO;
    GameObject velocityMarkerGO;
    GameObject targetBoxGO;
    Image targetBoxImage;
    GameObject missileLockGO;
    Image missileLockImage;
    GameObject reticleGO;
    GameObject targetArrowGO;
    GameObject missileArrowGO;

    float lastUpdateTime;
    float enemyHealthDisplayValue = 1f;
    Target lastEnemyTarget;

    Text legacyHealthText;
    Text legacyEnemyHealthText;
    Text legacyAirspeed;
    Text legacyAOA;
    Text legacyGForce;
    Text legacyAltitude;
    Text legacyTargetName;
    Text legacyTargetRange;

    const float metersToKnots = 1.94384f;
    const float metersToFeet = 3.28084f;

    void Start()
    {
        AutoAssignReferences();
        ValidateReferences();

        if (hudCenter != null)
            hudCenterGO = hudCenter.gameObject;
        if (velocityMarker != null)
            velocityMarkerGO = velocityMarker.gameObject;
        if (targetBox != null)
        {
            targetBoxGO = targetBox.gameObject;
            targetBoxImage = targetBox.GetComponent<Image>();
        }
        if (missileLock != null)
        {
            missileLockGO = missileLock.gameObject;
            missileLockImage = missileLock.GetComponent<Image>();
        }
        if (reticle != null)
            reticleGO = reticle.gameObject;
        if (targetArrow != null)
            targetArrowGO = targetArrow.gameObject;
        if (missileArrow != null)
            missileArrowGO = missileArrow.gameObject;

        if (enemyHealthBar == null)
        {
            var bars = GetComponentsInChildren<Bar>(true);
            foreach (var bar in bars)
            {
                if (bar != null && bar.gameObject.name == "enemy health bar")
                {
                    enemyHealthBar = bar;
                    break;
                }
            }
        }

        if (enemyHealthBar == null)
        {
            var enemyHealthGO = GameObject.Find("enemy health bar");
            if (enemyHealthGO != null)
                enemyHealthBar = enemyHealthGO.GetComponent<Bar>();
        }

        if (enemyHealthBar != null)
            enemyHealthBar.gameObject.SetActive(false);

        if (legacyHealthText != null)
            Debug.Log($"[PlaneHUD] Found legacy health text: {legacyHealthText.gameObject.name}");
        if (legacyEnemyHealthText != null)
            Debug.Log(
                $"[PlaneHUD] Found legacy enemy health text: {legacyEnemyHealthText.gameObject.name}"
            );
    }

    void AutoAssignReferences()
    {
        if (hudCenter == null)
            hudCenter = FindTransform("HUD Center", "Center", "Boresight");
        if (velocityMarker == null)
            velocityMarker = FindTransform("Velocity Marker", "Flight Path Marker");

        var allTMP = GetComponentsInChildren<TMP_Text>(true);
        var allLegacy = GetComponentsInChildren<Text>(true);

        Debug.Log(
            $"[PlaneHUD] Found {allTMP.Length} TMP components and {allLegacy.Length} Legacy Text components"
        );
        foreach (var t in allLegacy)
            Debug.Log(
                $"[PlaneHUD]   Legacy: '{t.name}' text='{t.text?.Substring(0, Mathf.Min(20, t.text?.Length ?? 0))}'"
            );

        void SmartFind(
            ref TMP_Text tmpField,
            ref Text legacyField,
            string[] names,
            string[] keywords
        )
        {
            if (tmpField != null || legacyField != null)
                return;

            foreach (var name in names)
            {
                if (name.Contains("/"))
                {
                    var parts = name.Split('/');

                    var parent = FindChildRecursive(transform, parts[0]);
                    if (parent != null)
                    {
                        var child = FindChildRecursive(parent, parts[1]);
                        if (child != null)
                        {
                            var tmpComp = child.GetComponent<TMP_Text>();
                            if (tmpComp != null)
                            {
                                tmpField = tmpComp;
                                Debug.Log($"[PlaneHUD]  Found TMP at path '{name}'");
                                return;
                            }
                            var legacyComp = child.GetComponent<Text>();
                            if (legacyComp != null)
                            {
                                legacyField = legacyComp;
                                Debug.Log($"[PlaneHUD]  Found Legacy Text at path '{name}'");
                                return;
                            }
                        }

                        foreach (Transform c in parent)
                        {
                            var tmpComp = c.GetComponent<TMP_Text>();
                            if (tmpComp != null)
                            {
                                tmpField = tmpComp;
                                Debug.Log(
                                    $"[PlaneHUD]  Found TMP in parent '{parts[0]}' child '{c.name}'"
                                );
                                return;
                            }
                            var legacyComp = c.GetComponent<Text>();
                            if (legacyComp != null)
                            {
                                legacyField = legacyComp;
                                Debug.Log(
                                    $"[PlaneHUD]  Found Legacy Text in parent '{parts[0]}' child '{c.name}'"
                                );
                                return;
                            }
                        }
                    }
                }
            }

            foreach (var t in allLegacy)
            {
                if (t.transform.parent != null && MatchesAny(t.transform.parent.name, names))
                {
                    legacyField = t;
                    Debug.Log(
                        $"[PlaneHUD]  Auto-assigned Legacy Text '{t.name}' (parent: '{t.transform.parent.name}') to {names[0]} by Parent Match"
                    );
                    return;
                }
            }

            foreach (var t in allTMP)
            {
                if (t.transform.parent != null && MatchesAny(t.transform.parent.name, names))
                {
                    tmpField = t;
                    Debug.Log(
                        $"[PlaneHUD]  Auto-assigned TMP '{t.name}' (parent: '{t.transform.parent.name}') to {names[0]} by Parent Match"
                    );
                    return;
                }
            }

            foreach (var t in allLegacy)
            {
                if (MatchesAny(t.name, names))
                {
                    legacyField = t;
                    Debug.Log(
                        $"[PlaneHUD]  Auto-assigned Legacy Text '{t.name}' to {names[0]} by Name Match"
                    );
                    return;
                }
            }

            foreach (var t in allTMP)
            {
                if (MatchesAny(t.name, names))
                {
                    tmpField = t;
                    Debug.Log(
                        $"[PlaneHUD]  Auto-assigned TMP '{t.name}' to {names[0]} by Name Match"
                    );
                    return;
                }
            }

            foreach (var t in allLegacy)
            {
                if (MatchesAny(t.text, keywords))
                {
                    legacyField = t;
                    Debug.Log(
                        $"[PlaneHUD]  Auto-assigned Legacy Text '{t.name}' (text: '{t.text}') to {names[0]} by Content Match"
                    );
                    return;
                }
            }

            foreach (var t in allTMP)
            {
                if (MatchesAny(t.text, keywords))
                {
                    tmpField = t;
                    Debug.Log(
                        $"[PlaneHUD]  Auto-assigned TMP '{t.name}' (text: '{t.text}') to {names[0]} by Content Match"
                    );
                    return;
                }
            }

            Debug.LogWarning(
                $"[PlaneHUD]  Could not find any Text component for '{names[0]}' (searched names: {string.Join(", ", names)})"
            );
        }

        bool MatchesAny(string source, string[] targets)
        {
            if (string.IsNullOrEmpty(source))
                return false;
            foreach (var t in targets)
                if (source.IndexOf(t, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }

        SmartFind(
            ref airspeed,
            ref legacyAirspeed,
            new[] { "Airspeed/Text", "Airspeed", "Speed" },
            new[] { "knots", "kph", "mph", "kn", "spd", "S:" }
        );
        SmartFind(
            ref aoaIndicator,
            ref legacyAOA,
            new[] { "AOA/Text", "AOA", "Angle" },
            new[] { "aoa", "AOA", "deg" }
        );
        SmartFind(
            ref gforceIndicator,
            ref legacyGForce,
            new[] { "GForce/Text", "GForce", "G-Force", "G_Force" },
            new[] { "g-force", "force", "g's", "0.0 g", "1.0 g", " g" }
        );
        SmartFind(
            ref altitude,
            ref legacyAltitude,
            new[] { "Altitude/Text", "Altitude", "Alt" },
            new[] { "feet", "meters", "ft", "H:" }
        );
        SmartFind(
            ref healthText,
            ref legacyHealthText,
            new[]
            {
                "HealthBar/Text",
                "Text",
                "Health Text",
                "HP Text",
                "PlayerHealth",
                "Health",
                "HP",
            },
            new[] { "hp", "health", "100" }
        );
        SmartFind(
            ref enemyHealthText,
            ref legacyEnemyHealthText,
            new[] { "enemy health bar/Text", "Text", "Enemy Health", "EnemyHealth", "EnemyHP" },
            new[] { "enemy", "100" }
        );

        SmartFind(
            ref targetName,
            ref legacyTargetName,
            new[] { "Name", "TargetName", "Target Name" },
            new[] { "target", "enemy" }
        );
        SmartFind(
            ref targetRange,
            ref legacyTargetRange,
            new[] { "Range", "TargetRange", "Target Range", "Distance" },
            new[] { "dist", "range", "m" }
        );

        if (throttleBar == null)
            throttleBar = FindComponent<Bar>("Throttle Bar", "Throttle");
        if (healthBar == null)
            healthBar = FindComponent<Bar>(
                "Health Bar",
                "Health",
                "HealthBar",
                "Player Health Bar"
            );

        if (targetBox == null)
            targetBox = FindTransform("Target Box", "TargetBox");
        if (missileLock == null)
            missileLock = FindTransform("Missile Lock", "MissileLock");
        if (reticle == null)
            reticle = FindTransform("Reticle", "Gun Reticle");
        if (reticleLine == null)
            reticleLine = FindComponent<RectTransform>("Reticle Line", "ReticleLine");
        if (targetArrow == null)
            targetArrow = FindComponent<RectTransform>("Target Arrow", "TargetArrow");
        if (missileArrow == null)
            missileArrow = FindComponent<RectTransform>("Missile Arrow", "MissileArrow");
    }

    Transform FindTransform(params string[] names)
    {
        foreach (var name in names)
        {
            var found = FindChildRecursive(transform, name);
            if (found != null)
                return found;
        }
        return null;
    }

    T FindComponent<T>(params string[] names)
        where T : Component
    {
        foreach (var name in names)
        {
            if (name.Contains("/"))
            {
                var parts = name.Split('/');
                Transform current = transform;

                foreach (var part in parts)
                {
                    var trimmedPart = part.Trim();
                    current = FindChildRecursive(current, trimmedPart);
                    if (current == null)
                        break;
                }

                if (current != null)
                {
                    var comp = current.GetComponent<T>();
                    if (comp != null)
                        return comp;
                }
            }
            else
            {
                var found = FindChildRecursive(transform, name);
                if (found != null)
                {
                    var comp = found.GetComponent<T>();
                    if (comp != null)
                        return comp;
                }
            }
        }
        return null;
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                return child;
            var result = FindChildRecursive(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    string GetFullPath(Transform t)
    {
        if (t == null)
            return "null";
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + " / " + path;
        }
        return path;
    }

    void ValidateReferences()
    {
        var missing = new List<string>();

        if (hudCenter == null)
            missing.Add(nameof(hudCenter));
        if (velocityMarker == null)
            missing.Add(nameof(velocityMarker));
        if (airspeed == null && legacyAirspeed == null)
            missing.Add(nameof(airspeed));
        if (aoaIndicator == null && legacyAOA == null)
            missing.Add(nameof(aoaIndicator));
        if (gforceIndicator == null && legacyGForce == null)
            missing.Add(nameof(gforceIndicator));
        if (altitude == null && legacyAltitude == null)
            missing.Add(nameof(altitude));
        if (throttleBar == null)
            missing.Add(nameof(throttleBar));
        if (healthBar == null)
            missing.Add(nameof(healthBar));
        if (healthText == null && legacyHealthText == null)
            missing.Add(nameof(healthText));
        if (targetBox == null)
            missing.Add(nameof(targetBox));
        if (targetName == null && legacyTargetName == null)
            missing.Add(nameof(targetName));
        if (targetRange == null && legacyTargetRange == null)
            missing.Add(nameof(targetRange));
        if (missileLock == null)
            missing.Add(nameof(missileLock));
        if (reticle == null)
            missing.Add(nameof(reticle));
        if (reticleLine == null)
            missing.Add(nameof(reticleLine));
        if (targetArrow == null)
            missing.Add(nameof(targetArrow));
        if (missileArrow == null)
            missing.Add(nameof(missileArrow));

        int totalChecked = 17;
        if (missing.Count > 0 && missing.Count < totalChecked)
        {
            Debug.LogWarning(
                $"[PlaneHUD] Some references not assigned on '{name}': {string.Join(", ", missing)}. HUD will function with available elements.",
                this
            );
        }
    }

    public void SetPlane(Plane plane)
    {
        this.plane = plane;

        if (plane == null)
        {
            planeTransform = null;
            selfTarget = null;
        }
        else
        {
            aiController = plane.GetComponent<AIController>();
            planeTransform = plane.GetComponent<Transform>();
            selfTarget = plane.GetComponent<Target>();
        }

        if (compass != null)
        {
            compass.SetPlane(plane);
        }

        if (pitchLadder != null)
        {
            pitchLadder.SetPlane(plane);
        }
    }

    public void SetCamera(Camera camera)
    {
        this.camera = camera;

        if (camera == null)
        {
            cameraTransform = null;
        }
        else
        {
            cameraTransform = camera.GetComponent<Transform>();
        }

        if (compass != null)
        {
            compass.SetCamera(camera);
        }

        if (pitchLadder != null)
        {
            pitchLadder.SetCamera(camera);
        }
    }

    public void ToggleHelpDialogs()
    {
        foreach (var dialog in helpDialogs)
        {
            if (dialog == null)
                continue;
            dialog.SetActive(!dialog.activeSelf);
        }
    }

    void UpdateVelocityMarker()
    {
        if (velocityMarker == null || velocityMarkerGO == null)
            return;

        var velocity = planeTransform.forward;

        if (plane.LocalVelocity.sqrMagnitude > 1)
        {
            velocity = plane.Rigidbody.linearVelocity;
        }

        var hudPos = TransformToHUDSpace(cameraTransform.position + velocity);

        if (hudPos.z > 0)
        {
            velocityMarkerGO.SetActive(true);
            velocityMarker.localPosition = new Vector3(hudPos.x, hudPos.y, 0);
        }
        else
        {
            velocityMarkerGO.SetActive(false);
        }
    }

    void UpdateAirspeed()
    {
        var val = plane.Velocity.magnitude * metersToKnots;
        var str = string.Format("{0:0}", val);
        if (airspeed != null)
            airspeed.text = str;
        else if (legacyAirspeed != null)
            legacyAirspeed.text = str;
    }

    void UpdateAOA()
    {
        var val = plane.AngleOfAttack * Mathf.Rad2Deg;
        var str = string.Format("{0:0.0} AOA", val);
        if (aoaIndicator != null)
            aoaIndicator.text = str;
        else if (legacyAOA != null)
            legacyAOA.text = str;
    }

    void UpdateGForce()
    {
        var gforce = plane.LocalGForce.y / 9.81f;
        var str = string.Format("{0:0.0} G", gforce);
        if (gforceIndicator != null)
            gforceIndicator.text = str;
        else if (legacyGForce != null)
            legacyGForce.text = str;
    }

    void UpdateAltitude()
    {
        var alt = plane.Rigidbody.position.y * metersToFeet;
        var str = string.Format("{0:0}", alt);
        if (altitude != null)
            altitude.text = str;
        else if (legacyAltitude != null)
            legacyAltitude.text = str;
    }

    Vector3 TransformToHUDSpace(Vector3 worldSpace)
    {
        var screenSpace = camera.WorldToScreenPoint(worldSpace);
        return screenSpace - new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2);
    }

    void UpdateHUDCenter()
    {
        if (hudCenter == null || hudCenterGO == null)
            return;

        var rotation = cameraTransform.localEulerAngles;
        var hudPos = TransformToHUDSpace(cameraTransform.position + planeTransform.forward);

        if (hudPos.z > 0)
        {
            hudCenterGO.SetActive(true);
            hudCenter.localPosition = new Vector3(hudPos.x, hudPos.y, 0);
            hudCenter.localEulerAngles = new Vector3(0, 0, -rotation.z);
        }
        else
        {
            hudCenterGO.SetActive(false);
        }
    }

    void UpdateHealth()
    {
        if (plane == null)
            return;

        if (healthBar != null)
        {
            float ratio = plane.MaxHealth > 0 ? plane.Health / plane.MaxHealth : 0f;
            healthBar.SetValue(ratio);
        }

        string healthStr = string.Format("{0:0}", plane.Health);
        if (healthText != null)
        {
            healthText.text = healthStr;
        }
        else if (legacyHealthText != null)
        {
            legacyHealthText.text = healthStr;
        }
    }

    void UpdateWeapons()
    {
        if (plane.Target == null)
        {
            if (targetBoxGO != null)
                targetBoxGO.SetActive(false);
            if (missileLockGO != null)
                missileLockGO.SetActive(false);
            if (reticleGO != null)
                reticleGO.SetActive(false);
            if (targetArrowGO != null)
                targetArrowGO.SetActive(false);
            return;
        }

        var targetDistance = Vector3.Distance(plane.Rigidbody.position, plane.Target.Position);
        var targetPos = TransformToHUDSpace(plane.Target.Position);
        var missileLockPos = plane.MissileLocked
            ? targetPos
            : TransformToHUDSpace(
                plane.Rigidbody.position + plane.MissileLockDirection * targetDistance
            );

        if (targetBoxGO != null && targetBox != null)
        {
            if (targetPos.z > 0)
            {
                targetBoxGO.SetActive(true);
                targetBox.localPosition = new Vector3(targetPos.x, targetPos.y, 0);
            }
            else
            {
                targetBoxGO.SetActive(false);
            }
        }

        if (missileLockGO != null && missileLock != null)
        {
            if (plane.MissileTracking && missileLockPos.z > 0)
            {
                missileLockGO.SetActive(true);
                missileLock.localPosition = new Vector3(missileLockPos.x, missileLockPos.y, 0);
            }
            else
            {
                missileLockGO.SetActive(false);
            }
        }

        if (plane.MissileLocked)
        {
            if (targetBoxImage != null)
                targetBoxImage.color = lockColor;
            if (targetName != null)
                targetName.color = lockColor;
            else if (legacyTargetName != null)
                legacyTargetName.color = lockColor;
            if (targetRange != null)
                targetRange.color = lockColor;
            else if (legacyTargetRange != null)
                legacyTargetRange.color = lockColor;
            if (missileLockImage != null)
                missileLockImage.color = lockColor;
        }
        else
        {
            if (targetBoxImage != null)
                targetBoxImage.color = normalColor;
            if (targetName != null)
                targetName.color = normalColor;
            else if (legacyTargetName != null)
                legacyTargetName.color = normalColor;
            if (targetRange != null)
                targetRange.color = normalColor;
            else if (legacyTargetRange != null)
                legacyTargetRange.color = normalColor;
            if (missileLockImage != null)
                missileLockImage.color = normalColor;
        }

        if (targetName != null)
            targetName.text = plane.Target.Name;
        else if (legacyTargetName != null)
            legacyTargetName.text = plane.Target.Name;

        var distStr = string.Format("{0:0 m}", targetDistance);
        if (targetRange != null)
            targetRange.text = distStr;
        else if (legacyTargetRange != null)
            legacyTargetRange.text = distStr;

        var targetDir = (plane.Target.Position - plane.Rigidbody.position).normalized;
        var targetAngle = Vector3.Angle(cameraTransform.forward, targetDir);

        if (targetArrowGO != null && targetArrow != null)
        {
            if (targetAngle > targetArrowThreshold)
            {
                targetArrowGO.SetActive(true);

                float flip = targetPos.z > 0 ? 0 : 180;
                targetArrow.localEulerAngles = new Vector3(
                    0,
                    0,
                    flip + Vector2.SignedAngle(Vector2.up, new Vector2(targetPos.x, targetPos.y))
                );
            }
            else
            {
                targetArrowGO.SetActive(false);
            }
        }

        if (reticleGO != null && reticle != null)
        {
            var leadPos = Utilities.FirstOrderIntercept(
                plane.Rigidbody.position,
                plane.Rigidbody.linearVelocity,
                bulletSpeed,
                plane.Target.Position,
                plane.Target.Velocity
            );
            var reticlePos = TransformToHUDSpace(leadPos);

            if (reticlePos.z > 0 && targetDistance <= cannonRange)
            {
                reticleGO.SetActive(true);
                reticle.localPosition = new Vector3(reticlePos.x, reticlePos.y, 0);

                if (reticleLine != null)
                {
                    var reticlePos2 = new Vector2(reticlePos.x, reticlePos.y);
                    if (Mathf.Sign(targetPos.z) != Mathf.Sign(reticlePos.z))
                        reticlePos2 = -reticlePos2;
                    var targetPos2 = new Vector2(targetPos.x, targetPos.y);
                    var reticleError = reticlePos2 - targetPos2;

                    var lineAngle = Vector2.SignedAngle(Vector3.up, reticleError);
                    reticleLine.localEulerAngles = new Vector3(0, 0, lineAngle + 180f);
                    reticleLine.sizeDelta = new Vector2(
                        reticleLine.sizeDelta.x,
                        reticleError.magnitude
                    );
                }
            }
            else
            {
                reticleGO.SetActive(false);
            }
        }
    }

    void UpdateEnemyHealth()
    {
        if (enemyHealthBar == null)
            return;

        var target = plane.Target;
        var enemyPlane = target != null ? target.Plane : null;

        bool shouldShow = enemyPlane != null && !enemyPlane.Dead && enemyPlane.MaxHealth > 0;
        if (enemyHealthBar.gameObject.activeSelf != shouldShow)
            enemyHealthBar.gameObject.SetActive(shouldShow);
        if (!shouldShow)
        {
            lastEnemyTarget = null;
            return;
        }

        float normalized = Mathf.Clamp01(enemyPlane.Health / enemyPlane.MaxHealth);

        if (target != lastEnemyTarget)
        {
            enemyHealthDisplayValue = normalized;
            lastEnemyTarget = target;
        }

        if (enemyHealthFillSpeed > 0f)
        {
            enemyHealthDisplayValue = Mathf.MoveTowards(
                enemyHealthDisplayValue,
                normalized,
                Time.deltaTime * enemyHealthFillSpeed
            );
            enemyHealthBar.SetValue(enemyHealthDisplayValue);
        }
        else
        {
            enemyHealthBar.SetValue(normalized);
        }

        if (enemyHealthText != null)
        {
            enemyHealthText.text = string.Format("{0:0}", enemyPlane.Health);
        }
        else if (legacyEnemyHealthText != null)
        {
            legacyEnemyHealthText.text = string.Format("{0:0}", enemyPlane.Health);
        }
    }

    void UpdateWarnings()
    {
        if (selfTarget == null)
            return;

        var incomingMissile = selfTarget.GetIncomingMissile();

        if (incomingMissile != null)
        {
            var missilePos = TransformToHUDSpace(incomingMissile.Rigidbody.position);
            var missileDir = (
                incomingMissile.Rigidbody.position - plane.Rigidbody.position
            ).normalized;
            var missileAngle = Vector3.Angle(cameraTransform.forward, missileDir);

            if (missileArrowGO != null && missileArrow != null)
            {
                if (missileAngle > missileArrowThreshold)
                {
                    missileArrowGO.SetActive(true);

                    float flip = missilePos.z > 0 ? 0 : 180;
                    missileArrow.localEulerAngles = new Vector3(
                        0,
                        0,
                        flip
                            + Vector2.SignedAngle(
                                Vector2.up,
                                new Vector2(missilePos.x, missilePos.y)
                            )
                    );
                }
                else
                {
                    missileArrowGO.SetActive(false);
                }
            }

            if (missileWarningGraphics != null)
            {
                foreach (var graphic in missileWarningGraphics)
                {
                    if (graphic == null)
                        continue;
                    graphic.color = lockColor;
                }
            }

            if (pitchLadder != null)
                pitchLadder.UpdateColor(lockColor);
            if (compass != null)
                compass.UpdateColor(lockColor);
        }
        else
        {
            if (missileArrowGO != null)
                missileArrowGO.SetActive(false);

            if (missileWarningGraphics != null)
            {
                foreach (var graphic in missileWarningGraphics)
                {
                    if (graphic == null)
                        continue;
                    graphic.color = normalColor;
                }
            }

            if (pitchLadder != null)
                pitchLadder.UpdateColor(normalColor);
            if (compass != null)
                compass.UpdateColor(normalColor);
        }
    }

    void LateUpdate()
    {
        if (plane == null)
            return;
        if (camera == null)
            return;

        float degreesToPixels = camera.pixelHeight / camera.fieldOfView;

        if (throttleBar != null)
            throttleBar.SetValue(plane.Throttle);

        if (!plane.Dead)
        {
            UpdateVelocityMarker();
            UpdateHUDCenter();
        }
        else
        {
            if (hudCenterGO != null)
                hudCenterGO.SetActive(false);
            if (velocityMarkerGO != null)
                velocityMarkerGO.SetActive(false);
        }

        if (aiController != null)
        {
            if (aiMessage != null)
                aiMessage.SetActive(aiController.enabled);
        }

        UpdateAirspeed();
        UpdateAltitude();
        UpdateHealth();
        UpdateWeapons();
        UpdateEnemyHealth();
        UpdateWarnings();

        if (Time.time > lastUpdateTime + (1f / updateRate))
        {
            UpdateAOA();
            UpdateGForce();
            lastUpdateTime = Time.time;
        }
    }
}
