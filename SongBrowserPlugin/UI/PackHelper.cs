using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SongLoaderPlugin;
using SongLoaderPlugin.OverrideClasses;
using Logger = SongBrowserPlugin.Logging.Logger;

namespace SongBrowserPlugin.UI
{
    public static class PackHelper
    {

        public static void CreatePack(string packName, Sprite packImage, IEnumerable<CustomLevel> customLevels)
        {
            string packId = packName.ToLower();
            Logger.Info("Loading pack {0}", packName);

            var levelCollectionSO = Resources.FindObjectsOfTypeAll<BeatmapLevelCollectionSO>().FirstOrDefault();
            var customLevelCollectionSO = SongLoaderPlugin.OverrideClasses.CustomLevelCollectionSO.ReplaceOriginal(levelCollectionSO);

            if (customLevels != null)
            {
                customLevelCollectionSO.AddCustomLevels(customLevels);
            }

            var beatmapLevelPackCollectionSO = Resources.FindObjectsOfTypeAll<BeatmapLevelPackCollectionSO>().FirstOrDefault();
            var customBeatmapLevelPackCollectionSO = SongLoaderPlugin.SongLoader.CustomBeatmapLevelPackCollectionSO;
            var customBeatmapLevelPackSO = SongLoaderPlugin.OverrideClasses.CustomBeatmapLevelPackSO.GetPack(customLevelCollectionSO);

            customBeatmapLevelPackSO.SetPrivateField("_packID", packId);
            customBeatmapLevelPackSO.SetPrivateField("_packName", packName);
            customBeatmapLevelPackSO.SetPrivateField("_coverImage", packImage);
            customBeatmapLevelPackSO.SetPrivateField("_isPackAlwaysOwned", true);

            customBeatmapLevelPackCollectionSO.AddLevelPack(customBeatmapLevelPackSO);
            customBeatmapLevelPackCollectionSO.ReplaceReferences();

            var soloFreePlay = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().FirstOrDefault();
            LevelPacksViewController levelPacksViewController = (LevelPacksViewController)soloFreePlay.GetField("_levelPacksViewController");
            levelPacksViewController.SetData(customBeatmapLevelPackCollectionSO, 0);


            var additionalContentModelSO = Resources.FindObjectsOfTypeAll<AdditionalContentModelSO>().FirstOrDefault();
            HashSet<string> _alwaysOwnedBeatmapLevelPackIds = (HashSet<string>)additionalContentModelSO.GetField("_alwaysOwnedPacksIds");
            if (!_alwaysOwnedBeatmapLevelPackIds.Contains(packId))
            {
                _alwaysOwnedBeatmapLevelPackIds.Add(packId);
            }
            additionalContentModelSO.SetPrivateField("_alwaysOwnedPacksIds", _alwaysOwnedBeatmapLevelPackIds);
        }

    }
}
