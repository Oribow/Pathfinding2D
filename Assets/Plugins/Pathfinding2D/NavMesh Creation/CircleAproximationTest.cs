using UnityEngine;
using System.Collections;
using Utility;
using Utility.ExtensionMethods;

public class CircleAproximationTest : MonoBehaviour
{

    public Vector2 pointA;
    public Vector2 pointB;
    public Vector2 M;
    public float radius;
    public float L;

    public int S;
    public float correctedL;
    public float segRad;
    public Vector2 cPoint;
    public Vector2 prevPoint;

    void OnDrawGizmos()
    {
        //segRad = Mathf.Acos(Mathf.Clamp(Vector2.Dot(pointA - M, pointB - M), -1f, 1f));
        Vector2 nA = pointA.normalized * radius ;
        Vector2 nB = pointB.normalized * radius ;

        /* segRad = Vector2.Angle(nA, nB) * Mathf.Deg2Rad;
         if (Vector3.Cross(pointA, pointB).z < 0)
             segRad = Mathf.PI * 2 - segRad;

         S = Mathf.Max(Mathf.CeilToInt(segRad / (Mathf.Acos(radius / (radius + L)) * 2)), 1);
         correctedL = (radius / Mathf.Cos(segRad / (S * 2))) - radius;

         float angleStep = segRad / S;
         prevPoint = nA;
         cPoint = prevPoint.RotateRad(angleStep / 2).normalized * (radius + correctedL);
         Gizmos.DrawLine(prevPoint, cPoint);
         for (int i = 0; i < S - 1; i++)
         {
             //cPoint = new Vector2(Mathf.Sin(beta), Mathf.Cos(beta)) * (radius + correctedL) + M;
             prevPoint = cPoint;
             cPoint = cPoint.RotateRad(angleStep);
             Gizmos.DrawLine(prevPoint, cPoint);
         }
         Gizmos.DrawLine(cPoint, nB);
         */
        Vector2[] verts = AproximateCircle(radius, pointA, pointB, M);

        foreach (Vector2 v in verts)
        {
             DebugExtension.DrawArrow(M, v, Color.black);
        }

        for (int i = 0; i < 300; i++)
        {
            Vector2 point = Random.insideUnitCircle * radius + M;
            if (IsPointInsideCircleSector(nB, nA, radius * radius, Vector2.Angle(nA, nB), point - M))
                DebugExtension.DebugPoint(point, Color.green);
            else
                DebugExtension.DebugPoint(point, Color.red);
        }

        DebugExtension.DrawArrow(M, nA - M);
        DebugExtension.DrawArrow(M, nB - M);

        
    }
    const float maxVertDeviation = 0.01f;
    Vector2[] AproximateCircle(float radius, Vector2 a, Vector2 b, Vector2 m)
    {
        Vector2 nA = pointA.normalized * radius;
        Vector2 nB = pointB.normalized * radius;

        float segRad = Vector2.Angle(nA, nB) * Mathf.Deg2Rad;
        if (Vector3.Cross(pointA, pointB).z < 0)
            segRad = Mathf.PI * 2 - segRad;

        int S = Mathf.Max(Mathf.CeilToInt(segRad / (Mathf.Acos(radius / (radius + maxVertDeviation)) * 2)), 1);
        float correctedL = (radius / Mathf.Cos(segRad / (S * 2))) - radius;

        Vector2[] result = new Vector2[S + 2];

        float angleStep = segRad / S;

        cPoint = nA.RotateRad(angleStep / 2).normalized * (radius + correctedL);
        result[0] = nA;
        result[1] = cPoint;
        result[S+1] = nB;
        for (int i = 2; i < S+1; i++)
        {
            cPoint = cPoint.RotateRad(angleStep);
            result[i] = cPoint;
        }

        return result;
    }

    private static bool IsPointInsideCircleSector(Vector2 a, Vector2 b, float radSquared, float angle, Vector2 point)
    {
        //is within radius:
        //if(angle > Mathf.PI)
        //return ((-a.x * point.y + a.y * point.x > 0 || -b.x * point.y + b.y * point.x <= 0) && point.x * point.x + point.y * point.y <= radSquared);
        return (-a.x * point.y + a.y * point.x > 0 && -b.x * point.y + b.y * point.x <= 0 && point.x * point.x + point.y * point.y <= radSquared);
    }
}
