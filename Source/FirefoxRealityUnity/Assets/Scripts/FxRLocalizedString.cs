// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2020, Mozilla.

/*
 * Class that represents a single localized string key/value pair. Used by FxRLocalizedStringsLoader to deserialize a strings.xml file.
 */
using System;
using System.Xml.Serialization;

[Serializable]
public class FxRLocalizedString
{
    [XmlAttribute("name")] public string key;

    [XmlText(Type = typeof(string))] public string value;
}