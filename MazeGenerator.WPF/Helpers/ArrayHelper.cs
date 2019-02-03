using System;

namespace MazeGenerator.WPF.Helpers
{
    internal static class ArrayHelper
    {
        public static bool IndexesAreInRange(this Array array, params int[] indexes)
        {
            var result = true;
            for (var i = 0; i < array.Rank && result; i++)
            {
                result = indexes[i] >= 0 && indexes[i] < array.GetLength(i);
            }
            return result;
        }
    }
}
