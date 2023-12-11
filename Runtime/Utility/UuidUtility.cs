
using System;
using System.IO;
using System.Xml.Serialization;

namespace MetaTable
{
    public static class UuidUtility
    {

        public static string GetNewUuid()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}