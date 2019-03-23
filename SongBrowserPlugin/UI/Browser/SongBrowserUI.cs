using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UI;
using HMUI;
using VRUI;
using SongBrowserPlugin.DataAccess;
using System.IO;
using SongLoaderPlugin;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Logger = SongBrowserPlugin.Logging.Logger;
using SongBrowserPlugin.DataAccess.BeatSaverApi;
using CustomUI.BeatSaber;
using SongBrowserPlugin.Internals;
using SongLoaderPlugin.OverrideClasses;
using SongBrowserPlugin.UI.Playlists;

namespace SongBrowserPlugin.UI
{
    /// <summary>
    /// Hijack the flow coordinator.  Have access to all StandardLevel easily.
    /// </summary>
    public class SongBrowserUI : MonoBehaviour
    {
        // Logging
        public const String Name = "SongBrowserUI";

        private const float SEGMENT_PERCENT = 0.1f;
        private const int LIST_ITEMS_VISIBLE_AT_ONCE = 6;

        // Beat Saber UI Elements
        private FlowCoordinator _levelSelectionFlowCoordinator;

        private LevelPacksViewController _levelPackViewController;
        private LevelPacksTableView _levelPacksTableView;
        private LevelPackDetailViewController _levelPackDetailViewController;

        private LevelPackLevelsViewController _levelPackLevelsViewController;
        private LevelPackLevelsTableView _levelPackLevelsTableView;
        private StandardLevelDetailViewController _levelDetailViewController;
        private StandardLevelDetailView _standardLevelDetailView;

        private BeatmapDifficultySegmentedControlController _levelDifficultyViewController;
        private BeatmapCharacteristicSegmentedControlController _beatmapCharacteristicSelectionViewController; 

        private DismissableNavigationController _levelSelectionNavigationController;        

        private RectTransform _tableViewRectTransform;

        private Button _tableViewPageUpButton;
        private Button _tableViewPageDownButton;
        private Button _playButton;

        // New UI Elements
        private List<SongSortButton> _sortButtonGroup;
        private List<SongFilterButton> _filterButtonGroup;

        private Button _clearSortFilterButton;

        private Button _addFavoriteButton;

        private SimpleDialogPromptViewController _simpleDialogPromptViewControllerPrefab;
        private SimpleDialogPromptViewController _deleteDialog;
        private Button _deleteButton;        

        private Button _pageUpFastButton;
        private Button _pageDownFastButton;

        private SearchKeyboardViewController _searchViewController;

        private PlaylistFlowCoordinator _playListFlowCoordinator;

        private RectTransform _ppStatButton;
        private RectTransform _starStatButton;
        private RectTransform _njsStatButton;

        // Cached items
        private Sprite _addFavoriteSprite;
        private Sprite _removeFavoriteSprite;
        private Sprite _currentAddFavoriteButtonSprite;

        // Plugin Compat checks
        private bool _detectedTwitchPluginQueue = false;
        private bool _checkedForTwitchPlugin = false;

        // Debug
        private int _sortButtonLastPushedIndex = 0;
        private int _lastRow = 0;

        // Model
        private SongBrowserModel _model;

        // UI Created
        private bool _uiCreated = false;

