using System.Collections.Generic;
using System.Linq;

namespace ToX_Free_Utility_NEW_ERA.Tweaks
{
    public static class TweakCatalog
    {
        public static Dictionary<TweakConfigType, Dictionary<string, EmbeddedTweak>> Load()
        {
            var result = new Dictionary<TweakConfigType, Dictionary<string, EmbeddedTweak>>();

            result[TweakConfigType.WindowsTweaks] = ToMap(WindowsTweaks.All);
            result[TweakConfigType.Peripherals] = ToMap(PeripheralsTweaks.All);
            result[TweakConfigType.GraphicTweaks] = ToMap(GraphicTweaks.All);
            result[TweakConfigType.Debloat] = ToMap(DebloatTweaks.All);
            result[TweakConfigType.GameBoosters] = ToMap(GameBoosterTweaks.All);

            return result;
        }

        public static int GetTotalTweakCount()
        {
            int total = 0;

            foreach (Dictionary<string, EmbeddedTweak> map in Load().Values)
            {
                if (map != null)
                    total += map.Count;
            }

            return total;
        }

        public static string[] GetCategoryKeys(TweakConfigType type)
        {
            switch (type)
            {
                case TweakConfigType.WindowsTweaks:
                    return ToKeys(WindowsTweaks.All);
                case TweakConfigType.Peripherals:
                    return ToKeys(PeripheralsTweaks.All);
                case TweakConfigType.GraphicTweaks:
                    return ToKeys(GraphicTweaks.All);
                case TweakConfigType.Debloat:
                    return ToKeys(DebloatTweaks.All);
                case TweakConfigType.GameBoosters:
                    return ToKeys(GameBoosterTweaks.All);
                default:
                    return new string[0];
            }
        }

        private static string[] ToKeys(List<EmbeddedTweak> tweaks)
        {
            if (tweaks == null)
                return new string[0];

            return tweaks
                .Where(t => t != null && !string.IsNullOrWhiteSpace(t.Key))
                .Select(t => t.Key)
                .ToArray();
        }

        private static Dictionary<string, EmbeddedTweak> ToMap(List<EmbeddedTweak> tweaks)
        {
            var map = new Dictionary<string, EmbeddedTweak>(System.StringComparer.OrdinalIgnoreCase);

            foreach (EmbeddedTweak tweak in tweaks)
            {
                if (tweak == null || string.IsNullOrWhiteSpace(tweak.Key))
                    continue;

                map[tweak.Key] = tweak;
            }

            return map;
        }
    }
}