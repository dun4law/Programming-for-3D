using UnityEngine;

public static class AircraftTuningApplier
{
    public static void ApplySelectedAircraft(Plane plane)
    {
        if (plane == null)
            return;

        string aircraftId = PlayerPrefs.GetString(
            AircraftSelectionApplier.SelectedAircraftKey,
            "F15"
        );
        Apply(plane, aircraftId);
    }

    public static void Apply(Plane plane, string aircraftId)
    {
        if (plane == null)
            return;

        var db = AircraftTuningDatabase.LoadOrCreateDefault();
        var tuning = db != null ? db.Get(aircraftId) : null;
        if (tuning == null)
            return;

        plane.ApplyTuning(tuning);

        var ai = plane.GetComponent<AIController>();
        if (ai != null)
        {
            ai.ApplyTuning(tuning);

            if (plane.team == Team.Player)
            {
                ai.SetCanUseCountermeasures(true);
            }
        }

        var stall = plane.GetComponent<StallWarning>();
        if (stall != null)
        {
            stall.ApplyTuning(tuning);
        }

        var gforce = plane.GetComponent<GForceEffects>();
        if (gforce != null)
        {
            gforce.ApplyTuning(tuning);
        }

        if (plane.team == Team.Player)
        {
            string playerCallsign = PlayerPrefs.GetString("PlayerCallsign", "PHOENIX");
            plane.SetCallsign(playerCallsign);

            var radar = Object.FindFirstObjectByType<RadarHUD>();
            if (radar != null)
            {
                radar.SetFixedRadarRange(tuning.radarRangeMeters);
            }
        }
    }
}