        public SongBrowserModel Model
        {
            get
            {
                return _model;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SongBrowserUI() : base()
        {
            if (_model == null)
            {
                _model = new SongBrowserModel();
            }

            _model.Init();

            _sortButtonLastPushedIndex = (int)(_model.Settings.sortMode);
        }

        /// <summary>
        /// Builds the UI for this plugin.
        /// </summary>
        public void CreateUI(MainMenuViewController.MenuButton mode)
        {
            Logger.Trace("CreateUI()");

            // Determine the flow controller to use
            if (mode == MainMenuViewController.MenuButton.SoloFreePlay)
            {
                Logger.Debug("Entering SOLO mode...");
                _levelSelectionFlowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
            }
            else if (mode == MainMenuViewController.MenuButton.Party)
            {
                Logger.Debug("Entering PARTY mode...");
                _levelSelectionFlowCoordinator = Resources.FindObjectsOfTypeAll<PartyFreePlayFlowCoordinator>().First();
            }
            else
            {
                Logger.Debug("Entering SOLO CAMPAIGN mode...");
                _levelSelectionFlowCoordinator = Resources.FindObjectsOfTypeAll<CampaignFlowCoordinator>().First();
            }

            // returning to the menu and switching modes could trigger this.
            if (_uiCreated)
            {
                return;
            }

            try
            {
                // gather controllers and ui elements.
                _levelPackViewController = _levelSelectionFlowCoordinator.GetPrivateField<LevelPacksViewController>("_levelPacksViewController");
                Logger.Debug("Acquired LevelPacksViewController [{0}]", _levelPackViewController.GetInstanceID());

                _levelPackDetailViewController = _levelSelectionFlowCoordinator.GetPrivateField<LevelPackDetailViewController>("_levelPackDetailViewController");
                Logger.Debug("Acquired LevelPackDetailViewController [{0}]", _levelPackDetailViewController.GetInstanceID());

                _levelPacksTableView = _levelPackViewController.GetPrivateField<LevelPacksTableView>("_levelPacksTableView");
                Logger.Debug("Acquired LevelPacksTableView [{0}]", _levelPacksTableView.GetInstanceID());

                _levelPackLevelsViewController = _levelSelectionFlowCoordinator.GetPrivateField<LevelPackLevelsViewController>("_levelPackLevelsViewController");
                Logger.Debug("Acquired LevelPackLevelsViewController [{0}]", _levelPackLevelsViewController.GetInstanceID());

                _levelPackLevelsTableView = this._levelPackLevelsViewController.GetComponentInChildren<LevelPackLevelsTableView>();
                Logger.Debug("Acquired LevelPackLevelsTableView [{0}]", _levelPackLevelsTableView.GetInstanceID());

                _levelDetailViewController = _levelSelectionFlowCoordinator.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
                Logger.Debug("Acquired StandardLevelDetailViewController [{0}]", _levelDetailViewController.GetInstanceID());

                _standardLevelDetailView = _levelDetailViewController.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
                Logger.Debug("Acquired StandardLevelDetailView [{0}]", _standardLevelDetailView.GetInstanceID());

                _beatmapCharacteristicSelectionViewController = Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSegmentedControlController>().First();
                Logger.Debug("Acquired BeatmapCharacteristicSegmentedControlController [{0}]", _beatmapCharacteristicSelectionViewController.GetInstanceID());

                _levelSelectionNavigationController = _levelSelectionFlowCoordinator.GetPrivateField<DismissableNavigationController>("_navigationController");
                Logger.Debug("Acquired DismissableNavigationController [{0}]", _levelSelectionNavigationController.GetInstanceID());

                _levelDifficultyViewController = _standardLevelDetailView.GetPrivateField<BeatmapDifficultySegmentedControlController>("_beatmapDifficultySegmentedControlController");
                Logger.Debug("Acquired BeatmapDifficultySegmentedControlController [{0}]", _levelDifficultyViewController.GetInstanceID());

                _tableViewRectTransform = _levelPackLevelsTableView.transform as RectTransform;
                Logger.Debug("Acquired TableViewRectTransform from LevelPackLevelsTableView [{0}]", _tableViewRectTransform.GetInstanceID());

                _tableViewPageUpButton = _tableViewRectTransform.GetComponentsInChildren<Button>().First(x => x.name == "PageUpButton");
                _tableViewPageDownButton = _tableViewRectTransform.GetComponentsInChildren<Button>().First(x => x.name == "PageDownButton");
                Logger.Debug("Acquired Page Up and Down buttons...");

                _playButton = _standardLevelDetailView.GetComponentsInChildren<Button>().FirstOrDefault(x => x.name == "PlayButton");
                Logger.Debug("Acquired PlayButton [{0}]", _playButton.GetInstanceID());

                _simpleDialogPromptViewControllerPrefab = Resources.FindObjectsOfTypeAll<SimpleDialogPromptViewController>().First();

                if (_playListFlowCoordinator == null)
                {
                    _playListFlowCoordinator = UIBuilder.CreateFlowCoordinator<PlaylistFlowCoordinator>("PlaylistFlowCoordinator");
                    _playListFlowCoordinator.didFinishEvent += HandleDidSelectPlaylist;
                }

                // delete dialog
                this._deleteDialog = UnityEngine.Object.Instantiate<SimpleDialogPromptViewController>(this._simpleDialogPromptViewControllerPrefab);
                this._deleteDialog.name = "DeleteDialogPromptViewController";
                this._deleteDialog.gameObject.SetActive(false);

                // sprites
                this._addFavoriteSprite = Base64Sprites.AddToFavoritesIcon;
                this._removeFavoriteSprite = Base64Sprites.RemoveFromFavoritesIcon;

                // create song browser main ui
                this.CreateUIElements();

                this.InstallHandlers();

                this.ResizeStatsPanel();

                //---

                var packImage = "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";

                var customLevelCollectionSO = new CustomLevelCollectionSO();

                Logger.Info("Beatmap collection songloader loaded {0}", SongLoader.CustomBeatmapLevelPackCollectionSO != null);

                var levelCollectionSO = Resources.FindObjectsOfTypeAll<BeatmapLevelCollectionSO>().FirstOrDefault();
                var CustomLevelCollectionSO = SongLoaderPlugin.OverrideClasses.CustomLevelCollectionSO.ReplaceOriginal(levelCollectionSO);
                var beatmapLevelPackCollectionSO = Resources.FindObjectsOfTypeAll<BeatmapLevelPackCollectionSO>().FirstOrDefault();
                var CustomBeatmapLevelPackCollectionSO = SongLoaderPlugin.SongLoader.CustomBeatmapLevelPackCollectionSO;
                var CustomBeatmapLevelPackSO = SongLoaderPlugin.OverrideClasses.CustomBeatmapLevelPackSO.GetPack(CustomLevelCollectionSO);
                CustomBeatmapLevelPackSO.SetPrivateField("_packID", "Top300");
                CustomBeatmapLevelPackSO.SetPrivateField("_packName", "Top300");
                CustomBeatmapLevelPackSO.SetPrivateField("_coverImage", CustomUI.Utilities.UIUtilities.LoadSpriteRaw(Convert.FromBase64String(packImage)));
                CustomBeatmapLevelPackCollectionSO.AddLevelPack(CustomBeatmapLevelPackSO);
                CustomBeatmapLevelPackCollectionSO.ReplaceReferences();

                var soloFreePlay = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().FirstOrDefault();
                LevelPacksViewController levelPacksViewController = (LevelPacksViewController)soloFreePlay.GetField("_levelPacksViewController");
                levelPacksViewController.SetData(CustomBeatmapLevelPackCollectionSO, 0);

                //var levelPackCol = customBeatmapLevelPackCollectionSO.GetField<IBeatmapLevelPackCollection>("_levelPackCollection"); NullReferenceException
                //Logger.Info("Betmap level packs {0}", levelPackCol.beatmapLevelPacks == null);
                //Logger.Info("Betmap level packs length {0}", levelPackCol.beatmapLevelPacks.Length);

                //----

                // make sure the quick scroll buttons don't desync with regular scrolling
                _tableViewPageDownButton.onClick.AddListener(delegate ()
                {
                    this.RefreshQuickScrollButtons();
                });
                _tableViewPageUpButton.onClick.AddListener(delegate ()
                {
                    this.RefreshQuickScrollButtons();
                });

                _uiCreated = true;
                Logger.Debug("Done Creating UI...");
            }
            catch (Exception e)
            {
                Logger.Exception("Exception during CreateUI: ", e);
            }
        }

        /// <summary>
        /// Builds the SongBrowser UI
        /// </summary>
        private void CreateUIElements()
        {
            Logger.Trace("CreateUIElements");

            try
            {
                // Gather some transforms and templates to use.
                RectTransform sortButtonTransform = this._levelSelectionNavigationController.transform as RectTransform;
                RectTransform otherButtonTransform = this._levelDetailViewController.transform as RectTransform;
                Button sortButtonTemplate = Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "SettingsButton"));
                Button otherButtonTemplate = Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "SettingsButton"));

                RectTransform playContainerRect = _standardLevelDetailView.GetComponentsInChildren<RectTransform>().First(x => x.name == "PlayContainer");
                RectTransform playButtonsRect = playContainerRect.GetComponentsInChildren<RectTransform>().First(x => x.name == "PlayButtons");

                Button practiceButton = playButtonsRect.GetComponentsInChildren<Button>().First(x => x.name == "PracticeButton");
                RectTransform practiceButtonRect = (practiceButton.transform as RectTransform);

                Button playButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "PlayButton");
                RectTransform playButtonRect = (playButton.transform as RectTransform);
                Sprite arrowIcon = SongBrowserApplication.Instance.CachedIcons["ArrowIcon"];
                Sprite borderSprite = SongBrowserApplication.Instance.CachedIcons["RoundRectBigStroke"];

                // Resize some of the UI
                _tableViewRectTransform.sizeDelta = new Vector2(0f, -20f);
                _tableViewRectTransform.anchoredPosition = new Vector2(0f, -2.5f);

                // Create Sorting Songs By-Buttons
                Logger.Debug("Creating sort by buttons...");
                float buttonSpacing = 0.5f;                                
                float fontSize = 2.0f;
                float buttonWidth = 12.25f;
                float buttonHeight = 5.5f;
                float startButtonX = 24.50f;
                float curButtonX = 0.0f;
                float buttonY = -6.0f;
                Vector2 iconButtonSize = new Vector2(buttonHeight, buttonHeight);

                // Create cancel button
                Logger.Debug("Creating cancel button...");
                _clearSortFilterButton = UIBuilder.CreateIconButton(
                    sortButtonTransform, 
                    otherButtonTemplate, 
                    Base64Sprites.XIcon, 
                    new Vector2(startButtonX - buttonHeight - (buttonSpacing * 2.0f), buttonY), 
                    new Vector2(iconButtonSize.x, iconButtonSize.y),
                    new Vector2(3.5f, 3.5f),
                    new Vector2(1.0f, 1.0f),
                    0);
                _clearSortFilterButton.onClick.RemoveAllListeners();
                _clearSortFilterButton.onClick.AddListener(delegate () {
                    OnClearButtonClickEvent();
                });

                startButtonX += (buttonHeight + buttonSpacing);

