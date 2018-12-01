﻿using HMUI;
using IllusionPlugin;
using SimpleJSON;
using System;
using System.Text.RegularExpressions;
using UnityEngine;


namespace SongBrowserPlugin.UI
{
    class Base64Sprites
    {
        // https://www.iconfinder.com/icons/17236/folder_icon
        public static string FolderIcon = "data:image/png;base64,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";

        // https://www.iconfinder.com/icons/103857/find_glass_magnifying_search_icon
        public static string SearchIcon = "data:image/png;base64,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";

        // https://icons8.com/icon/24520/playlist
        public static string PlaylistIcon = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGAAAABgCAYAAADimHc4AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAPtSURBVHhe7dzLa9RQFAbwLgQXihvdaC2CCG5c6STjYzE28dGF+le40oKu3AndunYhPnDhWqzFRylMC8W2/4KKG10qBe0kU5Ha1nviN3EqZx7J5GZC+H7wYe3J3Nx70slkMtURIiIiIiIiIiIiIiIiIhIvF5a3yxwsMxZ6br15vlrFX4dPm3SZgmXGQt/dDnx3K/Ccl8HFygl8e3i0SZcpWGZMDkArgeduBr7z7KdfOYpy/rRJlylYZqz9ALQdiF/mWfEgvHzyIDYjW7QD0Ip5Nqyb3Fvznf3YnLKmNV7J99B37n6t1fbiYZQVpdmd47nfzOnp9tbEsd14OA1KbXSPmIPwJfSc69u12i4MQ2lpDU6QDxiG0lKamigYhtLSmpokGIbS0pqaJBiG0tKamiQYhtLSmhrFc++YrKq1tmCY7Ghv38sULDOmNVUitdUJd595JzxlLjnXtG0k0SBZ0iZdpmCZMa2pEpQjjdqpA3JLQm5NdNsuE9qkyxQsM/Z/Q1tBeQe5ORf6zn25Wddtu4Foky5TsMxYe9Pbg7JqffzMEfNseGqygW9RWlrzJSh3tTZeOY4vKS2t+RKUyTat+RKUyTat+RKUyTat+RKUyTat+RKUyTat+RKUyTat+RKUyTat+RKUyTat+RKU86e9fS9TsMyY1nwJyvnTJl2mYJkxrfkSlPOnTbpMwTJjWvMlKOdPm3SZgmXGtOZLUCbbtOZLUCbbtOZLUCbbtOZLUCbbtOZLUKY0mjX3cOC5kyazoee+N3+GEnw9a3JTtpFtteZLooEomeaFs4cC33kin9VqTW2PbBP47mOtJsGQ1K/mePVa6DuB1sw0wbDUj8Cr3jKnlU2tkWmDoakX+cnPuvkSDE/dNL3To+a009AaOGiwi/xpb9+LkOmFpebc3NweTDMiL7ha87IIdpE/bfGFyPzyc0wxsn6pMhZ4zm+teVkEu8mfuvgCZKb+bhJTjJjz/qTWuKyC3eRPW3wRMrOwcg5TjJjTzxutcVkFu6FOzLvaj1rjsgp2Q53YuvppBbuhTuweAKeB3VAnVk9BZmzshjqx+SJsLm9fYzfUic3L0IZfuYHdUCfRG7E+bjsnjYzZ+qyAeuh2Pz9tzDPrEYanXuQDmG7/hjdxPPcH/5uyhP7ejh78npCM0fQqVzHs8Gi3AYqU6fnlKUw1NugHMvJYGQPDDZe26CLFHICNF/UVF9ONRR9JpjkdRY9xrmCY4dMWXbwsfZ5eXBzDlGNy/pYX5n6ujmQbc9p5WLhzvr7g4mV6funTq/rKKKa9g1xGyrW8ORhv8aso/34txXxParzUJCIiIiIiIiIiIiIiIsrOyMgfUAEg+sZSgwsAAAAASUVORK5CYII=";

        // https://github.com/andruzzzhka/BeatSaverDownloader/blob/master/BeatSaverDownloader/Misc/Base64Sprites.cs
        public static string AddToFavoritesIcon = "data:image/png;base64,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";
        public static string RemoveFromFavoritesIcon = "data:image/png;base64,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";

