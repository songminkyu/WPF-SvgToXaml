using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using SvgToXaml.Model;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace SvgToXaml.SvgControlService
{
    /// <summary>
    /// SVG 헬퍼
    /// </summary>
    public static class SVGService
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Static
        //////////////////////////////////////////////////////////////////////////////// Private

        #region Field

        /// <summary>
        /// 디폴트 XML 네임스페이스
        /// </summary>
        private static XNamespace _defaultXMLNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        /// <summary>
        /// X XML 네임스페이스
        /// </summary>
        private static XNamespace _xXMLNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        /// <summary>
        /// XML 네임스페이스 관리자
        /// </summary>
        private static XmlNamespaceManager _xmlNamespaceManager = new XmlNamespaceManager(new NameTable());

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////// Instance
        //////////////////////////////////////////////////////////////////////////////// Private

        #region Field

        /// <summary>
        /// 접두사 분리자
        /// </summary>
        private const char PREFIX_SEPARATOR = '_';

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Constructor
        ////////////////////////////////////////////////////////////////////////////////////////// Static

        #region 생성자 - SVGHelper()

        /// <summary>
        /// 생성자
        /// </summary>
        static SVGService()
        {
            _xmlNamespaceManager.AddNamespace("defns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            _xmlNamespaceManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Static
        //////////////////////////////////////////////////////////////////////////////// Public

        #region 객체 구하기 - GetObject(filePath, conversionMode, settings, name, resourceKeyInfo)

        /// <summary>
        /// 객체 구하기
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="conversionMode">변환 모드</param>
        /// <param name="settings">설정</param>
        /// <param name="name">명칭</param>
        /// <param name="resourceKeyInfo">리소스 키 정보</param>
        /// <returns>객체</returns>
        public static object GetObject
        (
            string filePath,
            ConversionMode conversionMode,
            WpfDrawingSettings settings,
            out string name,
            ResourceKeyInfo resourceKeyInfo
        )
        {
            DrawingGroup drawingGroup = GetDrawingGroup(filePath, settings);

            string elementName = Path.GetFileNameWithoutExtension(filePath);

            switch (conversionMode)
            {
                case ConversionMode.DrawingGroup:

                    name = GetDrawingGroupName(elementName, resourceKeyInfo);

                    return drawingGroup;

                case ConversionMode.DrawingImage:

                    name = GetDrawingImageName(elementName, resourceKeyInfo);

                    return GetDrawingImage(drawingGroup);

                default:

                    throw new ArgumentOutOfRangeException(nameof(conversionMode));
            }
        }

        #endregion
        #region XAML 구하기 - GetXAML(instance, includeRuntime, name, filterPixelCountPerDIP)

        /// <summary>
        /// XAML 구하기
        /// </summary>
        /// <param name="instance">인스턴스</param>
        /// <param name="includeRuntime">런타임 포함 여부</param>
        /// <param name="name">명칭</param>
        /// <param name="filterPixelCountPerDIP">DIP 당 픽셀 카운트 필터링 여부</param>
        /// <returns>XAML</returns>
        public static string GetXAML(object instance, bool includeRuntime, string name, bool filterPixelCountPerDIP)
        {
            var xamlUntidy = GetXAML(instance, includeRuntime);

            XDocument document = XDocument.Parse(xamlUntidy);

            NormalizeDrawingElement(document.Root, name);

            if (filterPixelCountPerDIP)
            {
                FilterPixelCountPerDIP(document.Root);
            }

            string temporaryXAML = document.ToString();

            string xaml = RemoveNamespaceDeclaration(temporaryXAML);

            return xaml;
        }

        #endregion
        #region XAML 구하기 - GetXAML(filePath, conversionMode, resourceKeyInfo, filterPixelCountPerDIP, settings)

        /// <summary>
        /// XAML 구하기
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="conversionMode">변환 모드</param>
        /// <param name="resourceKeyInfo">리소스 키 정보</param>
        /// <param name="filterPixelCountPerDIP">DIP당 픽셀 카운트 필터링 여부</param>
        /// <param name="settings">WPF 그리기 설정</param>
        /// <returns>XAML</returns>
        public static string GetXAML
        (
            string filePath,
            ConversionMode conversionMode,
            ResourceKeyInfo resourceKeyInfo,
            bool filterPixelCountPerDIP,
            WpfDrawingSettings settings = null
        )
        {
            object instance = GetObject(filePath, conversionMode, settings, out var name, resourceKeyInfo);

            return GetXAML(instance, settings != null && settings.IncludeRuntime, name, filterPixelCountPerDIP);
        }

        #endregion
        #region SVG 데이터 구하기 - GetSVGData(filePath)

        /// <summary>
        /// SVG 데이터 구하기
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <returns>SVG 데이터</returns>
        public static SVGData GetSVGData(string filePath)
        {
            return new SVGData { FilePath = filePath };
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////// Private

        #region SVGZ 파일 여부 구하기 - IsSVGZFile(filePath)

        /// <summary>
        /// SVGZ 파일 여부 구하기
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <returns>SVGZ 파일 여부</returns>
        private static bool IsSVGZFile(string filePath)
        {
            return string.Equals(Path.GetExtension(filePath), ".svgz", StringComparison.OrdinalIgnoreCase);
        }

        #endregion
        #region ID 정정하기 - FixID(rootElement)

        /// <summary>
        /// ID 정정하기
        /// </summary>
        /// <param name="rootElement">루트 엘리먼트</param>
        private static void FixID(XElement rootElement)
        {
            IEnumerable<XAttribute> attributeEnumerable = rootElement.DescendantsAndSelf()
                .SelectMany(d => d.Attributes())
                .Where(a => string.Equals(a.Name.LocalName, "Id", StringComparison.InvariantCultureIgnoreCase));

            foreach (XAttribute attribute in attributeEnumerable)
            {
                if (char.IsDigit(attribute.Value.FirstOrDefault()))
                {
                    attribute.Value = "_" + attribute.Value;
                }

                attribute.Value = attribute.Value.Replace("/", "_");
            }
        }

        #endregion
        #region 드로잉 그룹 구하기 (코어) - GetDrawingGroupCore(filePath, settings)

        /// <summary>
        /// 드로잉 그룹 구하기 (코어)
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="settings">설정</param>
        /// <returns>드로잉 그룹</returns>
        private static DrawingGroup GetDrawingGroupCore(string filePath, WpfDrawingSettings settings)
        {
            if (settings == null)
            {
                settings = new WpfDrawingSettings
                {
                    IncludeRuntime = false,
                    TextAsGeometry = false,
                    OptimizePath = true
                };
            }

            FileSvgReader reader = new FileSvgReader(settings);

            filePath = Path.GetFullPath(filePath);

            Stream stream = IsSVGZFile(filePath) ? new GZipStream(File.OpenRead(filePath), CompressionMode.Decompress, false) :
                                                   File.OpenRead(filePath);

            XDocument document = XDocument.Load(stream);

            stream.Dispose();

            FixID(document.Root); // 예 : "3d-view-icon" -> "_3d-view-icon"

            using (MemoryStream memoryStream = new MemoryStream())
            {
                document.Save(memoryStream);

                memoryStream.Position = 0;

                reader.Read(memoryStream);

                return reader.Drawing;
            }
        }

        #endregion
        #region 드로잉 그룹에서 크기 구하기 - GetSizeFromDrawingGroup(drawingGroup)

        /// <summary>
        /// 드로잉 그룹에서 크기 구하기
        /// </summary>
        /// <param name="drawingGroup">드로잉 그룹</param>
        /// <returns>크기</returns>
        private static Size? GetSizeFromDrawingGroup(DrawingGroup drawingGroup)
        {
            if (drawingGroup != null)
            {
                DrawingGroup firstChild = drawingGroup.Children.OfType<DrawingGroup>()
                                                               .FirstOrDefault(child => child.ClipGeometry != null);

                if (firstChild != null)
                {
                    return firstChild.ClipGeometry.Bounds.Size;
                }
            }

            return null;
        }

        #endregion
        #region 패스 지오메트리 리스트 구하기 - GetPathGeometryList(sourceDrawing)

        /// <summary>
        /// 패스 지오메트리 리스트 구하기
        /// </summary>
        /// <param name="sourceDrawing">소스 드로잉</param>
        /// <returns>패스 지오메트리 리스트</returns>
        private static List<PathGeometry> GetPathGeometryList(Drawing sourceDrawing)
        {
            List<PathGeometry> list = new List<PathGeometry>();

            Action<Drawing> action = null;

            action = drawing =>
            {
                if (drawing is DrawingGroup)
                {
                    foreach (Drawing childDrawing in ((DrawingGroup)drawing).Children)
                    {
                        action(childDrawing);
                    }
                }

                if (drawing is GeometryDrawing)
                {
                    GeometryDrawing geometryDrawing = (GeometryDrawing)drawing;

                    Geometry geometry = geometryDrawing.Geometry;

                    if (geometry is PathGeometry)
                    {
                        list.Add((PathGeometry)geometry);
                    }
                }
            };

            action(sourceDrawing);

            return list;
        }

        #endregion
        #region 크기 패스 피규어 추가하기 - AddSizePathFigure(sourceGeometry, size)

        /// <summary>
        /// 크기 패스 피규어 추가하기
        /// </summary>
        /// <param name="sourceGeometry">소스 패스 지오메트리</param>
        /// <param name="size">크기</param>
        private static void AddSizePathFigure(PathGeometry sourceGeometry, Size size)
        {
            if (size.Width > 0 && size.Height > 0)
            {
                PathFigure[] pathFigureArray =
                {
                    new PathFigure
                    (
                        new Point(size.Width, size.Height),
                        Enumerable.Empty<PathSegment>(),
                        true
                    ),
                    new PathFigure
                    (
                        new Point(0,0),
                        Enumerable.Empty<PathSegment>(),
                        true
                    )
                };

                PathGeometry targetGeometry = new PathGeometry
                (
                    pathFigureArray.Concat(sourceGeometry.Figures),
                    sourceGeometry.FillRule,
                    null
                );

                sourceGeometry.Clear();

                sourceGeometry.AddGeometry(targetGeometry);
            }
        }

        #endregion
        #region 패스 지오메트리에 크기 설정하기 - SetSizeToPathGeometry(drawingGroup)

        /// <summary>
        /// 패스 지오메트리에 크기 설정하기
        /// </summary>
        /// <param name="drawingGroup">드로잉 그룹</param>
        private static void SetSizeToPathGeometry(DrawingGroup drawingGroup)
        {
            Size? size = GetSizeFromDrawingGroup(drawingGroup);

            if (size.HasValue)
            {
                List<PathGeometry> pathGeometryList = GetPathGeometryList(drawingGroup);

                pathGeometryList.ForEach(pathGemoetry => AddSizePathFigure(pathGemoetry, size.Value));
            }
        }

        #endregion
        #region 객체 명칭 값 제거하기 - RemoveObjectNameValue(drawingGroup)

        /// <summary>
        /// 객체 명칭 값 제거하기
        /// </summary>
        /// <param name="drawingGroup">드로잉 그룹</param>
        public static void RemoveObjectNameValue(DrawingGroup drawingGroup)
        {
            if (drawingGroup.GetValue(FrameworkElement.NameProperty) != null)
            {
                drawingGroup.SetValue(FrameworkElement.NameProperty, null);
            }

            foreach (DependencyObject child in drawingGroup.Children.OfType<DependencyObject>())
            {
                if (child.GetValue(FrameworkElement.NameProperty) != null)
                {
                    child.SetValue(FrameworkElement.NameProperty, null);
                }

                if (child is DrawingGroup)
                {
                    RemoveObjectNameValue(child as DrawingGroup);
                }
            }
        }

        #endregion
        #region 드로잉 그룹 구하기 - GetDrawingGroup(filePath, settings)

        /// <summary>
        /// 드로잉 그룹 구하기
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="settings">설정</param>
        /// <returns>드로잉 그룹</returns>
        private static DrawingGroup GetDrawingGroup(string filePath, WpfDrawingSettings settings)
        {
            DrawingGroup drawingGroup = GetDrawingGroupCore(filePath, settings);

            SetSizeToPathGeometry(drawingGroup);

            RemoveObjectNameValue(drawingGroup);

            return drawingGroup;
        }

        #endregion

        #region 명칭 검증하기 - ValidateName(name)

        /// <summary>
        /// 명칭 검증하기
        /// </summary>
        /// <param name="name">명칭</param>
        /// <returns>명칭</returns>
        private static string ValidateName(string name)
        {
            string result = Regex.Replace(name, @"[^[0-9a-zA-Z]]*", "_");

            if (Regex.IsMatch(result, "^[0-9].*"))
            {
                result = "_" + result;
            }

            return result;
        }

        #endregion
        #region 명칭 구하기 - GetName(sourceName, resourceKeyInfo)

        /// <summary>
        /// 명칭 구하기
        /// </summary>
        /// <param name="sourceName">소스 명칭</param>
        /// <param name="resourceKeyInfo">리소스 키 정보</param>
        /// <returns>명칭</returns>
        private static string GetName(string sourceName, ResourceKeyInfo resourceKeyInfo)
        {
            if (resourceKeyInfo.UseComponentRessourceKey)
            {
                return $"{{x:Static {resourceKeyInfo.NameSpaceName}:{resourceKeyInfo.XAMLName}.{ValidateName(sourceName)}Key}}";
            }

            string targetName = sourceName;

            if (resourceKeyInfo.Prefix != null)
            {
                targetName = resourceKeyInfo.Prefix + PREFIX_SEPARATOR + sourceName;
            }

            targetName = ValidateName(targetName);

            return targetName;
        }

        #endregion
        #region 드로잉 그룹명 구하기 - GetDrawingGroupName(elementName, resourceKeyInfo)

        /// <summary>
        /// 드로잉 그룹명 구하기
        /// </summary>
        /// <param name="elementName">엘리먼트명</param>
        /// <param name="resourceKeyInfo">리소스 키 정보</param>
        /// <returns>드로잉 그룹명</returns>
        private static string GetDrawingGroupName(string elementName, ResourceKeyInfo resourceKeyInfo)
        {
            string sourceName = elementName + "DrawingGroup";

            return GetName(sourceName, resourceKeyInfo);
        }

        #endregion
        #region 드로잉 이미지명 구하기 - GetDrawingImageName(elementName, resourceKeyInfo)

        /// <summary>
        /// 드로잉 이미지명 구하기
        /// </summary>
        /// <param name="elementName">엘리먼트명</param>
        /// <param name="resourceKeyInfo">리소스 키 정보</param>
        /// <returns>드로잉 이미지명</returns>
        private static string GetDrawingImageName(string elementName, ResourceKeyInfo resourceKeyInfo)
        {
            string sourceName = elementName + "DrawingImage";

            return GetName(sourceName, resourceKeyInfo);
        }

        #endregion

        #region 드로잉 이미지 구하기 - GetDrawingImage(drawing)

        /// <summary>
        /// 드로잉 이미지 구하기
        /// </summary>
        /// <param name="drawing">드로잉</param>
        /// <returns>드로잉 이미지</returns>
        private static DrawingImage GetDrawingImage(Drawing drawing)
        {
            return new DrawingImage(drawing);
        }

        #endregion

        #region XAML 구하기 - GetXAML(instance, includeRuntime)

        /// <summary>
        /// XAML 구하기
        /// </summary>
        /// <param name="instance">인스턴스</param>
        /// <param name="includeRuntime">런타임 포함 여부</param>
        /// <returns>XAML</returns>
        private static string GetXAML(object instance, bool includeRuntime)
        {
            XmlXamlWriter writer = new XmlXamlWriter(new WpfDrawingSettings { IncludeRuntime = includeRuntime });

            string xaml = writer.Save(instance);

            return xaml;
        }

        #endregion
        #region 클리핑 엘리먼트 구하기 - GetClippingElement(drawingGroupElement, rectangle)

        /// <summary>
        /// 클리핑 엘리먼트 구하기
        /// </summary>
        /// <param name="drawingGroupElement">드로잉 그룹 엘리먼트</param>
        /// <param name="rectangle">사각형</param>
        /// <returns>클리핑 엘리먼트</returns>
        private static XElement GetClippingElement(XElement drawingGroupElement, out Rect rectangle)
        {
            rectangle = default;

            if (drawingGroupElement == null)
            {
                return null;
            }

            XElement clippingElement = drawingGroupElement.XPathSelectElement(".//defns:DrawingGroup.ClipGeometry", _xmlNamespaceManager);

            if (clippingElement != null)
            {
                XElement rectangleElement = clippingElement.Element(_defaultXMLNamespace + "RectangleGeometry");

                if (rectangleElement != null)
                {
                    XAttribute rectangleAttribute = rectangleElement.Attribute("Rect");

                    if (rectangleAttribute != null)
                    {
                        rectangle = Rect.Parse(rectangleAttribute.Value);

                        return clippingElement;
                    }
                }
            }

            return null;
        }

        #endregion
        #region 인라인 클리핑 추가하기 - AddInlineClipping(drawingElement)

        /// <summary>
        /// 인라인 클리핑 추가하기
        /// </summary>
        /// <param name="drawingElement">드로잉 엘리먼트</param>
        private static void AddInlineClipping(XElement drawingElement)
        {
            Rect clippingRectangle;

            XElement clippingElement = GetClippingElement(drawingElement, out clippingRectangle);

            if (clippingElement != null && clippingElement.Parent.Name.LocalName == "DrawingGroup")
            {
                clippingElement.Parent.Add
                (
                    new XAttribute
                    (
                        "ClipGeometry",
                        string.Format
                        (
                            CultureInfo.InvariantCulture,
                            "M{0},{1} V{2} H{3} V{0} H{1} Z",
                            clippingRectangle.Left,
                            clippingRectangle.Top,
                            clippingRectangle.Bottom,
                            clippingRectangle.Right
                        )
                    )
                );

                clippingElement.Remove();
            }
        }

        #endregion
        #region 계단식 드로잉 그룹 제거하기 - RemoveCascadedDrawingGroup(drawingElement)

        /// <summary>
        /// 계단식 드로잉 그룹 제거하기
        /// </summary>
        /// <param name="drawingElement">드로잉 엘리먼트</param>
        private static void RemoveCascadedDrawingGroup(XElement drawingElement)
        {
            IEnumerable<XElement> drawingGroupEnumerable = drawingElement.DescendantsAndSelf(_defaultXMLNamespace + "DrawingGroup");

            foreach (XElement drawingGroup in drawingGroupEnumerable)
            {
                List<XElement> elementList = drawingGroup.Elements().ToList();

                if (elementList.Count == 1 && elementList[0].Name.LocalName == "DrawingGroup")
                {
                    XElement childDrawingGroup = elementList[0];

                    IEnumerable<XName> childAttributeNameEnumerable = childDrawingGroup.Attributes().Select(a => a.Name);

                    IEnumerable<XName> attributeNameEnumerable = drawingGroup.Attributes().Select(a => a.Name);

                    if (childAttributeNameEnumerable.Intersect(attributeNameEnumerable).Any())
                    {
                        return;
                    }

                    drawingGroup.Add(childDrawingGroup.Attributes());

                    drawingGroup.Add(childDrawingGroup.Elements());

                    childDrawingGroup.Remove();
                }
            }
        }

        #endregion
        #region 패스 지오메트리 축소하기 - CollapsePathGeometries(drawingElement)

        /// <summary>
        /// 패스 지오메트리 축소하기
        /// </summary>
        /// <param name="drawingElement">드로잉 엘리먼트</param>
        private static void CollapsePathGeometries(XElement drawingElement)
        {
            XElement[] pathGeometryElementArray = drawingElement.Descendants(_defaultXMLNamespace + "PathGeometry").ToArray();

            foreach (XElement pathGeometryElement in pathGeometryElementArray)
            {
                if
                (
                    pathGeometryElement.Parent != null &&
                    pathGeometryElement.Parent.Parent != null &&
                    pathGeometryElement.Parent.Parent.Name.LocalName == "GeometryDrawing"
                )
                {
                    List<string> attributeNameList = pathGeometryElement.Attributes().Select(a => a.Name.LocalName).ToList();

                    if
                    (
                        attributeNameList.Count <= 2 &&
                        attributeNameList.Contains("Figures") &&
                        (attributeNameList.Contains("FillRule") || attributeNameList.Count == 1)
                    )
                    {
                        string figuresValue = pathGeometryElement.Attribute("Figures").Value;

                        XAttribute fillRuleAttribute = pathGeometryElement.Attribute("FillRule");

                        if (fillRuleAttribute != null)
                        {
                            if (fillRuleAttribute.Value == "Nonzero")
                            {
                                figuresValue = "F1 " + figuresValue; // Nonzero
                            }
                            else
                            {
                                figuresValue = "F0 " + figuresValue; // EvenOdd
                            }
                        }

                        pathGeometryElement.Parent.Parent.Add(new XAttribute("Geometry", figuresValue));

                        pathGeometryElement.Parent.Remove();
                    }
                }
            }
        }

        #endregion
        #region 드로잉 엘리먼트 명칭 설정하기 - SetDrawingElementName(drawingElement, name)

        /// <summary>
        /// 드로잉 엘리먼트 명칭 설정하기
        /// </summary>
        /// <param name="drawingElement">드로잉 엘리먼트</param>
        /// <param name="name">명칭</param>
        private static void SetDrawingElementName(XElement drawingElement, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            List<XAttribute> attributeList = drawingElement.Attributes().ToList();

            attributeList.Insert(0, new XAttribute(_xXMLNamespace + "Key", name));

            drawingElement.ReplaceAttributes(attributeList);
        }

        #endregion
        #region 드로잉 엘리먼트 정규화하기 - NormalizeDrawingElement(drawingElement, name)

        /// <summary>
        /// 드로잉 엘리먼트 정규화하기
        /// </summary>
        /// <param name="drawingElement">드로잉 엘리먼트</param>
        /// <param name="name">명칭</param>
        private static void NormalizeDrawingElement(XElement drawingElement, string name)
        {
            AddInlineClipping(drawingElement);

            RemoveCascadedDrawingGroup(drawingElement);

            CollapsePathGeometries(drawingElement);

            SetDrawingElementName(drawingElement, name);
        }

        #endregion
        #region DIP당 픽셀 카운트 필터링하기 - FilterPixelCountPerDIP(drawingElement)

        /// <summary>
        /// DIP당 픽셀 카운트 필터링하기
        /// </summary>
        /// <param name="drawingElement">드로잉 엘리먼트</param>
        private static void FilterPixelCountPerDIP(XElement drawingElement)
        {
            List<XElement> glyphRunElementList = drawingElement.Descendants(_defaultXMLNamespace + nameof(GlyphRun)).ToList();

            foreach (XElement glyphRunElement in glyphRunElementList)
            {
                XAttribute pixelCountPerDIPAttribute = glyphRunElement.Attribute(nameof(GlyphRun.PixelsPerDip));

                if (pixelCountPerDIPAttribute != null)
                {
                    pixelCountPerDIPAttribute.Remove();
                }
            }
        }

        #endregion
        #region 네임스페이스 선언 제거하기 - RemoveNamespaceDeclaration(source)

        /// <summary>
        /// 네임스페이스 선언 제거하기
        /// </summary>
        /// <param name="source">소스 문자열</param>
        /// <returns>네임스페이서 선언 제거 문자열</returns>
        public static string RemoveNamespaceDeclaration(string source)
        {
            source = source.Replace(" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"", "");
            source = source.Replace(" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"", "");

            return source;
        }

        #endregion
    }
}