                // define sort buttons
                string[] sortButtonNames = new string[]
                {
                    "Song", "Author", "Newest", "Plays", "PP", "Difficult", "Random"
                };

                SongSortMode[] sortModes = new SongSortMode[]
                {
                    SongSortMode.Default, SongSortMode.Author, SongSortMode.Newest, SongSortMode.PlayCount, SongSortMode.PP, SongSortMode.Difficulty, SongSortMode.Random
                };
                
                _sortButtonGroup = new List<SongSortButton>();
                for (int i = 0; i < sortButtonNames.Length; i++)
                {
                    curButtonX = startButtonX + (buttonWidth*i) + (buttonSpacing*i);
                    SongSortButton newButton = UIBuilder.CreateSortButton(sortButtonTransform, sortButtonTemplate, arrowIcon, borderSprite,
                        sortButtonNames[i],
                        fontSize,
                        curButtonX,
                        buttonY,
                        buttonWidth,
                        buttonHeight,
                        sortModes[i],
                        OnSortButtonClickEvent);
                    _sortButtonGroup.Add(newButton);
                    newButton.Button.name = "Sort" + sortModes[i].ToString() + "Button";
                }

                // Create filter buttons
                Logger.Debug("Creating filter buttons...");

                float filterButtonX = curButtonX + (buttonWidth / 2.0f);

                List<Tuple<SongFilterMode, UnityEngine.Events.UnityAction, Sprite>> filterButtonSetup = new List<Tuple<SongFilterMode, UnityEngine.Events.UnityAction, Sprite>>()
                {
                    Tuple.Create<SongFilterMode, UnityEngine.Events.UnityAction, Sprite>(SongFilterMode.Favorites, OnFavoriteFilterButtonClickEvent, Base64Sprites.StarFullIcon),
                    Tuple.Create<SongFilterMode, UnityEngine.Events.UnityAction, Sprite>(SongFilterMode.Playlist, OnPlaylistButtonClickEvent, Base64Sprites.PlaylistIcon),
                    Tuple.Create<SongFilterMode, UnityEngine.Events.UnityAction, Sprite>(SongFilterMode.Search, OnSearchButtonClickEvent, Base64Sprites.SearchIcon),
                };

                _filterButtonGroup = new List<SongFilterButton>();
                for (int i = 0; i < filterButtonSetup.Count; i++)
                {
                    Tuple<SongFilterMode, UnityEngine.Events.UnityAction, Sprite> t = filterButtonSetup[i];
                    Button b = UIBuilder.CreateIconButton(sortButtonTransform, otherButtonTemplate,
                        t.Item3,
                        new Vector2(filterButtonX + (iconButtonSize.x * i) + (buttonSpacing * i), buttonY),
                        new Vector2(iconButtonSize.x, iconButtonSize.y), 
                        new Vector2(3.5f, 3.5f),
                        new Vector2(1.0f, 1.0f),
                        0);
                    SongFilterButton filterButton = new SongFilterButton
                    {
                        Button = b,
                        FilterMode = t.Item1
                    };
                    b.onClick.AddListener(t.Item2);
                    filterButton.Button.name = "Filter" + t.Item1.ToString() + "Button";
                    _filterButtonGroup.Add(filterButton);                    
                }

                // Create add favorite button
                Logger.Debug("Creating Add to favorites button...");
                _addFavoriteButton = UIBuilder.CreateIconButton(playButtonsRect,
                    practiceButton,
                    Base64Sprites.AddToFavoritesIcon
                );
                _addFavoriteButton.onClick.RemoveAllListeners();
                _addFavoriteButton.onClick.AddListener(delegate () {
                    ToggleSongInPlaylist();
                });

                // Create delete button          
                Logger.Debug("Creating delete button...");
                _deleteButton = UIBuilder.CreateIconButton(playButtonsRect,
                    practiceButton,
                    Base64Sprites.DeleteIcon
                );
                _deleteButton.onClick.RemoveAllListeners();
                _deleteButton.onClick.AddListener(delegate () {
                    HandleDeleteSelectedLevel();
                });

                // Create fast scroll buttons
                int pageFastButtonX = 25;
                Vector2 pageFastSize = new Vector2(12.5f, 7.75f);
                Vector2 pageFastIconSize = new Vector2(0.1f, 0.1f);
                Vector2 pageFastIconScale = new Vector2(0.4f, 0.4f);

                Logger.Debug("Creating fast scroll button...");
                _pageUpFastButton = UIBuilder.CreateIconButton(sortButtonTransform, otherButtonTemplate, arrowIcon,
                    new Vector2(pageFastButtonX, -13f),
                    pageFastSize,
                    pageFastIconSize,
                    pageFastIconScale,
                    180);
                UnityEngine.GameObject.Destroy(_pageUpFastButton.GetComponentsInChildren<UnityEngine.UI.Image>().First(btn => btn.name == "Stroke"));
                _pageUpFastButton.onClick.AddListener(delegate ()
                {
                    this.JumpSongList(-1, SEGMENT_PERCENT);
                });
                
                _pageDownFastButton = UIBuilder.CreateIconButton(sortButtonTransform, otherButtonTemplate, arrowIcon,
                    new Vector2(pageFastButtonX, -80f),
                    pageFastSize,
                    pageFastIconSize,
                    pageFastIconScale,
                    0);
                
                UnityEngine.GameObject.Destroy(_pageDownFastButton.GetComponentsInChildren<UnityEngine.UI.Image>().First(btn => btn.name == "Stroke"));
                _pageDownFastButton.onClick.AddListener(delegate ()
                {
                    this.JumpSongList(1, SEGMENT_PERCENT);
                });
                                
                RefreshSortButtonUI();

