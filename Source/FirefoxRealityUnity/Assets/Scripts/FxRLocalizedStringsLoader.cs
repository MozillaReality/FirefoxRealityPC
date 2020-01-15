// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2020, Mozilla.

/*
 * This class is responsible for loading localized strings for a user locale from "strings.xml" files that are
 * provided per language.
 */

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Diagnostics;

public class FxRLocalizedStringsLoader : MonoBehaviour
{
    public delegate void LocalizedStringsLoaded();

    public static LocalizedStringsLoaded OnLocalizedStringsLoaded;

    static private Dictionary<string, string> LoadedStrings = new Dictionary<string, string>();

    public static string GetApplicationString(string key)
    {
        LoadedStrings.TryGetValue(key, out var applicationString);
        return applicationString;
    }

    void Start()
    {
        // TODO: Detect Firefox Desktop language, and load the strings for that locale. For now, we'll load current OS locale...
        LoadApplicationStringsForLocale(CultureInfo.CurrentCulture?.Name);
    }
   
    // Load and parse the strings.xml for the given locale.
    // If a strings.xml file does not exist for the given locale, then try the parent locale.
    // If a strings.xml file does not exist for the parent locale, then load the fallback locale of "en"
    // If a duplicate key is found in the loaded strings.xml, log an error and crash
    public static void LoadApplicationStringsForLocale(string locale)
    {
        LoadedStrings.Clear();
        string stringsFile = Path.Combine(Application.streamingAssetsPath, "l10n", locale, "strings.xml");
        if (!File.Exists(stringsFile))
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture(locale);
            if (cultureInfo.Parent != null)
            {
                stringsFile = Path.Combine(Application.streamingAssetsPath, "l10n", cultureInfo.Parent.Name,
                    "strings.xml");
                if (!File.Exists(stringsFile))
                {
                    stringsFile = null;
                }
            }

            if (stringsFile == null)
            {
                Debug.LogWarningFormat(
                    "Could not find strings.xml for locale {0}. Loading English fallback language strings", locale);
                stringsFile = Path.Combine(Application.streamingAssetsPath, "l10n", "en", "strings.xml");
            }
        }

        string xml = File.ReadAllText(stringsFile);

        XmlSerializer serializer = new XmlSerializer(typeof(FxRLocalizedStringResources));
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
        {
            var resources = (FxRLocalizedStringResources) serializer.Deserialize(stream);
            foreach (var applicationString in resources.strings)
            {
                if (LoadedStrings.ContainsKey(applicationString.key))
                {
                    Debug.LogErrorFormat("Found duplicate key '{0}' in strings.xml for locale {1}.",
                        applicationString.key, locale);
                    Utils.ForceCrash(ForcedCrashCategory.Abort);
                }

                LoadedStrings.Add(applicationString.key, applicationString.value);
            }
        }

        OnLocalizedStringsLoaded?.Invoke();
    }
}