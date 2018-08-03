using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VRUI;

namespace SongBrowserPlugin.UI
{
    public class SettingsFlowCoordinator : FlowCoordinator
    {
        public const String Name = "SettingsFlowCoordinator";
        private Logger _log = new Logger(Name);

        private SettingsNavigationController _settingsNavigationController;
        private SettingsListViewController _settingsViewController;

        private Button _dismissButton;

        private bool _initialized;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentViewController"></param>
        /// <param name="levels"></param>
        /// <param name="gameplayMode"></param>
        public virtual void Present(VRUIViewController parentViewController)
        {
            if (!this._initialized)
            {
                _settingsNavigationController = UIBuilder.CreateViewController<SettingsNavigationController>("SettingsNavigationController");
                _settingsViewController = UIBuilder.CreateViewController<SettingsListViewController>("SettingsListViewController");

                this._initialized = true;
            }

            //this._levelSelectionNavigationController.InitWithGameplayModeIndicator(gameplayMode, true);
            parentViewController.PresentModalViewController(this._settingsNavigationController, null, parentViewController.isRebuildingHierarchy);

            //this._levelListViewController.Init(levels, false);
            this._settingsNavigationController.PushViewController(this._settingsViewController, parentViewController.isRebuildingHierarchy);            
        }
    }


}
