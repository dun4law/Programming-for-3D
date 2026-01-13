using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField]
    new string name;

    public string Name
    {
        get { return name; }
    }

    public void SetName(string newName)
    {
        name = newName;
    }

    public Vector3 Position
    {
        get
        {
            if (rigidbody == null)
                return transform.position;
            return rigidbody.position;
        }
    }

    public Vector3 Velocity
    {
        get
        {
            if (rigidbody == null)
                return Vector3.zero;
            return rigidbody.linearVelocity;
        }
    }

    public Plane Plane { get; private set; }

    new Rigidbody rigidbody;

    List<Missile> incomingMissiles = new List<Missile>();
    const float sortInterval = 0.5f;
    float sortTimer;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        Plane = GetComponent<Plane>();

        if (incomingMissiles == null)
        {
            incomingMissiles = new List<Missile>();
        }
    }

    void FixedUpdate()
    {
        sortTimer = Mathf.Max(0, sortTimer - Time.fixedDeltaTime);

        if (sortTimer == 0)
        {
            SortIncomingMissiles();
            sortTimer = sortInterval;
        }
    }

    void SortIncomingMissiles()
    {
        if (incomingMissiles == null || incomingMissiles.Count == 0)
            return;
        if (rigidbody == null)
            return;

        var position = Position;

        incomingMissiles.RemoveAll(m => m == null);

        if (incomingMissiles.Count > 0)
        {
            incomingMissiles.Sort(
                (Missile a, Missile b) =>
                {
                    if (a == null || a.Rigidbody == null)
                        return 1;
                    if (b == null || b.Rigidbody == null)
                        return -1;
                    var distA = Vector3.Distance(a.Rigidbody.position, position);
                    var distB = Vector3.Distance(b.Rigidbody.position, position);
                    return distA.CompareTo(distB);
                }
            );
        }
    }

    public Missile GetIncomingMissile()
    {
        if (incomingMissiles == null)
        {
            incomingMissiles = new List<Missile>();
            return null;
        }

        incomingMissiles.RemoveAll(m => m == null);

        if (incomingMissiles.Count > 0)
        {
            return incomingMissiles[0];
        }

        return null;
    }

    public void NotifyMissileLaunched(Missile missile, bool value)
    {
        if (value)
        {
            incomingMissiles.Add(missile);
            SortIncomingMissiles();
        }
        else
        {
            incomingMissiles.Remove(missile);
        }

        var threatWarning = FindAnyObjectByType<ThreatWarningSystem>();
        if (threatWarning != null)
        {
            threatWarning.OnMissileLaunched(missile, value);
        }
    }
}
