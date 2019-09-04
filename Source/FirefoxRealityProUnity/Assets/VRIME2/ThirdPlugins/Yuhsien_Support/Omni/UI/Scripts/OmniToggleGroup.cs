// ========================================================================== //
//
//  class OmniToggleGroup
//  -----
//  Purpose: Toggle group for OmniToggle
//
//  Based on: https://bitbucket.org/Unity-Technologies/ui/src/2017.3/UnityEngine.UI/UI/Core/ToggleGroup.cs
//
//  Created: 2018-11-21
//  Updated: 2018-11-21
//
//  Copyright 2018 HTC America Innovation
// 
// ========================================================================== //
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Htc.Omni
{
    [DisallowMultipleComponent]
    public class OmniToggleGroup : BaseBehaviour
    {
        [SerializeField]
        private bool _allowSwitchOff = false;
        public bool allowSwitchOff { get { return _allowSwitchOff; } set { _allowSwitchOff = value; } }

        private List<OmniToggle> toggles = new List<OmniToggle>();

        private void ValidateToggleIsInGroup(OmniToggle toggle)
        {
            if (toggle == null || !toggles.Contains(toggle))
                throw new ArgumentException(string.Format("Toggle {0} is not part of ToggleGroup {1}", new object[] { toggle, this }));
        }

        public void NotifyToggleOn(OmniToggle toggle)
        {
            ValidateToggleIsInGroup(toggle);

            // disable all toggles in the group
            for (var i = 0; i < toggles.Count; i++)
            {
                if (toggles[i] == toggle)
                    continue;

                toggles[i].isOn = false;
            }
        }

        public void UnregisterToggle(OmniToggle toggle)
        {
            if (toggles.Contains(toggle))
                toggles.Remove(toggle);
        }

        public void RegisterToggle(OmniToggle toggle)
        {
            if (!toggles.Contains(toggle))
                toggles.Add(toggle);
        }

        public bool AnyTogglesOn()
        {
            return toggles.Find(x => x.isOn) != null;
        }

        public IEnumerable<OmniToggle> ActiveToggles()
        {
            return toggles.Where(x => x.isOn);
        }

        public void SetAllTogglesOff()
        {
            bool oldAllowSwitchOff = allowSwitchOff;
            allowSwitchOff = true;

            for (var i = 0; i < toggles.Count; i++)
                toggles[i].isOn = false;

            allowSwitchOff = oldAllowSwitchOff;
        }
    }
}