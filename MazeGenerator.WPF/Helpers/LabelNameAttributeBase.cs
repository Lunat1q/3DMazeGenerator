using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGenerator.WPF.Helpers
{
    public abstract class LabelNameAttributeBase : Attribute
    {
        public abstract string GetProperText();

        protected LabelNameAttributeBase(string label)
        {
            Label = label;
        }

        public string Label { get; }
    }
}
