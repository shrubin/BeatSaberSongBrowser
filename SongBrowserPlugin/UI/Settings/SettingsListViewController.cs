using SongBrowserPlugin.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using VRUI;

namespace SongBrowserPlugin.UI
{
    public class SettingsListViewController : VRUI.VRUIViewController
    {
        public const String Name = "SettingsListViewController";

        private Logger _log = new Logger(Name);
        
        protected override void DidActivate(bool firstActivation, VRUIViewController.ActivationType activationType)
        {
            _log.Debug("DidActivate()");           

            base.DidActivate(firstActivation, activationType);            
        }
    }
}
