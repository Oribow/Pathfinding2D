using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct Boundsd {

    [SerializeField]
    private Vector3d m_Center;
    [SerializeField]
    private Vector3d m_Extents;

    /// <summary>
    ///   <para>The center of the bounding box.</para>
    /// </summary>
    public Vector3d center
    {
        get
        {
            return this.m_Center;
        }
        set
        {
            this.m_Center = value;
        }
    }

    /// <summary>
    ///   <para>The total size of the box. This is always twice as large as the extents.</para>
    /// </summary>
    public Vector3d size
    {
        get
        {
            return this.m_Extents * 2;
        }
        set
        {
            this.m_Extents = value * 0.5;
        }
    }

    /// <summary>
    ///   <para>The extents of the box. This is always half of the size.</para>
    /// </summary>
    public Vector3d extents
    {
        get
        {
            return this.m_Extents;
        }
        set
        {
            this.m_Extents = value;
        }
    }

    /// <summary>
    ///   <para>The minimal point of the box. This is always equal to center-extents.</para>
    /// </summary>
    public Vector3d min
    {
        get
        {
            return this.center - this.extents;
        }
        set
        {
            this.SetMinMax(value, this.max);
        }
    }

    /// <summary>
    ///   <para>The maximal point of the box. This is always equal to center+extents.</para>
    /// </summary>
    public Vector3d max
    {
        get
        {
            return this.center + this.extents;
        }
        set
        {
            this.SetMinMax(this.min, value);
        }
    }

    /// <summary>
    ///   <para>Creates new Bounds with a given center and total size. Bound extents will be half the given size.</para>
    /// </summary>
    /// <param name="center"></param>
    /// <param name="size"></param>
    public Boundsd(Vector3d center, Vector3d size)
    {
        this.m_Center = center;
        this.m_Extents = size * 0.5f;
    }

    public static bool operator ==(Boundsd lhs, Boundsd rhs)
    {
        if (lhs.center == rhs.center)
            return lhs.extents == rhs.extents;
        return false;
    }

    public static bool operator !=(Boundsd lhs, Boundsd rhs)
    {
        return !(lhs == rhs);
    }

    public override int GetHashCode()
    {
        return this.center.GetHashCode() ^ this.extents.GetHashCode() << 2;
    }

    public override bool Equals(object other)
    {
        if (!(other is Boundsd))
            return false;
        Boundsd bounds = (Boundsd)other;
        if (this.center.Equals((object)bounds.center))
            return this.extents.Equals((object)bounds.extents);
        return false;
    }

    /// <summary>
    ///   <para>Sets the bounds to the min and max value of the box.</para>
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public void SetMinMax(Vector3d min, Vector3d max)
    {
        this.extents = (max - min) * 0.5;
        this.center = min + this.extents;
    }

    /// <summary>
    ///   <para>Grows the Bounds to include the point.</para>
    /// </summary>
    /// <param name="point"></param>
    public void Encapsulate(Vector3d point)
    {
        this.SetMinMax(Vector3d.Min(this.min, point), Vector3d.Max(this.max, point));
    }

    /// <summary>
    ///   <para>Grow the bounds to encapsulate the bounds.</para>
    /// </summary>
    /// <param name="bounds"></param>
    public void Encapsulate(Boundsd bounds)
    {
        this.Encapsulate(bounds.center - bounds.extents);
        this.Encapsulate(bounds.center + bounds.extents);
    }

    /// <summary>
    ///   <para>Expand the bounds by increasing its size by amount along each side.</para>
    /// </summary>
    /// <param name="amount"></param>
    public void Expand(double amount)
    {
        amount *= 0.5;
        this.extents += new Vector3d(amount, amount, amount);
    }

    /// <summary>
    ///   <para>Expand the bounds by increasing its size by amount along each side.</para>
    /// </summary>
    /// <param name="amount"></param>
    public void Expand(Vector3d amount)
    {
        this.extents += amount * 0.5;
    }

    /// <summary>
    ///   <para>Does another bounding box intersect with this bounding box?</para>
    /// </summary>
    /// <param name="bounds"></param>
    public bool Intersects(Boundsd bounds)
    {
        if ((double)this.min.x <= (double)bounds.max.x && (double)this.max.x >= (double)bounds.min.x && ((double)this.min.y <= (double)bounds.max.y && (double)this.max.y >= (double)bounds.min.y) && (double)this.min.z <= (double)bounds.max.z)
            return (double)this.max.z >= (double)bounds.min.z;
        return false;
    }

    /// <summary>
    ///   <para>Is point contained in the bounding box?</para>
    /// </summary>
    /// <param name="point"></param>
    public bool Contains(Vector3d point)
    {
        Vector3d min = this.min;
        Vector3d max = this.max;

        return point.x >= min.x && point.y >= min.y && point.z >= min.z
            && point.x <= max.x && point.y <= max.y && point.z <= max.z;
    }

    /*
    /// <summary>
    ///   <para>The smallest squared distance between the point and this bounding box.</para>
    /// </summary>
    /// <param name="point"></param>
    public float SqrDistance(Vector3 point)
    {
        return Bounds.Internal_SqrDistance(this, point);
    }

    /// <summary>
    ///   <para>Does ray intersect this bounding box?</para>
    /// </summary>
    /// <param name="ray"></param>
    public bool IntersectRay(Ray ray)
    {
        float distance;
        return Bounds.Internal_IntersectRay(ref ray, ref this, out distance);
    }

    public bool IntersectRay(Ray ray, out float distance)
    {
        return Bounds.Internal_IntersectRay(ref ray, ref this, out distance);
    }

    /// <summary>
    ///   <para>The closest point on the bounding box.</para>
    /// </summary>
    /// <param name="point">Arbitrary point.</param>
    /// <returns>
    ///   <para>The point on the bounding box or inside the bounding box.</para>
    /// </returns>
    public Vector3 ClosestPoint(Vector3 point)
    {
        return Bounds.Internal_GetClosestPoint(ref this, ref point);
    }*/

    /// <summary>
    ///   <para>Returns a nicely formatted string for the bounds.</para>
    /// </summary>
    /// <param name="format"></param>
    public override string ToString()
    {
        return String.Format("Center: {0}, Extents: {1}", (object)this.m_Center, (object)this.m_Extents);
    }

    public static explicit operator Bounds(Boundsd src)
    {
        return new Bounds((Vector3)src.center, (Vector3)src.size);
    }
}
