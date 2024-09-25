using CommunityToolkit.Mvvm.Input;
using SvgToXaml.CommandServices;
using SvgToXaml.Model;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace SvgToXaml.View
{
    public class ViewModelBase : BindableBase
    {   
        protected List<SvgInfoData> ?svgInfoDatas { get; set; }
        protected OpenFileDialog ?openFileDialog;

        private ObservableCollection<ResourceDictionaryData> _ResourceList { get; set; }
        public ObservableCollection<ResourceDictionaryData> ResourceList
        {
            get { return _ResourceList; }
            set
            {
                if (_ResourceList == value) return;
                _ResourceList = value;
                OnPropertyChanged("ResourceList");
            }
        }
        private ObservableCollection<ResourceDictionaryFile> _RscDicFileList { get; set; }
        public ObservableCollection<ResourceDictionaryFile> RscDicFileList
        {
            get { return _RscDicFileList; }
            set
            {
                if (_RscDicFileList == value) return;
                _RscDicFileList = value;
                OnPropertyChanged("RscDicFileList");
            }
        }
        private ResourceDictionaryFile _SelectedRscDicFile { get; set; }
        public ResourceDictionaryFile SelectedRscDicFile
        {
            get { return _SelectedRscDicFile; }
            set
            {
                if (_SelectedRscDicFile == value) return;
                _SelectedRscDicFile = value;
                OnPropertyChanged("SelectedRscDicFile");
            }
        }
        
        private string? _toXaml { get; set; }
        public string? toXaml
        {
            get { return _toXaml; }
            set
            {
                if (_toXaml == value) return;
                _toXaml = value;
                OnPropertyChanged("toXaml");
            }
        }

        private ImageSource? _viewSvg { get; set; }
        public ImageSource? viewSvg
        {
            get { return _viewSvg; }
            set
            {
                if (_viewSvg == value) return;
                _viewSvg = value;
                OnPropertyChanged("viewSvg");
            }
        }
        private int _CurrentFileIndexView { get; set; }
        public int CurrentFileIndexView
        {
            get { return _CurrentFileIndexView; }
            set
            {
                if (_CurrentFileIndexView == value) return;
                _CurrentFileIndexView = value;
                OnPropertyChanged("CurrentFileIndexView");
            }
        }

        private int _TotalSvgCount { get; set; }
        public int TotalSvgCount
        {
            get { return _TotalSvgCount; }
            set
            {
                if (_TotalSvgCount == value) return;
                _TotalSvgCount = value;
                OnPropertyChanged("TotalSvgCount");
            }
        }
        
        private bool _IsIndexDecreaserEnabled { get; set; }
        public bool IsIndexDecreaserEnabled
        {
            get { return _IsIndexDecreaserEnabled; }
            set
            {
                if (_IsIndexDecreaserEnabled == value) return;
                _IsIndexDecreaserEnabled = value;
                OnPropertyChanged("IsIndexDecreaserEnabled");
            }
        }
        private bool _IsIndexIncreaserEnabled { get; set; }
        public bool IsIndexIncreaserEnabled
        {
            get { return _IsIndexIncreaserEnabled; }
            set
            {
                if (_IsIndexIncreaserEnabled == value) return;
                _IsIndexIncreaserEnabled = value;
                OnPropertyChanged("IsIndexIncreaserEnabled");
            }
        }
        private bool _ConvertRscDicView { get; set; }
        public bool ConvertRscDicView
        {
            get { return _ConvertRscDicView; }
            set
            {
                if (_ConvertRscDicView == value) return;
                _ConvertRscDicView = value;
                OnPropertyChanged("ConvertRscDicView");
            }
        }
        private bool _SvgPreviewVisible { get; set; }
        public bool SvgPreviewVisible
        {
            get { return _SvgPreviewVisible; }
            set
            {
                if (_SvgPreviewVisible == value) return;
                _SvgPreviewVisible = value;
                OnPropertyChanged("SvgPreviewVisible");
            }
        }
        private bool _RscDicPreviewVisible { get; set; }
        public bool RscDicPreviewVisible
        {
            get { return _RscDicPreviewVisible; }
            set
            {
                if (_RscDicPreviewVisible == value) return;
                _RscDicPreviewVisible = value;
                OnPropertyChanged("RscDicPreviewVisible");
            }
        }
        private IRelayCommand<object>? _SelectionChangedRscDicCommand { get; set; }
        public IRelayCommand<object>? SelectionChangedRscDicCommand
        {
            get { return _SelectionChangedRscDicCommand; }
            set
            {
                if (_SelectionChangedRscDicCommand == value) return;
                _SelectionChangedRscDicCommand = value;
            }
        }
        
        private IRelayCommand<object>? _ConvertRscDicCommand { get; set; }
        public IRelayCommand<object>? ConvertRscDicCommand
        {
            get { return _ConvertRscDicCommand; }
            set
            {
                if (_ConvertRscDicCommand == value) return;
                _ConvertRscDicCommand = value;
            }
        }
        private IRelayCommand<object>? _RscDicOpenFileCommand { get; set; }
        public IRelayCommand<object>? RscDicOpenFileCommand
        {
            get { return _RscDicOpenFileCommand; }
            set
            {
                if (_RscDicOpenFileCommand == value) return;
                _RscDicOpenFileCommand = value;
            }
        }
        
        private IRelayCommand<object>? _OpenFileCommand { get; set; }
        public IRelayCommand<object>? OpenFileCommand
        {
            get { return _OpenFileCommand; }
            set
            {
                if (_OpenFileCommand == value) return;
                _OpenFileCommand = value;
            }
        }

        private IRelayCommand<object>? _SVGFileDropCommand { get; set; }
        public IRelayCommand<object>? SVGFileDropCommand
        {
            get { return _SVGFileDropCommand; }
            set
            {
                if (_SVGFileDropCommand == value) return;
                _SVGFileDropCommand = value;
            }
        }
        private IRelayCommand<object>? _DecreaseFileIndexCommand { get; set; }
        public IRelayCommand<object>? DecreaseFileIndexCommand
        {
            get { return _DecreaseFileIndexCommand; }
            set
            {
                if (_DecreaseFileIndexCommand == value) return;
                _DecreaseFileIndexCommand = value;
            }
        }
        private IRelayCommand<object>? _IncreaseFileIndexCommand { get; set; }
        public IRelayCommand<object>? IncreaseFileIndexCommand
        {
            get { return _IncreaseFileIndexCommand; }
            set
            {
                if (_IncreaseFileIndexCommand == value) return;
                _IncreaseFileIndexCommand = value;
            }
        }

        private int _FileCurrentIndex = -1;
        public int FileCurrentIndex
        {
            get { return _FileCurrentIndex; }
            set
            {
                _FileCurrentIndex = value;

                CurrentFileIndexView = (FileCurrentIndex + 1);

                if (FileCurrentIndex <= 0)
                {
                    IsIndexDecreaserEnabled = false;
                }
                else
                {
                    IsIndexDecreaserEnabled = true;
                }

                if (FileCurrentIndex + 1 >= svgInfoDatas.Count)
                {
                    IsIndexIncreaserEnabled = false;
                }
                else
                {
                    IsIndexIncreaserEnabled = true;
                }
            }
        }     
    }
}
