namespace SvgToXaml.Model
{
    /// <summary>
    /// 리소스 키 정보
    /// </summary>
    public class ResourceKeyInfo
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Property
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region 명칭 - Name

        /// <summary>
        /// 명칭
        /// </summary>
        public string Name { get; set; }

        #endregion
        #region XAML 명칭 - XAMLName

        /// <summary>
        /// XAML 명칭
        /// </summary>
        public string XAMLName { get; set; }

        #endregion
        #region 접두사 - Prefix

        /// <summary>
        /// 접두사
        /// </summary>
        public string Prefix { get; set; }

        #endregion
        #region 컴포넌트 리소스 키 사용 여부 - UseComponentRessourceKey

        /// <summary>
        /// 컴포넌트 리소스 키 사용 여부
        /// </summary>
        public bool UseComponentRessourceKey { get; set; }

        #endregion
        #region 네임스페이스 - NameSpace

        /// <summary>
        /// 네임스페이스
        /// </summary>
        public string NameSpace { get; set; }

        #endregion
        #region 네임스페이스명 - NameSpaceName

        /// <summary>
        /// 네임스페이스명
        /// </summary>
        public string NameSpaceName { get; set; }

        #endregion
    }
}