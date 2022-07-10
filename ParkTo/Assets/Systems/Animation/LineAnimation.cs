using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class LineAnimation
{
    public static float Lerp(float a, float b, float t, float sStart = 0, float sEnd = 0)
        => Mathf.Lerp(a, b, LerpBase(t, sStart, sEnd));

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t, float sStart = 0, float sEnd = 0)
        => Vector3.Lerp(a, b, LerpBase(t, sStart, sEnd));

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t, float sStart = 0, float sEnd = 0)
        => Vector2.Lerp(a, b, LerpBase(t, sStart, sEnd));

    public static float LerpBound(float a, float b, float t, float sStart = 0, float bEnd = 0)
        => a + (b - a) * LerpBoundBase(t, sStart, bEnd);

    public static Vector3 LerpBound(Vector3 a, Vector3 b, float t, float sStart = 0, float bEnd = 0)
        => a + (b - a) * LerpBoundBase(t, sStart, bEnd);

    public static Vector2 LerpBound(Vector2 a, Vector2 b, float t, float sStart = 0, float bEnd = 0)
        => a + (b - a) * LerpBoundBase(t, sStart, bEnd);

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

    private static float LerpBoundBase(float playTime, float sStart = 0, float bEnd = 0)
    {
        double cl = Math.Pow(2 - sStart - bEnd * 2, -1);
        double temp = playTime - 1 + bEnd;
        double Result;

        if (playTime < sStart)
            Result = cl * (playTime - sStart / Math.PI * Math.Sin(Math.PI * playTime / sStart));
        else if (playTime <= 1 - bEnd) Result = cl * (playTime * 2 - sStart);
        else if (playTime < 1)
        {
            Result =
                bEnd * 2 / (Math.PI * 7) * cl *
                Math.Pow(2 / 3.71828182845905f, 7 * Math.PI * temp / (bEnd * 2)) *
                Math.Sin(temp * Math.PI * 7 / bEnd) + 1;
        }
        else Result = 1;

        return (float)Result;
    }
}