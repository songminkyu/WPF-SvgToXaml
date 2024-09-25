using CommunityToolkit.Mvvm.Input;
using SvgToXaml.Model;
using SvgToXaml.SvgControlService;
using Microsoft.Win32;
using SvgToXaml.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SvgToXaml.ViewModel
{
    public class SvgToXamlViewModel : ViewModelBase
    {
        public SvgToXamlViewModel() 
        {
            RscDicFileList                = new ObservableCollection<ResourceDictionaryFile>();
            ResourceList                  = new ObservableCollection<ResourceDictionaryData>();
            svgInfoDatas                  = new List<SvgInfoData>();
            OpenFileCommand               = new RelayCommand<object>(OpenFileCommandExe);
            SVGFileDropCommand            = new RelayCommand<object>(SVGFileDropCommandExe);
            DecreaseFileIndexCommand      = new RelayCommand<object>(DecreaseFileIndexCommandExe);
            IncreaseFileIndexCommand      = new RelayCommand<object>(IncreaseFileIndexCommandExe);
            ConvertRscDicCommand          = new RelayCommand<object>(ConvertRscDicCommandExe);
            RscDicOpenFileCommand         = new RelayCommand<object>(RscDicOpenFileCommandExe);
            SelectionChangedRscDicCommand = new RelayCommand<object>(SelectionChangedRscDicCommandExe);

            SelectedRscDicFile            = new ResourceDictionaryFile();
            viewSvg                       = new BitmapImage();
            toXaml                        = "";
            CurrentFileIndexView          = 0;
            TotalSvgCount                 = 0;
            ConvertRscDicView             = false;
            SvgPreviewVisible             = true;
            RscDicPreviewVisible          = false;
        }

        private void SelectionChangedRscDicCommandExe(object? obj)
        {            
            if (ResourceList != null && ResourceList.Count > 0)
            {
                ResourceList.Clear();
            }

            if(ResourceList != null && SelectedRscDicFile != null)
            {
                if(File.Exists(SelectedRscDicFile.XamlFilePath))
                {
                    svgInfoDatas = XamlToImageConverter.ConvertXamlToImage(SelectedRscDicFile.XamlFilePath);
                    int i = 1;
                    foreach (var resources in svgInfoDatas!)
                    {
                        ResourceList.Add(new ResourceDictionaryData() { Number = i, ResourceImage = resources.viewSvg, Xaml = resources.toXaml });
                        i++;
                    }
                }                
            }           
        }

        private void ConvertRscDicCommandExe(object? obj)
        {
            SvgPreviewVisible = !ConvertRscDicView;

            RscDicPreviewVisible = ConvertRscDicView;
        }

        private void IncreaseFileIndexCommandExe(object? obj)
        {
            if (svgInfoDatas == null || FileCurrentIndex + 1 >= svgInfoDatas.Count)
                return;

            FileCurrentIndex += 1;

            if (svgInfoDatas != null && svgInfoDatas.Count > FileCurrentIndex)
            {
                viewSvg = svgInfoDatas[FileCurrentIndex].viewSvg;
                toXaml  = svgInfoDatas[FileCurrentIndex].toXaml;
            }
        }

        private void DecreaseFileIndexCommandExe(object? obj)
        {
            if (FileCurrentIndex <= 0)
                return;

            FileCurrentIndex -= 1;

            if (svgInfoDatas != null && svgInfoDatas.Count > FileCurrentIndex)
            {
                viewSvg = svgInfoDatas[FileCurrentIndex].viewSvg;
                toXaml  = svgInfoDatas[FileCurrentIndex].toXaml;
            }
        }

        private void OpenFileCommandExe(object? obj)
        {          
            if (svgInfoDatas != null && svgInfoDatas.Count > 0)
            {
                svgInfoDatas.Clear();
            }

            openFileDialog = new OpenFileDialog();

            openFileDialog.Filter      = "SVG File (*.svg)|*.svg";
            openFileDialog.FilterIndex = 1;
            openFileDialog.DefaultExt  = ".svg";
            openFileDialog.Multiselect = true;

            bool result = openFileDialog.ShowDialog().GetValueOrDefault();

            if (result)
            {                
                foreach (var fileName in openFileDialog.FileNames)
                {
                    SVGData data = SVGService.GetSVGData(fileName);

                    DrawingImage svgImage = null;
                    if (data.ConvertedObject is DrawingImage image)
                    {
                        svgImage = image;
                    }
                    if (svgInfoDatas != null)
                    {
                        svgInfoDatas.Add(new SvgInfoData() { toXaml = data.XAML, viewSvg = svgImage });
                    }
                }
                if (svgInfoDatas != null && svgInfoDatas.Count > 0)
                {
                    FileCurrentIndex = 0;
                    var svginfo = svgInfoDatas.FirstOrDefault();
                    if (svginfo != null)
                    {
                        toXaml  = svginfo.toXaml;
                        viewSvg = svginfo.viewSvg;
                    }              
                    TotalSvgCount = svgInfoDatas.Count;
                }
            }
        }     
        private void RscDicOpenFileCommandExe(object? obj)
        {                        
            openFileDialog = new OpenFileDialog();

            openFileDialog.Filter      = "Xaml File (*.xaml)|*.xaml";
            openFileDialog.FilterIndex = 1;
            openFileDialog.DefaultExt  = ".xaml";
            openFileDialog.Multiselect = true;

            bool result = openFileDialog.ShowDialog().GetValueOrDefault();

            if (result)
            {                        
                if(RscDicFileList != null)
                {
                    RscDicFileList.Clear();

                    foreach (var XamlFile in openFileDialog.FileNames)
                    {
                        string FileName = Path.GetFileName(XamlFile);
                        RscDicFileList.Add(new ResourceDictionaryFile() { XamlFileName = FileName, XamlFilePath = XamlFile });
                    }

                    SelectedRscDicFile = RscDicFileList.First();
                }                                                                          
            }           
        }
        private void SVGFileDropCommandExe(object? obj)
        {
            if(svgInfoDatas != null && svgInfoDatas.Count > 0)
            {
                svgInfoDatas.Clear();
            }

            if(obj is DragEventArgs DropEvent)
            {
                foreach (string target in (string[])DropEvent.Data.GetData(DataFormats.FileDrop))
                {
                    System.IO.FileAttributes attributes = System.IO.File.GetAttributes(target);
                    if ((attributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                    {
                        //디렉토리 패스
                        continue;
                    }
                    else
                    {
                        string ext = Path.GetExtension(target);
                        if(File.Exists(target) && string.Compare(".svg", ext, true) == 0)
                        {
                            SVGData data = SVGService.GetSVGData(target);

                            DrawingImage svgImage = null;
                            if (data.ConvertedObject is DrawingImage image)
                            {
                                svgImage = image;
                            }                           
                            if(svgInfoDatas != null)
                            {
                                svgInfoDatas.Add(new SvgInfoData() { toXaml = data.XAML, viewSvg = svgImage });
                            }                            
                        }                        
                    }
                }
                if(svgInfoDatas != null && svgInfoDatas.Count > 0)
                {
                    FileCurrentIndex = 0;
                    var svginfo = svgInfoDatas.FirstOrDefault();

                    if(svginfo != null)
                    {
                        toXaml  = svginfo.toXaml;
                        viewSvg = svginfo.viewSvg;
                    }
                  
                    TotalSvgCount = svgInfoDatas.Count;
                }                
            }           
        }     
    }
}