                Logger.Debug("Done Creating UIElements");
            }
            catch (Exception e)
            {
                Logger.Exception("Exception CreateUIElements:", e);
            }
        }

        /// <summary>
        /// Resize the stats panel to fit more stats.
        /// </summary>
        private void ResizeStatsPanel()
        {
            // modify details view
            Logger.Debug("Resizing Stats Panel...");

            var statsPanel = _standardLevelDetailView.GetPrivateField<LevelParamsPanel>("_levelParamsPanel");
            var statTransforms = statsPanel.GetComponentsInChildren<RectTransform>();
            var valueTexts = statsPanel.GetComponentsInChildren<TextMeshProUGUI>().Where(x => x.name == "ValueText").ToList();
            RectTransform panelRect = (statsPanel.transform as RectTransform);
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x * 1.2f, panelRect.sizeDelta.y * 1.2f);

            for (int i = 0; i < statTransforms.Length; i++)
            {
                var r = statTransforms[i];
                if (r.name == "Separator")
                {
                    continue;
                }
                r.sizeDelta = new Vector2(r.sizeDelta.x * 0.75f, r.sizeDelta.y * 0.75f);
            }

            for (int i = 0; i < valueTexts.Count; i++)
            {
                var text = valueTexts[i];
                text.fontSize = 3.25f;
            }

            _ppStatButton = UnityEngine.Object.Instantiate(statTransforms[1], statsPanel.transform, false);
            UIBuilder.SetStatButtonIcon(_ppStatButton, Base64Sprites.GraphIcon);

            _starStatButton = UnityEngine.Object.Instantiate(statTransforms[1], statsPanel.transform, false);
            UIBuilder.SetStatButtonIcon(_starStatButton, Base64Sprites.StarIcon);

            _njsStatButton = UnityEngine.Object.Instantiate(statTransforms[1], statsPanel.transform, false);
            UIBuilder.SetStatButtonIcon(_njsStatButton, Base64Sprites.SpeedIcon);

            // shrink title
            var titleText = this._levelDetailViewController.GetComponentsInChildren<TextMeshProUGUI>(true).First(x => x.name == "SongNameText");
            
            titleText.fontSize = 5.0f;
        }

        /// <summary>
        /// Show the UI.
        /// </summary>
        public void Show()
        {
            Logger.Trace("Show SongBrowserUI()");

            this.SetVisibility(true);
        }

        /// <summary>
        /// Hide the UI.
        /// </summary>
        public void Hide()
        {
            Logger.Trace("Hide SongBrowserUI()");

            this.SetVisibility(false);
        }

        /// <summary>
        /// Handle showing or hiding UI logic.
        /// </summary>
        /// <param name="visible"></param>
        private void SetVisibility(bool visible)
        {
            // UI not created, nothing visible to hide...
            if (!_uiCreated)
            {
                return;
            }

            _ppStatButton.gameObject.SetActive(visible);
            _starStatButton.gameObject.SetActive(visible);
            _njsStatButton.gameObject.SetActive(visible);

            _clearSortFilterButton.gameObject.SetActive(visible);
            _sortButtonGroup.ForEach(x => x.Button.gameObject.SetActive(visible));
            _filterButtonGroup.ForEach(x => x.Button.gameObject.SetActive(visible));

            _addFavoriteButton.gameObject.SetActive(visible);
            _deleteButton.gameObject.SetActive(visible);

            _pageUpFastButton.gameObject.SetActive(visible);
            _pageDownFastButton.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Add our handlers into BeatSaber.
        /// </summary>
        private void InstallHandlers()
        {
            // handlers
            TableView tableView = ReflectionUtil.GetPrivateField<TableView>(_levelPackLevelsTableView, "_tableView");
            tableView.didSelectCellWithIdxEvent += HandleDidSelectTableViewRow;
            _levelPackLevelsViewController.didSelectLevelEvent += OnDidSelectLevelEvent;
            _levelDifficultyViewController.didSelectDifficultyEvent += OnDidSelectDifficultyEvent;

            var packListTableView = _levelPacksTableView;

            _levelPacksTableView.didSelectPackEvent += _levelPacksTableView_didSelectPackEvent;
            _levelPackViewController.didSelectPackEvent += _levelPackViewController_didSelectPackEvent;
                  
            _beatmapCharacteristicSelectionViewController.didSelectBeatmapCharacteristicEvent += OnDidSelectBeatmapCharacteristic;
        }

        /// <summary>
        /// Handler for level pack selection.
        /// UNUSED
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void _levelPacksTableView_didSelectPackEvent(LevelPacksTableView arg1, IBeatmapLevelPack arg2)
        {
            Logger.Trace("_levelPacksTableView_didSelectPackEvent(arg2={0})", arg2);

            try
            {
                if (this._model.Settings.filterMode == SongFilterMode.Playlist)
                {
                    this._model.Settings.filterMode = SongFilterMode.None;
                    this._model.Settings.Save();
                }

                this._model.SetCurrentLevelPack(arg2);
                this._model.ProcessSongList();

                RefreshSongList();
                RefreshSortButtonUI();
                RefreshQuickScrollButtons();
            }
            catch (Exception e)
            {
                Logger.Exception("Exception handling didSelectPackEvent...", e);
            }
        }

        /// <summary>
        /// Handler for level pack selection, controller.
        /// Sets the current level pack into the model and updates.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void _levelPackViewController_didSelectPackEvent(LevelPacksViewController arg1, IBeatmapLevelPack arg2)
        {
            Logger.Trace("_levelPackViewController_didSelectPackEvent(arg2={0})", arg2);

            try
            {
                RefreshSongList();
                RefreshSortButtonUI();
                RefreshQuickScrollButtons();
            }
            catch (Exception e)
            {
                Logger.Exception("Exception handling didSelectPackEvent...", e);
            }
        }

        private void OnClearButtonClickEvent()
        {
            Logger.Debug("Clearing all sorts and filters.");

            _model.Settings.sortMode = SongSortMode.Original;
            _model.Settings.filterMode = SongFilterMode.None;
            _model.Settings.invertSortResults = false;
            _model.Settings.Save();
            
            this._model.ProcessSongList();
            RefreshSongList();
            RefreshSortButtonUI();
        }

        /// <summary>
        /// Sort button clicked.
        /// </summary>
        private void OnSortButtonClickEvent(SongSortMode sortMode)
        {
            Logger.Debug("Sort button - {0} - pressed.", sortMode.ToString());
            _model.LastSelectedLevelId = null;

            if (_model.Settings.sortMode == sortMode)
            {
                _model.ToggleInverting();
            }

            _model.Settings.sortMode = sortMode;

            // update the seed
            if (_model.Settings.sortMode == SongSortMode.Random)
            {
                this.Model.Settings.randomSongSeed = Guid.NewGuid().GetHashCode();
            }

            _model.Settings.Save();

            this._model.ProcessSongList();
            RefreshSongList();
            RefreshSortButtonUI();
            RefreshQuickScrollButtons();

            // Handle instant queue logic
            if (_model.Settings.sortMode == SongSortMode.Random && _model.Settings.randomInstantQueue)
            {
                int index = 0;
                if (_model.SortedSongList.Count > index)
                {
                    this.SelectAndScrollToLevel(_levelPackLevelsTableView, _model.SortedSongList[index].levelID);
                    var beatMapDifficulties = _model.SortedSongList[index].difficultyBeatmapSets
                        .Where(x => x.beatmapCharacteristic == _model.CurrentBeatmapCharacteristicSO)
                        .SelectMany(x => x.difficultyBeatmaps);
                    this._levelDifficultyViewController.HandleDifficultySegmentedControlDidSelectCell(null, beatMapDifficulties.Count() - 1);
                    _playButton.onClick.Invoke();
                }
            }

            //Scroll to start of the list
            var levelsTableView = _levelPackLevelsViewController.GetPrivateField<LevelPackLevelsTableView>("_levelPackLevelsTableView");
            TableView listTableView = levelsTableView.GetPrivateField<TableView>("_tableView");
            listTableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
        }

        /// <summary>
        /// Filter by favorites.
        /// </summary>
        private void OnFavoriteFilterButtonClickEvent()
        {
            Logger.Debug("Filter button - {0} - pressed.", SongFilterMode.Favorites.ToString());

            if (_model.Settings.filterMode != SongFilterMode.Favorites)
            {
                _model.Settings.filterMode = SongFilterMode.Favorites;
            }
            else
            {
                _model.Settings.filterMode = SongFilterMode.None;
            }
            _model.Settings.Save();

            _model.ProcessSongList();
            RefreshSongList();
            RefreshSortButtonUI();
            RefreshQuickScrollButtons();
        }

        /// <summary>
        /// Filter button clicked.  
        /// </summary>
        /// <param name="sortMode"></param>
        private void OnSearchButtonClickEvent()
        {
            Logger.Debug("Filter button - {0} - pressed.", SongFilterMode.Search.ToString());
            if (_model.Settings.filterMode != SongFilterMode.Search)
            {
                this.ShowSearchKeyboard();
            }
            else
            {
                _model.Settings.filterMode = SongFilterMode.None;
                _model.ProcessSongList();

                RefreshSongList();
                RefreshSortButtonUI();
                RefreshQuickScrollButtons();
            }
            _model.Settings.Save();            
        }

        /// <summary>
        /// Display the playlist selector.
        /// </summary>
        /// <param name="sortMode"></param>
        private void OnPlaylistButtonClickEvent()
        {
            Logger.Debug("Filter button - {0} - pressed.", SongFilterMode.Playlist.ToString());
            _model.LastSelectedLevelId = null;

            if (_model.Settings.filterMode != SongFilterMode.Playlist)
            {
                _playListFlowCoordinator.parentFlowCoordinator = _levelSelectionFlowCoordinator;
                _levelSelectionFlowCoordinator.InvokePrivateMethod("PresentFlowCoordinator", new object[] { _playListFlowCoordinator, null, false, false });                                
            }
            else
            {
                _model.Settings.filterMode = SongFilterMode.None;
                _model.Settings.Save();
                _model.ProcessSongList();

                RefreshSongList();
                RefreshSortButtonUI();
                RefreshQuickScrollButtons();
            }
        }

        /// <summary>
        /// Adjust UI based on level selected.
        /// Various ways of detecting if a level is not properly selected.  Seems most hit the first one.
        /// </summary>
        private void OnDidSelectLevelEvent(LevelPackLevelsViewController view, IPreviewBeatmapLevel level)
        {            
            try
            {
                Logger.Trace("OnDidSelectLevelEvent()");
                if (level == null)
                {
                    Logger.Debug("No level selected?");
                    return;
                }

                if (_model.Settings == null)
                {
                    Logger.Debug("Settings not instantiated yet?");
                    return;
                }

                _model.LastSelectedLevelId = level.levelID;

                RefreshAddFavoriteButton(level.levelID);
                RefreshQuickScrollButtons();

                HandleDidSelectLevelRow(level);
            }
            catch (Exception e)
            {
                Logger.Exception("Exception selecting song:", e);
            }
        }

        /// <summary>
        /// Switching one-saber modes for example.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="bc"></param>
        private void OnDidSelectBeatmapCharacteristic(BeatmapCharacteristicSegmentedControlController view, BeatmapCharacteristicSO bc)
        {
            Logger.Trace("OnDidSelectBeatmapCharacteristic({0}", bc.name);
            _model.CurrentBeatmapCharacteristicSO = bc;
            _model.UpdateLevelRecords();
            this.RefreshSongList();
        }

        /// <summary>
        /// Handle difficulty level selection.
        /// </summary>
        private void OnDidSelectDifficultyEvent(BeatmapDifficultySegmentedControlController view, BeatmapDifficulty beatmap)
        {
            Logger.Trace("OnDidSelectDifficultyEvent({0})", beatmap);

            _deleteButton.interactable = (_levelDetailViewController.selectedDifficultyBeatmap.level.levelID.Length >= 32);

            this.RefreshScoreSaberData(_levelDetailViewController.selectedDifficultyBeatmap.level);
        }

        /// <summary>
        /// Refresh stats panel.
        /// </summary>
        /// <param name="level"></param>
        private void HandleDidSelectLevelRow(IPreviewBeatmapLevel level)
        {
            Logger.Trace("HandleDidSelectLevelRow({0})", level);

            _deleteButton.interactable = (level.levelID.Length >= 32);

            this.RefreshScoreSaberData(level);
        }

        /// <summary>
        /// Track the current row.
        /// </summary>
        /// <param name="tableView"></param>
        /// <param name="row"></param>
        private void HandleDidSelectTableViewRow(TableView tableView, int row)
        {
            Logger.Trace("HandleDidSelectTableViewRow({0})", row);
            _lastRow = row;
        }

        /// <summary>
        /// Pop up a delete dialog.
        /// </summary>
        private void HandleDeleteSelectedLevel()
        {
            IBeatmapLevel level = _levelDetailViewController.selectedDifficultyBeatmap.level;
            _deleteDialog.Init("Delete song", $"Do you really want to delete \"{ level.songName} {level.songSubName}\"?", "Delete", "Cancel",
                (selectedButton) =>
                {
                    _levelSelectionFlowCoordinator.InvokePrivateMethod("DismissViewController", new object[] { _deleteDialog, null, false });
                    if (selectedButton == 0)
                    {
                        try
                        {
                            var levelsTableView = _levelPackLevelsViewController.GetPrivateField<LevelPackLevelsTableView>("_levelPackLevelsTableView");
                            List<IPreviewBeatmapLevel> levels = levelsTableView.GetPrivateField<IBeatmapLevelPack>("_pack").beatmapLevelCollection.beatmapLevels.ToList();
                            int selectedIndex = levels.FindIndex(x => x.levelID == _levelDetailViewController.selectedDifficultyBeatmap.level.levelID);

                            SongDownloader.Instance.DeleteSong(new Song(SongLoader.CustomLevels.First(x => x.levelID == _levelDetailViewController.selectedDifficultyBeatmap.level.levelID)));

                            if (selectedIndex > -1)
                            {
                                int removedLevels = levels.RemoveAll(x => x.levelID == _levelDetailViewController.selectedDifficultyBeatmap.level.levelID);
                                Logger.Log("Removed " + removedLevels + " level(s) from song list!");

                                this.UpdateLevelDataModel();
                                this.RefreshSongList();

                                TableView listTableView = levelsTableView.GetPrivateField<TableView>("_tableView");
                                listTableView.ScrollToCellWithIdx(selectedIndex, TableView.ScrollPositionType.Beginning, false);
                                levelsTableView.SetPrivateField("_selectedRow", selectedIndex);
                                listTableView.SelectCellWithIdx(selectedIndex, true);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Unable to delete song! Exception: " + e);
                        }
                    }
                });
            _levelSelectionFlowCoordinator.InvokePrivateMethod("PresentViewController", new object[] { _deleteDialog, null, false });
        }        

        /// <summary>
        /// Create MD5 of a file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string CreateMD5FromFile(string path)
        {
            string hash = "";
            if (!File.Exists(path)) return null;
            using (MD5 md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);

                    StringBuilder sb = new StringBuilder();
                    foreach (byte hashByte in hashBytes)
                    {
                        sb.Append(hashByte.ToString("X2"));
                    }

                    hash = sb.ToString();
                    return hash;
                }
            }
        }

        /// <summary>
        /// Handle selection of a playlist.  Show just the songs in the playlist.
        /// </summary>
        /// <param name="p"></param>
        private void HandleDidSelectPlaylist(Playlist p)
        {
            if (p != null)
            {
                Logger.Debug("Showing songs for playlist: {0}", p.playlistTitle);
                _model.Settings.filterMode = SongFilterMode.Playlist;
                _model.CurrentPlaylist = p;
                _model.Settings.Save();
                _model.ProcessSongList();

                this.RefreshSongList();
                this.RefreshSortButtonUI();
            }
            else
            {
                Logger.Debug("No playlist selected");
            }
        }

        /// <summary>
        /// Display the search keyboard
        /// </summary>
        void ShowSearchKeyboard()
        {
            if (_searchViewController == null)
            {
                _searchViewController = UIBuilder.CreateViewController<SearchKeyboardViewController>("SearchKeyboardViewController");
                _searchViewController.searchButtonPressed += SearchViewControllerSearchButtonPressed;
                _searchViewController.backButtonPressed += SearchViewControllerbackButtonPressed;
            }

            Logger.Debug("Presenting search keyboard");
            _levelSelectionFlowCoordinator.InvokePrivateMethod("PresentViewController", new object[] { _searchViewController, null, false });
        }

        /// <summary>
        /// Handle back button event from search keyboard.
        /// </summary>
        private void SearchViewControllerbackButtonPressed()
        {
            _levelSelectionFlowCoordinator.InvokePrivateMethod("DismissViewController", new object[] { _searchViewController, null, false });

            // force disable search filter.
            this._model.Settings.filterMode = SongFilterMode.None;
            this._model.Settings.Save();

            RefreshSortButtonUI();
            RefreshQuickScrollButtons();
        }

        /// <summary>
        /// Handle search.
        /// </summary>
        /// <param name="searchFor"></param>
        private void SearchViewControllerSearchButtonPressed(string searchFor)
        {
            _levelSelectionFlowCoordinator.InvokePrivateMethod("DismissViewController", new object[] { _searchViewController, null, false });

            Logger.Debug("Searching for \"{0}\"...", searchFor);

            _model.Settings.filterMode = SongFilterMode.Search;
            _model.Settings.searchTerms.Insert(0, searchFor);
            _model.Settings.Save();
            _model.LastSelectedLevelId = null;
            _model.ProcessSongList();

            RefreshSongList();
            RefreshSortButtonUI();
            RefreshQuickScrollButtons();
        }

        /// <summary>
        /// Make big jumps in the song list.
        /// </summary>
        /// <param name="numJumps"></param>
        private void JumpSongList(int numJumps, float segmentPercent)
        {
            int totalSize = _model.SortedSongList.Count;
            int segmentSize = (int)(totalSize * segmentPercent);

            // Jump at least one scree size.
            if (segmentSize < LIST_ITEMS_VISIBLE_AT_ONCE)
            {
                segmentSize = LIST_ITEMS_VISIBLE_AT_ONCE;
            }

            TableView tableView = ReflectionUtil.GetPrivateField<TableView>(_levelPackLevelsTableView, "_tableView");
            int jumpDirection = Math.Sign(numJumps);
            int newRow = _lastRow + (jumpDirection * segmentSize);
            
            if (newRow <= 0)
            {
                newRow = 0;
            }
            else if (newRow >= totalSize)
            {
                newRow = totalSize - 1;
            }
            
            Logger.Debug("jumpDirection: {0}, newRow: {1}", jumpDirection, newRow);
            _lastRow = newRow;
            this.SelectAndScrollToLevel(_levelPackLevelsTableView, _model.SortedSongList[newRow].levelID);
        }

        /// <summary>
        /// Add/Remove song from favorites depending on if it already exists.
        /// </summary>
        private void ToggleSongInPlaylist()
        {
            IBeatmapLevel songInfo = _levelDetailViewController.selectedDifficultyBeatmap.level;
            if (_model.CurrentEditingPlaylist != null)
            {
                if (_model.CurrentEditingPlaylistLevelIds.Contains(songInfo.levelID))
                {
                    Logger.Info("Remove {0} from editing playlist", songInfo.songName);
                    _model.RemoveSongFromEditingPlaylist(songInfo);
                }
                else
                {
                    Logger.Info("Add {0} to editing playlist", songInfo.songName);
                    _model.AddSongToEditingPlaylist(songInfo);
                }
            }

            RefreshAddFavoriteButton(songInfo.levelID);

            _model.Settings.Save();
        }

        /// <summary>
        /// Update GUI elements that show score saber data.
        /// </summary>
        public void RefreshScoreSaberData(IPreviewBeatmapLevel level)
        {
            Logger.Trace("RefreshScoreSaberData({0})", level.levelID);

            // use controllers level...
            if (level == null)
            {
                level = _levelDetailViewController.selectedDifficultyBeatmap.level;
            }

            // abort!
            if (level == null)
            {
                Logger.Debug("Aborting RefreshScoreSaberData()");
                return;
            }

            BeatmapDifficulty difficulty = this._levelDifficultyViewController.selectedDifficulty;
            string njsText;
            string difficultyString = difficulty.ToString();

            //Grab NJS for difficulty
            //Default to 10 if a standard level
            float njs = 0;
            if (!_model.LevelIdToCustomSongInfos.ContainsKey(level.levelID))
            {
                njsText = "OST";
            }
            else
            {
                //Grab the matching difficulty level
                SongLoaderPlugin.OverrideClasses.CustomLevel customLevel = _model.LevelIdToCustomSongInfos[level.levelID];
                CustomSongInfo.DifficultyLevel difficultyLevel = null;
                foreach (var diffLevel in customLevel.customSongInfo.difficultyLevels)
                {
                    if (diffLevel.difficulty == difficultyString)
                    {
                        difficultyLevel = diffLevel;                            
                        break;
                    }
                }

                // set njs text
                if (difficultyLevel == null || String.IsNullOrEmpty(difficultyLevel.json))
                {
                    njsText = "NA";
                }
                else
                {
                    njs = GetNoteJump(difficultyLevel.json);
                    njsText = njs.ToString();
                }
            }
            UIBuilder.SetStatButtonText(_njsStatButton, njsText);

            // check if we have score saber data
            if (this._model.LevelIdToScoreSaberData != null)
            {
                // Check for PP
                Logger.Debug("Checking if have info for song {0}", level.songName);
                if (this._model.LevelIdToScoreSaberData.ContainsKey(level.levelID))
                {
                    Logger.Debug("Checking if have difficulty for song {0} difficulty {1}", level.songName, difficultyString);
                    ScoreSaberData ppData = this._model.LevelIdToScoreSaberData[level.levelID];
                    if (ppData.difficultyToSaberDifficulty.ContainsKey(difficultyString))
                    {
                        Logger.Debug("Display pp for song.");
                        float pp = ppData.difficultyToSaberDifficulty[difficultyString].pp;
                        float star = ppData.difficultyToSaberDifficulty[difficultyString].star;

                        UIBuilder.SetStatButtonText(_ppStatButton, String.Format("{0:0.#}", pp));
                        UIBuilder.SetStatButtonText(_starStatButton, String.Format("{0:0.#}", star));
                    }
                    else
                    {
                        UIBuilder.SetStatButtonText(_ppStatButton, "NA");
                        UIBuilder.SetStatButtonText(_starStatButton, "NA");
                    }
                }
                else
                {
                    UIBuilder.SetStatButtonText(_ppStatButton, "?");
                    UIBuilder.SetStatButtonText(_starStatButton, "?");
                }
                
            }
            else
            {
                Logger.Debug("No ScoreSaberData available...  Cannot display pp/star stats...");
            }



            Logger.Debug("Done refreshing score saber stats.");
        }

        /// <summary>
        /// Update interactive state of the quick scroll buttons.
        /// </summary>
        private void RefreshQuickScrollButtons()
        {
            // if you are ever viewing the song list with less than 5 songs the up/down buttons do not exist.
            // just try and fetch them and ignore the exception.
            if (_tableViewPageUpButton == null)
            {
                try
                {
                    _tableViewPageUpButton = _tableViewRectTransform.GetComponentsInChildren<Button>().First(x => x.name == "PageUpButton");
                    (_tableViewPageUpButton.transform as RectTransform).anchoredPosition = new Vector2(0f, -1f);
                }
                catch (Exception)
                {
                    // We don't care if this fails.
                    return;
                }
            }

            if (_tableViewPageDownButton == null)
            {
                try
                {
                    _tableViewPageDownButton = _tableViewRectTransform.GetComponentsInChildren<Button>().First(x => x.name == "PageDownButton");
                    (_tableViewPageDownButton.transform as RectTransform).anchoredPosition = new Vector2(0f, 1f);
                }
                catch (Exception)
                {
                    // We don't care if this fails.
                    return;
                }
            }

            // Refresh the fast scroll buttons
            if (_tableViewPageUpButton != null && _pageUpFastButton != null)
            {
                _pageUpFastButton.interactable = _tableViewPageUpButton.interactable;
                _pageUpFastButton.gameObject.SetActive(_tableViewPageUpButton.IsActive());
            }

            if (_tableViewPageDownButton != null && _pageUpFastButton != null)
            {
                _pageDownFastButton.interactable = _tableViewPageDownButton.interactable;
                _pageDownFastButton.gameObject.SetActive(_tableViewPageDownButton.IsActive());
            }
        }

        /// <summary>
        /// Helper to quickly refresh add to favorites button
        /// </summary>
        /// <param name="levelId"></param>
        private void RefreshAddFavoriteButton(String levelId)
        {
            if (levelId == null)
            {
                _currentAddFavoriteButtonSprite = null;
            }
            else
            {
                if (_model.CurrentEditingPlaylistLevelIds.Contains(levelId))
                {
                    _currentAddFavoriteButtonSprite = _removeFavoriteSprite;
                }
                else
                {
                    _currentAddFavoriteButtonSprite = _addFavoriteSprite;
                }
            }

            UIBuilder.SetButtonIcon(_addFavoriteButton, _currentAddFavoriteButtonSprite);
        }

        /// <summary>
        /// Adjust the UI colors.
        /// </summary>
        public void RefreshSortButtonUI()
        {
            // So far all we need to refresh is the sort buttons.
            foreach (SongSortButton sortButton in _sortButtonGroup)
            {
                //UIBuilder.SetButtonTextColor(sortButton.Button, Color.white);
                UIBuilder.SetButtonBorder(sortButton.Button, Color.white);
                if (sortButton.SortMode == _model.Settings.sortMode)
                {
                    if (this._model.Settings.invertSortResults)
                    {
                        //UIBuilder.SetButtonTextColor(sortButton.Button, Color.red);
                        UIBuilder.SetButtonBorder(sortButton.Button, Color.red);
                    }
                    else
                    {
                        //UIBuilder.SetButtonTextColor(sortButton.Button, Color.green);
                        UIBuilder.SetButtonBorder(sortButton.Button, Color.green);
                    }
                }
            }

            // refresh filter buttons
            foreach (SongFilterButton filterButton in _filterButtonGroup)
            {
                UIBuilder.SetButtonBorder(filterButton.Button, Color.white);
                if (filterButton.FilterMode == _model.Settings.filterMode)
                {
                    UIBuilder.SetButtonBorder(filterButton.Button, Color.green);
                }
            }
        }

        /// <summary>
        /// Try to refresh the song list.  Broken for now.
        /// </summary>
        public void RefreshSongList()
        {
            Logger.Info("Refreshing the song list view.");
            try
            {
                // TODO - remove as part of unifying the we handle the song lists
                if (_model.IsCurrentLevelPackPreview)
                {
                    return;
                }

                if (_model.SortedSongList == null)
                {
                    Logger.Debug("Songs are not sorted yet, nothing to refresh.");
                    return;
                }

                var levels = _model.SortedSongList.ToArray();

                Logger.Debug("Checking if TableView is initialized...");
                TableView tableView = ReflectionUtil.GetPrivateField<TableView>(_levelPackLevelsTableView, "_tableView");
                bool tableViewInit = ReflectionUtil.GetPrivateField<bool>(tableView, "_isInitialized");
                if (!tableViewInit)
                {
                    Logger.Debug("LevelPackLevelListTableView.TableView is not initialized... nothing to reload...");
                    return;
                }

                Logger.Debug("Reloading SongList TableView");
                tableView.ReloadData();

                Logger.Debug("Attempting to scroll to level...");
                String selectedLevelID = null;
                if (_model.LastSelectedLevelId != null)
                {
                    selectedLevelID = _model.LastSelectedLevelId;
                    Logger.Debug("Scrolling to row for level ID: {0}", selectedLevelID);                    
                }
                else
                {
                    if (levels.Length > 0)
                    {
                        selectedLevelID = levels.FirstOrDefault().levelID;
                    }
                }

                // HACK, seems like if 6 or less items scrolling to row causes the song list to disappear.
                //if (levels.Length > 6 && !String.IsNullOrEmpty(selectedLevelID) && levels.Any(x => x.levelID == selectedLevelID))
                {
                    SelectAndScrollToLevel(_levelPackLevelsTableView, selectedLevelID);
                }            
            }
            catch (Exception e)
            {
                Logger.Exception("Exception refreshing song list:", e);
            }
        }

        /// <summary>
        /// Scroll TableView to proper row, fire events.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="levelID"></param>
        private void SelectAndScrollToLevel(LevelPackLevelsTableView table, string levelID)
        {
            Logger.Debug("Scrolling to LevelID: {0}", levelID);

            // Check once per load
            if (!_checkedForTwitchPlugin)
            {
                Logger.Info("Checking for BeatSaber Twitch Integration Plugin...");

                // Try to detect BeatSaber Twitch Integration Plugin
                _detectedTwitchPluginQueue = Resources.FindObjectsOfTypeAll<VRUIViewController>().Any(x => x.name == "RequestInfo");

                Logger.Info("BeatSaber Twitch Integration plugin detected: " + _detectedTwitchPluginQueue);

                _checkedForTwitchPlugin = true;
            }

            // Skip scrolling to level if twitch plugin has queue active.
            if (_detectedTwitchPluginQueue)
            {
                Logger.Debug("Skipping SelectAndScrollToLevel() because we detected Twitch Integrtion Plugin has a Queue active...");
                return;
            }

            // try to find the index and scroll to it
            int selectedIndex = 0;
            List<IPreviewBeatmapLevel> levels = table.GetPrivateField<IBeatmapLevelPack>("_pack").beatmapLevelCollection.beatmapLevels.ToList();
            selectedIndex = levels.FindIndex(x => x.levelID == levelID);
            if (selectedIndex >= 0)
            {
                Logger.Debug("Scrolling to idx: {0}", selectedIndex);

                TableView listTableView = _levelPackLevelsViewController
                    .GetPrivateField<LevelPackLevelsTableView>("_levelPackLevelsTableView")
                    .GetPrivateField<TableView>("_tableView");

                var scrollPosType = TableView.ScrollPositionType.Center;
                if (selectedIndex == 0)
                {
                    scrollPosType = TableView.ScrollPositionType.Beginning;
                }
                if (selectedIndex == _model.SortedSongList.Count-1)
                {
                    scrollPosType = TableView.ScrollPositionType.End;
                }

                listTableView.ScrollToCellWithIdx(selectedIndex, scrollPosType, true);
                RefreshQuickScrollButtons();

                _lastRow = selectedIndex;
            }
            else
            {
                Logger.Debug("Song is not in the level pack, cannot scroll to it...");
            }
        }

        /// <summary>
        /// Helper for updating the model (which updates the song list)
        /// </summary>
        public void UpdateLevelDataModel()
        {
            try
            {
                Logger.Trace("UpdateLevelDataModel()");

                if (_model.CurrentLevelPack == null && _levelPackViewController != null)
                {
                    // TODO - is this acceptable?  review....
                    Logger.Debug("No level pack selected, acquiring the first available...");
                    var levelPackCollection = _levelPackViewController.GetPrivateField<IBeatmapLevelPackCollection>("_levelPackCollection");
                    this._model.SetCurrentLevelPack(levelPackCollection.beatmapLevelPacks[0]);
                }

                _model.UpdateLevelRecords();
            }
            catch (Exception e)
            {
                Logger.Exception("SongBrowser UI crashed trying to update the internal song lists: ", e);
            }
        }

        //Pull njs from a difficulty, based on private function from SongLoader
        public float GetNoteJump(string json)
        {
            float noteJumpSpeed = 0;
            var split = json.Split(':');
            for (var i = 0; i < split.Length; i++)
            {
                if (split[i].Contains("_noteJumpSpeed"))
                {
                    noteJumpSpeed = Convert.ToSingle(split[i + 1].Split(',')[0], CultureInfo.InvariantCulture);
                }
            }

            return noteJumpSpeed;
        }

