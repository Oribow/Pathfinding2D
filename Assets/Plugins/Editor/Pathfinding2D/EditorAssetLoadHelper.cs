using System.IO;
using UnityEditor;



public class EditorAssetLoadHelper
{
    public const string EditorWindowHeader_Builder = "Header_Builder.png";
    public const string BuildStepList_CircleFilled = "EnumCircle_Filled.png";
    public const string BuildStepList_CircleGrayed = "EnumCircle_Grayed.png";
    public const string BuildStepList_CircleOutdated = "EnumCircle_Outdated.png";

    /// <summary>
    /// Loads an Asset relative to /Assets/Editor Default Resources/*PluginName*/, that is required to be there.
    /// </summary>
    /// <typeparam name="T">The Type of the Asset, that will be loaded</typeparam>
    /// <param name="name">The name of the to-load Asset</param>
    /// <returns></returns>
    public static T LoadAssetRequired<T>(string name) where T : UnityEngine.Object
    {
        return (T)EditorGUIUtility.LoadRequired(DefaultValues.PluginName + Path.DirectorySeparatorChar + name);
    }

    /// <summary>
    /// Loads an Asset relative to /Assets/Editor Default Resources/*PluginName*/
    /// </summary>
    /// <typeparam name="T">The Type of the Asset, that will be loaded</typeparam>
    /// <param name="name">The name of the to-load Asset</param>
    /// <returns></returns>
    public static T LoadAsset<T>(string name) where T : UnityEngine.Object
    {
        return (T)EditorGUIUtility.Load(DefaultValues.PluginName + Path.DirectorySeparatorChar + name);
    }
}
