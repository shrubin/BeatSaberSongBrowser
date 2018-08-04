using SongBrowserPlugin.DataAccess;
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

        private bool _initialized;

        private SongBrowserSettings _settings;

        public virtual void Present(VRUIViewController parentViewController)
        {
            if (!this._initialized)
            {
                _settingsNavigationController = UIBuilder.CreateViewController<SettingsNavigationController>("SettingsNavigationController");
                _settingsViewController = UIBuilder.CreateViewController<SettingsListViewController>("SettingsListViewController");

                this._initialized = true;
            }

            _settings = SongBrowserSettings.Load();

            parentViewController.PresentModalViewController(this._settingsNavigationController, null, parentViewController.isRebuildingHierarchy);
            this._settingsNavigationController.PushViewController(this._settingsViewController, parentViewController.isRebuildingHierarchy);

            this._settingsViewController.didSubmitSettingsEvent += HandleDidSubmitSettingsEvent;
            this._settingsViewController.didCancelSettingsEvent += HandleDidCancelSettingsEvent;
        }

        public void HandleDidSubmitSettingsEvent()
        {
            _settingsNavigationController.DismissModalViewController(delegate ()
            {
                _settings.Save();
            });
        }

        public void HandleDidCancelSettingsEvent()
        {
            _settingsNavigationController.DismissModalViewController(delegate ()
            {
            });
        }
    }


}