#if DEBUG
        /// <summary>
        /// Not normally called by the game-engine.  Dependent on SongBrowserApplication to call it.
        /// </summary>
        public void LateUpdate()
        {
            CheckDebugUserInput();
        }

        /// <summary>
        /// Map some key presses directly to UI interactions to make testing easier.
        /// </summary>
        private void CheckDebugUserInput()
        {
            try
            {
                if (this._levelPackLevelsViewController != null && this._levelPackLevelsViewController.isActiveAndEnabled)
                {
                    bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                    if (isShiftKeyDown && Input.GetKeyDown(KeyCode.X))
                    {
                        this._beatmapCharacteristicSelectionViewController.HandleDifficultySegmentedControlDidSelectCell(null, 1);
                    }
                    else if (Input.GetKeyDown(KeyCode.X))
                    {
                        this._beatmapCharacteristicSelectionViewController.HandleDifficultySegmentedControlDidSelectCell(null, 0);
                    }

                    // back
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        this._levelSelectionNavigationController.GoBackButtonPressed();
                    }
                    
                    // select current sort mode again (toggle inverting)
                    if (isShiftKeyDown && Input.GetKeyDown(KeyCode.BackQuote))
                    {
                        _sortButtonGroup[_sortButtonLastPushedIndex].Button.onClick.Invoke();
                    }
                    // cycle sort modes
                    else if (Input.GetKeyDown(KeyCode.BackQuote))
                    {
                        _sortButtonLastPushedIndex = (_sortButtonLastPushedIndex + 1) % _sortButtonGroup.Count;
                        _sortButtonGroup[_sortButtonLastPushedIndex].Button.onClick.Invoke();
                    }

                    // filter favorites
                    if (Input.GetKeyDown(KeyCode.F1))
                    {
                        OnFavoriteFilterButtonClickEvent();
                    }

                    // filter playlists
                    if (Input.GetKeyDown(KeyCode.F2))
                    {
                        OnPlaylistButtonClickEvent();
                    }

                    // filter search
                    if (Input.GetKeyDown(KeyCode.F3))
                    {
                        OnSearchButtonClickEvent();
                    }

                    // delete
                    if (Input.GetKeyDown(KeyCode.Delete))
                    {
                        _deleteButton.onClick.Invoke();
                    }

                    // c - select difficulty for top song
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        _levelPacksTableView.SelectCellWithIdx(5);
                        _levelPacksTableView.HandleDidSelectColumnEvent(null, 2);

                        TableView listTableView = this._levelPackLevelsTableView.GetPrivateField<TableView>("_tableView");
                        this._levelPackLevelsTableView.HandleDidSelectRowEvent(listTableView, 2);
                        listTableView.ScrollToCellWithIdx(2, TableView.ScrollPositionType.Beginning, false);

                        //this._levelDifficultyViewController.HandleDifficultySegmentedControlDidSelectCell(null, 0);
                    }

                    // v - select difficulty for top song
                    if (Input.GetKeyDown(KeyCode.V))
                    {
                        this.SelectAndScrollToLevel(_levelPackLevelsTableView, _model.SortedSongList[0].levelID);
                        this._levelDifficultyViewController.HandleDifficultySegmentedControlDidSelectCell(null, 0);
                    }

                    // return - start a song
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        if (_playButton.isActiveAndEnabled)
                        {
                            _playButton.onClick.Invoke();
                        }
                    }

                    // change song index
                    if (isShiftKeyDown && Input.GetKeyDown(KeyCode.N))
                    {
                        _pageUpFastButton.onClick.Invoke();
                    }
                    else if (Input.GetKeyDown(KeyCode.N))
                    {
                        _lastRow = (_lastRow - 1) != -1 ? (_lastRow - 1) % this._model.SortedSongList.Count : 0;
                        this.SelectAndScrollToLevel(_levelPackLevelsTableView, _model.SortedSongList[_lastRow].levelID);
                    }

                    if (isShiftKeyDown && Input.GetKeyDown(KeyCode.M))
                    {
                        _pageDownFastButton.onClick.Invoke();
                    }
                    else if (Input.GetKeyDown(KeyCode.M))
                    {                        
                        _lastRow = (_lastRow + 1) % this._model.SortedSongList.Count;
                        this.SelectAndScrollToLevel(_levelPackLevelsTableView, _model.SortedSongList[_lastRow].levelID);
                    }

                    // add to favorites
                    if (Input.GetKeyDown(KeyCode.KeypadPlus))
                    {
                        ToggleSongInPlaylist();
                    }
                }
                else if (_deleteDialog != null && _deleteDialog.isInViewControllerHierarchy)
                {
                    // accept delete
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        _deleteDialog.GetPrivateField<Button>("_okButton").onClick.Invoke();
                    }

                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        _deleteDialog.GetPrivateField<Button>("_cancelButton").onClick.Invoke();
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.B))
                    {
                        Logger.Debug("Invoking OK Button");
                        VRUIViewController view = Resources.FindObjectsOfTypeAll<VRUIViewController>().First(x => x.name == "StandardLevelResultsViewController");
                        view.GetComponentsInChildren<Button>().First(x => x.name == "Ok").onClick.Invoke();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Exception("Debug Input caused Exception: ", e);
            }
        }
#endif
    }
}
 