        //https://www.flaticon.com/free-icon/download_724933
        public static string DownloadIconB64 = "iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAYAAADDPmHLAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAACXBIWXMAAA3WAAAN1gGQb3mcAAAAB3RJTUUH4gscDB0RZoF8lQAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMS4xYyqcSwAABOpJREFUeF7t3D+OVUcQxeFBIiQgQ56EjIRdsA4WwQKQTErmnMgJXgCyvArkzIEzC3mwHDmy5Ghc/e4vwJKbmal5Z+a8vueTOuup6luHf90BZyu6vLw8r/W+1kWt2xo1Rq1zyoezEVStP2sd26iZXwTuKqTxu1XlPW3CVYV0jD/2Zy5oE64ISoY24YqcZGgTrshJhjbhipxkaBOuyEmGNuGKnGRoE67ISYY24YqcZGgTrshJhjbhipxkaBOuyEmGNuGKnGRoE67ISYY24YqcZGgTrshJhjbhipxkaBOuyEmGNuGKnGRoE67ISYY24YqcZGgTrshJhjbhipxkaBOuyEmGNuGKnGRoE67ISYY24YqcZGgTrshJhjbhipxkaBOuyEmGNuGKnGRoE67ISYY24YqcZGgTrshJhjbhipxkaBOuyEmGNuGKnGRoE67ISYY24YqcZGgTrshJhjbhipxkaBOuyEmGNuGKnGRoE67ISYY24YqcZGgTrshJhjbhipxkaBOuyEmGNuGKnGRoE67ISYY24YqcZGgTrshJhjbhipxkaBPHVHN9Wuu7Wr+OIS9ufOP41qd8/r7VIB7X+qXW3oxvfswY9quG8O4wjn16xxj2q4bwcZvFLn1kDPtVQ/i8zWKXPjOG/aohvN1msUtvGcN+1RCe1PrrMI59Gd/8hDHsWw3iRa1/xlR2YnzrCz4/hhrIy8No9uElnx1fqsG83uaztNd8bvyfGtDK7wK591+lhvSw1k+Hca1lfNNDPjO+pgb1qNbPY2qLGN/yiM+L66iBndf6bUzvxI1vOOez4iZqcM9rnfIbwTj7cz4nOmqAp/pGkLv+sdQgT/GNIHf9Y6qBntIbQe76CjXYU3gjyF1fpYbr/kaQu75aDdj1jSB3/btSg3Z7I8hd/67VwF3eCHLXvy81+Pt+I8hd/75VAPf5RpC7voMK4j7eCHLXd1KB3OUbQe76biqUu3ojyF3fVQWjfiPIXd9dBaR6I8hd/1RUUMd+I8hd/9RUYMd6I9jXXX/75j7KWKjjHOONwOquz5naKDPHvjbK2Kgj3eaNwO6uz7naKDPHvjbKWKljdd4ILO/6nK2NMnPsa6OMlTrWTd8IbO/62/H6KDPHvjbK2KmjXfeNwPquvx2xjzJz7GujjKU63lVvBPZ3/e2YfZSZY18bZWzVEccbwR+Hw/7X+J9K7O/621H7KDPHvjbKWKtjflPrh1q/1/pU6/taJ/GfNdQ5b4Uyc+xro0yIMOY2ysyxr40yIcKY2ygzx742yoQIY26jzBz72igTIoy5jTJz7GujTIgw5jbKzLGvjTIhwpjbKDPHvjbKhAhjbqPMHPvaKBMijLmNMnPsa6NMiDDmNsrMsa+NMiHCmNsoM8e+NsqECGNuo8wc+9ooEyKMuY0yc+xro0yIMOY2ysyxr40yIcKY2ygzx742yoQIY26jzBz72igTIoy5jTJz7ItFEfMc+2JRxDzHvlgUMc+xLxZFzHPsi0UR8xz7YlHEPMe+WBQxz7EvFkXMc7XnYtsaC7og5rna9GHbGwv6QMxztenbbW8s6A0xz9WmZ7X+PmyPlYxMnxHz19XGV4cfiZW8It6r1eYHtX48/FisYGT5gHivZ/xArfEnQf46OF0ju5HhzcL/Uv3w+DfBm1rjdpAror+R0chqZHbF3/lnZ/8CEi2he6BAVgUAAAAASUVORK5CYII=";


        public static Sprite Base64ToSprite(string base64)
        {
            // prune base64 encoded image header
            Regex r = new Regex(@"data:image.*base64,");
            base64 = r.Replace(base64, "");            

            Sprite s = null;
            try
            {
                Texture2D tex = Base64ToTexture2D(base64);
                s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), (Vector2.one / 2f));
            }
            catch (Exception)
            {
                Console.WriteLine("Exception loading texture from base64 data.");
                s = null;
            }

            return s;
        }

        public static Texture2D Base64ToTexture2D(string encodedData)
        {
            byte[] imageData = Convert.FromBase64String(encodedData);

            int width, height;
            GetImageSize(imageData, out width, out height);

            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Trilinear;
            texture.LoadImage(imageData);
            return texture;
        }

        private static void GetImageSize(byte[] imageData, out int width, out int height)
        {
            width = ReadInt(imageData, 3 + 15);
            height = ReadInt(imageData, 3 + 15 + 2 + 2);
        }

        private static int ReadInt(byte[] imageData, int offset)
        {
            return (imageData[offset] << 8) | imageData[offset + 1];
        }
    }
}
