using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot("resources")]
public class FxRLocalizedStringResources
{
    [XmlElement("string")]
    public FxRLocalizedString[] strings;
}