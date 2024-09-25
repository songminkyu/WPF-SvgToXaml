using SvgToXaml.Model;
using System.IO;
using System.Windows;

namespace SvgToXaml.SvgControlService
{
    /// <summary>
    /// SVG 데이터
    /// </summary>
    public class SVGData
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Private

        #region Field

        /// <summary>
        /// 파일 경로
        /// </summary>
        private string filePath;

        /// <summary>
        /// 변환 객체
        /// </summary>
        private DependencyObject convertedObject;

        /// <summary>
        /// 객체명
        /// </summary>
        private string objectName;

        /// <summary>
        /// SVG 데이터
        /// </summary>
        private string svg;

        /// <summary>
        /// XAML 데이터
        /// </summary>
        private string xaml;

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Property
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region 파일 경로 - FilePath

        /// <summary>
        /// 파일 경로
        /// </summary>
        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
            }
        }

        #endregion
        #region 변환 객체 - ConvertedObject

        /// <summary>
        /// 변환 객체
        /// </summary>
        public DependencyObject ConvertedObject
        {
            get
            {
                if (convertedObject == null)
                {
                    convertedObject = SVGService.GetObject
                    (
                        filePath,
                        ConversionMode.DrawingImage,
                        null,
                        out objectName,
                        new ResourceKeyInfo()
                    ) as DependencyObject;
                }

                return convertedObject;
            }
            set
            {
                convertedObject = value;
            }
        }

        #endregion
        #region SVG - SVG

        /// <summary>
        /// SVG
        /// </summary>
        public string SVG
        {
            get
            {
                return svg ?? (svg = File.ReadAllText(filePath));
            }
            set
            {
                svg = value;
            }
        }

        #endregion
        #region XAML - XAML

        /// <summary>
        /// XAML
        /// </summary>
        public string XAML
        {
            get
            {
                return xaml ?? (xaml = SVGService.GetXAML(ConvertedObject, false, objectName, false));
            }
            set
            {
                xaml = value;
            }
        }

        #endregion
    }
}