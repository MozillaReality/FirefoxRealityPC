// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// In order to show logs that are added in internal libaries,
// we need to redirect these messages from console to Unity
// debug panel.
internal static class FxRUnitySystemConsoleRedirector
{
  private class UnityTextWriter : TextWriter
  {
    private StringBuilder buffer = new StringBuilder();

    public override void Flush()
    {
      Debug.Log(buffer.ToString());
      buffer.Length = 0;
    }

    public override void Write(string value)
    {
      buffer.Append(value);
      if (value != null)
      {
        var len = value.Length;
        if (len > 0)
        {
          var lastChar = value[len - 1];
          if (lastChar == '\n')
          {
            Flush();
          }
        }
      }
    }

    public override void Write(char value)
    {
      buffer.Append(value);
      if (value == '\n')
      {
        Flush();
      }
    }

    public override void Write(char[] value, int index, int count)
    {
      Write(new string(value, index, count));
    }

    public override Encoding Encoding
    {
      get { return Encoding.Default; }
    }
  }

  public static void Redirect()
  {
    Console.SetOut(new UnityTextWriter());
  }
}
