using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Xml;
using System.Windows.Markup;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using SvgToXaml.Model;

namespace SvgToXaml.SvgControlService
{    
    public class XamlToImageConverter
    {
        public static List<SvgInfoData>? ConvertXamlToImage(string xamlFilePath)
        {
            // XAML 파일 읽기
            List<SvgInfoData> resources = new();
            string xamlContent = File.ReadAllText(xamlFilePath, System.Text.Encoding.UTF8);

            // XML 파서로 XAML 파일 분석
            XmlReader xmlReader = XmlReader.Create(new StringReader(xamlContent));

            // XAML 파싱 및 ResourceDictionary로 변환
            var resourceDictionary = (ResourceDictionary)XamlReader.Load(xmlReader);

            // 모든 DrawingImage 리소스를 순차적으로 처리
            foreach (var key in resourceDictionary.Keys)
            {
                if (resourceDictionary[key] is DrawingImage drawingImage)
                {
                    // DrawingImage에서 XAML 문자열을 추출
                    string xamlBlock = ExtractXamlBlockForKey(xamlContent, key.ToString());

                    // Width와 Height 추출
                    double width = drawingImage.Drawing.Bounds.Width;
                    double height = drawingImage.Drawing.Bounds.Height;

                    // SvgInfoData 객체 생성
                    SvgInfoData svgInfo = new SvgInfoData
                    {
                        resourceKey = key.ToString(),
                        toXaml      = xamlBlock,            // 추출한 XAML 내용
                        viewSvg     = drawingImage,         // DrawingImage를 ImageSource로 변환
                        width       = width,
                        height      = height
                    };

                    resources.Add(svgInfo);
                }
            }

            return resources;
        }

        // 특정 리소스 키에 대한 XAML 블록을 추출하는 함수
        public static string ExtractXamlBlockForKey(string xamlContent, string resourceKey)
        {
            // 리소스 키로 시작하는 XAML 블록을 찾는 정규식
            string pattern = $@"<DrawingImage\s+x:Key=""{resourceKey}""[\s\S]*?</DrawingImage>";

            Match match = Regex.Match(xamlContent, pattern);

            // XAML 블록이 발견되면 반환, 없으면 빈 문자열 반환
            return match.Success ? match.Value : string.Empty;
        }

        private static void SaveDrawingImageAsPng(DrawingImage drawingImage, string filePath)
        {
            // DrawingImage를 Bitmap으로 변환
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(drawingImage, new Rect(0, 0, drawingImage.Width, drawingImage.Height));
            }

            // Bitmap 생성
            var renderTargetBitmap = new RenderTargetBitmap((int)drawingImage.Width, (int)drawingImage.Height, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(drawingVisual);

            // PNG로 저장
            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                pngEncoder.Save(fileStream);
            }
        }
    }
}
