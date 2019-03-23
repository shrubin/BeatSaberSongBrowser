using SongLoaderPlugin.OverrideClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongBrowserPlugin.UI.Playlists
{
    public class CustomLevelPack : CustomBeatmapLevelPackSO
    {
        public static CustomLevelPack GetPlaylistPack(CustomLevelCollectionSO beatmapLevelCollectionSO, string packID, string packName, byte[] packImage)
        {
            var newPack = CreateInstance<CustomLevelPack>();
            newPack.InitPublic(beatmapLevelCollectionSO, packID, packName, packImage);
            return newPack;
        }

        public void InitPublic(CustomLevelCollectionSO beatmapLevelCollectionSO, string packID, string packName, byte[] packImage)
        {
            _isPackAlwaysOwned = true;
            _packID = packID;
            _packName = packName;
            _coverImage = CustomUI.Utilities.UIUtilities.LoadSpriteRaw(packImage);
            _beatmapLevelCollection = beatmapLevelCollectionSO;
        }
    }
}