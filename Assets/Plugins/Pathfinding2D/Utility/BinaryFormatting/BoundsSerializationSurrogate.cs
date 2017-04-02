using System.Runtime.Serialization;
using UnityEngine;

public sealed class BoundsSerializationSurrogate : ISerializationSurrogate
{

    // Method called to serialize a Vector3 object
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {

        Bounds bounds = (Bounds)obj;
        info.AddValue("centerx", bounds.center.x);
        info.AddValue("centery", bounds.center.y);
        info.AddValue("centerz", bounds.center.z);
        info.AddValue("extentsx", bounds.extents.x);
        info.AddValue("extentsy", bounds.extents.y);
        info.AddValue("extentsz", bounds.extents.z);
    }

    // Method called to deserialize a Vector3 object
    public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {

        Bounds bounds = (Bounds)obj;
        bounds.center = new Vector3((float)info.GetValue("centerx", typeof(float)),
            (float)info.GetValue("centery", typeof(float)),
            (float)info.GetValue("centerz", typeof(float)));
        bounds.extents = new Vector3((float)info.GetValue("extentsx", typeof(float)),
            (float)info.GetValue("extentsy", typeof(float)),
            (float)info.GetValue("extentsz", typeof(float)));
        obj = bounds;
        return obj;   // Formatters ignore this return value //Seems to have been fixed!
    }
}
