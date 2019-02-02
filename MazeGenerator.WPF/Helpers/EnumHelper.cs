using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MazeGenerator.WPF.Helpers
{
    public static class EnumHelper
    {
        public static string Description(this Enum eValue)
        {
            var nAttributes = eValue.GetType().GetField(eValue.ToString()).GetCustomAttributes(typeof(LabelNameAttributeBase), false);
            if (nAttributes.Any())
                return (nAttributes.First() as LabelNameAttributeBase)?.GetProperText();

            // If no description is found, the least we can do is replace underscores with spaces
            // You can add your own custom default formatting logic here
            TextInfo oTI = CultureInfo.CurrentCulture.TextInfo;
            return oTI.ToTitleCase(oTI.ToLower(eValue.ToString().Replace("_", " ")));
        }

        public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions(Type t, bool skipDefaultValue = false)
        {
            if (!t.IsEnum)
                throw new ArgumentException("t must be an enum type");

            return Enum.GetValues(t).Cast<Enum>().Skip(skipDefaultValue ? 1 : 0).Select((e) => new ValueDescription() { Value = e, Description = e.Description() }).ToList();
        }
    }
}