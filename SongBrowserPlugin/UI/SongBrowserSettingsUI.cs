using SongBrowserPlugin.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SongBrowserPlugin
{
    class SongBrowserSettingsUI : MonoBehaviour
    {
        // Logging
        public const String Name = "SongBrowserSettingsUI";

        private Logger _log = new Logger(Name);

        private MainMenuViewController _mainMenuViewController;
        private RectTransform _mainMenuRectTransform;

        private Button _settingsButton;

        public void CreateUI()
        {
            _log.Trace("CreateUI()");

            _mainMenuViewController = Resources.FindObjectsOfTypeAll<MainMenuViewController>().First();
            _mainMenuRectTransform = _mainMenuViewController.transform as RectTransform;
            Button buttonTemplate = Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "SettingsButton"));
            Vector2 currentSettingsButtonPos = (buttonTemplate.transform as RectTransform).anchoredPosition;
            Vector2 currentSettingsButtonSize = (buttonTemplate.transform as RectTransform).sizeDelta;

            _settingsButton = UIBuilder.CreateButton(_mainMenuRectTransform, buttonTemplate, "Browser Settings", 2.5f, currentSettingsButtonPos.x, currentSettingsButtonPos.y - 5, currentSettingsButtonSize.x, 5);
            _settingsButton.onClick.AddListener(onSettingsButtonClicked);
        }

        private void onSettingsButtonClicked()
        {
            SettingsFlowCoordinator view = UIBuilder.CreateFlowCoordinator<SettingsFlowCoordinator>("SettingsFlowCoordinator");
            view.Present(_mainMenuViewController);
        }

        public void LateUpdate()
        {
            if (!this._mainMenuViewController.isActiveAndEnabled) return;

            if (Input.GetKeyDown(KeyCode.S))
            {
                onSettingsButtonClicked();
            }
        }
    }
}
