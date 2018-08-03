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

            _settingsButton = UIBuilder.CreateUIButton(_mainMenuRectTransform, "SettingsButton");
            Vector2 currentPos = (_settingsButton.transform as RectTransform).anchoredPosition;
            UIBuilder.SetButtonText(ref _settingsButton, "Browser Settings");
            (_settingsButton.transform as RectTransform).anchoredPosition = new Vector2(currentPos.x, currentPos.y - 5);
            (_settingsButton.transform as RectTransform).sizeDelta = new Vector2(50.0f, 5.0f);
            _settingsButton.onClick.AddListener(onSettingsButtonClicked);
        }

        private void onSettingsButtonClicked()
        {
            SettingsFlowCoordinator view = UIBuilder.CreateFlowCoordinator<SettingsFlowCoordinator>("SettingsFlowCoordinator");
            view.Present(_mainMenuViewController);
        }

        public void LateUpdate()
        {
            if (!this._levelListViewController.isActiveAndEnabled) return;

            if (Input.GetKeyDown(KeyCode.S))
            {
                onSettingsButtonClicked();
            }
        }
    }
}
