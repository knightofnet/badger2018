using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace Badger2018.utils
{
    class ImagesBooleanWrapper
    {

        public ImageSource ImageTrue { get; private set; }
        public ImageSource ImageFalse { get; private set; }
        public Image ImageTarget { get; private set; }
        public bool Value { get; private set; }

        private string TooltipValueTrue { get; set; }
        private string TooltipValueFalse { get; set; }

        public ImagesBooleanWrapper(ImageSource imgTrue, ImageSource imgFalse, Image imgTarget)
        {
            ImageTrue = imgTrue;
            ImageFalse = imgFalse;

            ImageTarget = imgTarget;

            Value = false;
        }

        public void ChangeValue(bool b)
        {
            ImageTarget.Source = b ? ImageTrue : ImageFalse;
            ImageTarget.ToolTip = b ? TooltipValueTrue : TooltipValueFalse;
            Value = b;
        }

        public void SetTooltipTextValues(string strTooltipValueTrue, string strTooltipValueFalse)
        {
            TooltipValueTrue = strTooltipValueTrue;
            TooltipValueFalse = strTooltipValueFalse;
        }
    }
}
