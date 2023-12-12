#if UNITY_EDITOR


namespace MetaTable
{
    public class OverviewUtility
    {
        public static bool ExistsOverviewUuid<T>(string uuid, string packageDir = null) where T : MetaTableOverview
        {
            var overviews = AssetDatabaseUtility.FindAsset<T>(packageDir);
            foreach (var overview in overviews)
            {
                foreach (var row in overview.BaseRows)
                {
                    if (row.Uuid == uuid)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool ExistsOverviewName<T>(string name, string packageDir = null) where T : MetaTableOverview
        {
            var overviews = AssetDatabaseUtility.FindAsset<T>(packageDir);
            foreach (var overview in overviews)
            {
                foreach (var row in overview.BaseRows)
                {
                    if (row.Name == name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }


}
#endif