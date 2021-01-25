using System.Collections.Generic;

namespace OnlyOfficeDocumentClientNetCore.Model
{
    /// <summary>
    /// 文件配置
    /// </summary>
    public class FileConfig
    {
        /// <summary>
        /// 文件查看方式 桌面desktop   手机mobile   嵌入设备embedded
        /// </summary>
        public string Type { get; set; } = "desktop";

        /// <summary>
        /// 文件类型   text  文本 spreadsheet 表格  presentation  幻灯片
        /// </summary>
        public string documentType { get; set; }

        public Document document { get; set; } = new Document();

        public EditorConfig editorConfig { get; set; } = new EditorConfig();


        public string token { get; set; } = string.Empty;
    }

    public class Permissions
    {
        /// <summary>
        /// 是否可评论
        /// </summary>
        public bool comment { get; set; }

        /// <summary>
        /// 是否可以下载
        /// </summary>
        public bool download { get; set; }

        /// <summary>
        /// 是否可以编辑
        /// </summary>
        public bool edit { get; set; }

        /// <summary>
        /// 是否过滤
        /// </summary>
        public bool modifyFilter { get; set; }

        /// <summary>
        /// 是否显示修改内容控件
        /// </summary>
        public bool modifyContentControl { get; set; }

        /// <summary>
        /// 是否可以查看
        /// </summary>
        public bool review { get; set; }
    }

    public class Info
    {
        /// <summary>
        /// 文件拥有者
        /// </summary>
        public string owner { get; set; }

        /// <summary>
        /// 文件上传日期 2020-01-01
        /// </summary>
        public string uploaded { get; set; }
    }

    public class Document
    {
        /// <summary>
        /// 文件标题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 文件地址，要可下载
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string fileType { get; set; }

        /// <summary>
        ///  唯一标示，做版本识别用
        /// </summary>
        public string key { get; set; }

        public Permissions permissions { get; set; } = new Permissions();

        public Info info { get; set; } = new Info();
    }

    public class User
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string name { get; set; }
    }

    public class Embedded
    {
        /// <summary>
        /// 保存文件地址
        /// </summary>
        public string saveUrl { get; set; }

        /// <summary>
        /// 嵌入式文件地址
        /// </summary>
        public string embedUrl { get; set; }

        /// <summary>
        /// 分享文件地址
        /// </summary>
        public string shareUrl { get; set; }

        /// <summary>
        ///  工具栏位置  默认top
        /// </summary>
        public string toolbarDocked { get; set; }
    }

    public class Customization
    {
        /// <summary>
        /// 关于链接地址
        /// </summary>
        public bool about { get; set; }

        /// <summary>
        /// 反馈链接地址
        /// </summary>
        public bool feedback { get; set; }

        /// <summary>
        /// 默认返回页 ,当发生错误，或者莫有权限等时候，会跳转到此页  ，值  { "url", DefaultConfigTools.Host + "default.aspx" }
        /// </summary>
        public Dictionary<string, object> goback { get; set; } = new Dictionary<string, object>();
            }

    public class EditorConfig
    {
        /// <summary>
        /// 激活链接
        /// </summary>
        public string actionLink { get; set; }

        /// <summary>
        /// 查看或编辑  edit  view
        /// </summary>
        public string mode { get; set; }

        /// <summary>
        /// 语言 en   cn
        /// </summary>
        public string lang { get; set; }

        /// <summary>
        /// 回调地址，保存修改后文件用
        /// </summary>

        public string callbackUrl { get; set; }

        public User user { get; set; } = new User();
        public Embedded embedded { get; set; } = new Embedded();

        public Customization customization { get; set; } = new Customization();
    }
}