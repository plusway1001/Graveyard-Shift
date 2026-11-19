using UnityEngine;

/// <summary>Measures a room by raycasting from a point inside it to the nearest solid walls.</summary>
public static class L2RoomBounds
{
    public static Rect Measure(Vector2 inside, float maxDistance = 40f)
    {
        float left  = Cast(inside, Vector2.left,  maxDistance);
        float right = Cast(inside, Vector2.right, maxDistance);
        float up    = Cast(inside, Vector2.up,    maxDistance);
        float down  = Cast(inside, Vector2.down,  maxDistance);
        return Rect.MinMaxRect(inside.x - left, inside.y - down, inside.x + right, inside.y + up);
    }

    /// <summary>Distance to the first solid, static collider in a direction.</summary>
    public static float Cast(Vector2 origin, Vector2 dir, float max)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, max);
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider == null || h.collider.isTrigger) continue;
            if (h.distance < 0.05f) continue;
            Rigidbody2D rb = h.collider.attachedRigidbody;
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic) continue; // player, enemies
            return h.distance;
        }
        return max;
    }

    /// <summary>True when no solid collider overlaps the circle.</summary>
    public static bool IsFree(Vector2 p, float radius)
    {
        foreach (Collider2D c in Physics2D.OverlapCircleAll(p, radius))
        {
            if (c == null || c.isTrigger) continue;
            Rigidbody2D rb = c.attachedRigidbody;
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic) continue;
            return false;
        }
        return true;
    }

    /// <summary>True when a straight line between two points crosses no solid wall.</summary>
    public static bool ClearLine(Vector2 a, Vector2 b)
    {
        Vector2 d = b - a;
        float len = d.magnitude;
        if (len < 0.01f) return true;
        return Cast(a, d / len, len) >= len;
    }
}
