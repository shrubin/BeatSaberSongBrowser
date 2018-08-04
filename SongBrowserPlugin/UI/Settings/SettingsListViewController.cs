using SongBrowserPlugin.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRUI;

namespace SongBrowserPlugin.UI
{
    public class SettingsListViewController : VRUI.VRUIViewController
    {
        public const String Name = "SettingsListViewController";

        private Logger _log = new Logger(Name);

        private TextMeshProUGUI _folderSupportLabel;
        private Button _folderSupportButton;

        private Button _submitButton;
        private Button _cancelButton;

        public Action didSubmitSettingsEvent;
        public Action didCancelSettingsEvent;

        protected override void DidActivate(bool firstActivation, VRUIViewController.ActivationType activationType)
        {
            _log.Debug("DidActivate()");           

            base.DidActivate(firstActivation, activationType);

            Button buttonTemplate = Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "QuitButton"));

            _folderSupportLabel = UIBuilder.CreateText(this.rectTransform, "Folders Support:", -35.0f, -10.5f);
            _folderSupportButton = UIBuilder.CreateButton(this.rectTransform, buttonTemplate, "Enabled", 3.0f, 0, 0, 0, 0);
            //_folderSupportButton.onClick.AddListener(onSettingsButtonClicked);

            _submitButton = UIBuilder.CreateButton(this.rectTransform, buttonTemplate, "Submit", 3.0f, 80, 0.5f, 15, 5);
            _submitButton.onClick.AddListener(HandleSubmitButtonPressed);

            _cancelButton = UIBuilder.CreateButton(this.rectTransform, buttonTemplate, "Cancel", 3.0f, 65, 0.5f, 15, 5);
            _cancelButton.onClick.AddListener(HandleCancelButtonPressed);
        }

        private void HandleSubmitButtonPressed()
        {
            didSubmitSettingsEvent.Invoke();            
        }

        private void HandleCancelButtonPressed()
        {
            didCancelSettingsEvent.Invoke();
        }
    }
}
