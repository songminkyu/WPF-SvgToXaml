using SvgToXaml.CommandServices;
using System.Windows.Media;

namespace SvgToXaml.Model
{
    public class SvgInfoData : BindableBase
    {   
        public string? resourceKey
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
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
        public double width
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }
        public double height
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }        
    }
}
