using System;

namespace Gameplay.Campaign.Data
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EncounterNodeAttribute : Attribute
    {
        public string MenuPath { get; }
        public string HexColor { get; }
        public string FieldName { get; }
        public Type ConfigType { get; }

        public EncounterNodeAttribute(string menuPath, string hexColor, string fieldName, Type configType)
        {
            MenuPath = menuPath;
            HexColor = hexColor;
            FieldName = fieldName;
            ConfigType = configType;
        }
    }
}