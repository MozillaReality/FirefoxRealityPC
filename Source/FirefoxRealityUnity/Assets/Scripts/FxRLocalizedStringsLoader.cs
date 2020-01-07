using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

public class FxRLocalizedStringsLoader : MonoBehaviour
{
    public delegate void LocalizedStringsLoaded();

    public static LocalizedStringsLoaded onLocalizedStringsLoaded;
    
    static private Dictionary<string, string> LoadedStrings;

    public static string GetApplicationString(string key)
    {
        LoadedStrings.TryGetValue(key, out var applicationString);
        return applicationString;
    }

    void Start()
    {
        // TODO: Detect Firefox Desktop language, and load the strings for that locale. For now, we'll load en-us...
        LoadApplicationStringsForLocale("en-us");
    }

    public static void LoadApplicationStringsForLocale(string locale)
    {
        string xml = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "i10n", locale, "strings.xml"));

        XmlSerializer serializer = new XmlSerializer(typeof(FxRLocalizedStringResources));
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
        {
            var resources = (FxRLocalizedStringResources) serializer.Deserialize(stream);
            foreach (var applicationString in resources.strings)
            {
                LoadedStrings.Add(applicationString.key, applicationString.value);
            }
        }
        onLocalizedStringsLoaded?.Invoke();
    }
}