using UnityEngine;

public static class GameObjectExtensions
{
    public static int GetEntityId(this GameObject obj)
    {
        if (obj == null)
            return 0;

        return Mathf.Abs(obj.GetInstanceID());
    }

    public static int GetEntityId(this Component component)
    {
        if (component == null)
            return 0;

        return component.gameObject.GetEntityId();
    }

    public static int GetEntityId(this Missile missile)
    {
        if (missile == null)
            return 0;

        return missile.LaunchId;
    }
}
