using System;
using System.Xml.Serialization;

[Serializable]
public class FxRLocalizedString
{
    [XmlAttribute("name")] public string key;

    [XmlText(Type = typeof(string))] public string value;
}