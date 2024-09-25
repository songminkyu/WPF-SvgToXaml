using SvgToXaml.CommandServices;
using System.Windows.Media;

namespace SvgToXaml.Model
{
    public class SvgInfoData : BindableBase
    {   
        public string? toXaml
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public ImageSource? viewSvg
        {
            get { return GetValue<ImageSource>(); }
            set { SetValue(value); }
        }
    }
}
