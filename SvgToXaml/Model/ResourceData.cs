using SvgToXaml.CommandServices;
using System.Windows.Media;

namespace SvgToXaml.Model
{
    public class ResourceDictionaryFile : BindableBase
    {
        public string? XamlFileName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string? XamlFilePath
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }        
    }
    public class ResourceDictionaryData : BindableBase
    {
        public ResourceDictionaryData() { }
        
        public int Number
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }

        public string? Xaml
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public ImageSource? ResourceImage
        {
            get { return GetValue<ImageSource>(); }
            set { SetValue(value); }
        }
    }
}
