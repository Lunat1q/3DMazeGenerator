using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
