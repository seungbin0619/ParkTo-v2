using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class LineAnimation
{
    public static float Lerp(float a, float b, float t, float sStart = 0, float sEnd = 0)
    {
        return Mathf.Lerp(a, b, LerpBase(t, sStart, sEnd));
    }

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t, float sStart = 0, float sEnd = 0)
    {
        return Vector3.Lerp(a, b, LerpBase(t, sStart, sEnd));
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t, float sStart = 0, float sEnd = 0)
    {
        return Vector2.Lerp(a, b, LerpBase(t, sStart, sEnd));
    }

    private static float LerpBase(float playTime, float sStart = 0, float sEnd = 0)
    {
        float cl = 2 - sStart - sEnd;
        float Result;

        if (playTime < sStart) 
            Result = (playTime / cl) - 
                (sStart * Mathf.Sin(Mathf.PI * playTime / sStart) / (cl * Mathf.PI));
        else if (playTime <= 1 - sEnd) Result = (playTime * 2 - sStart) / cl;
        else if (playTime < 1)
        {
            Result =
                (playTime - 1 + sEnd) / cl +
                sEnd * Mathf.Sin((playTime - 1 + sEnd) * Mathf.PI / sEnd) / (cl * Mathf.PI) +
                (cl - sEnd) / cl;
        }
        else Result = 1;

        return Result;
    }
}