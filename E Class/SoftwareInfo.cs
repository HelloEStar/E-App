using System;

namespace E.Class
{
    public class AppInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get;}
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get;}
        /// <summary>
        /// 组织
        /// </summary>
        public string Company { get;}
        /// <summary>
        /// 作者
        /// </summary>
        public string Developer { get;}
        /// <summary>
        /// 信息链接
        /// </summary>
        public string InfoLink { get;}
        /// <summary>
        /// 下载链接
        /// </summary>
        public string DownloadLink { get;}
        /// <summary>
        /// 当前版本
        /// </summary>
        public Version CurrentVersion { get; set; }
        /// <summary>
        /// 最新版本
        /// </summary>
        public Version LatestVersion { get; set; }
        /// <summary>
        /// 更新日志
        /// </summary>
        public UpdateNote[] UpdateNotes { get; set; }

        public AppInfo(string name, string description, string company, string developer, string infoLink, string downloadLink, Version currentVersion, Version latestVersion)
        {
            Name = name;
            Description = description;
            Company = company;
            Developer = developer;
            InfoLink = infoLink;
            DownloadLink = downloadLink;
            CurrentVersion = currentVersion;
            LatestVersion = latestVersion;
        }

        public struct UpdateNote
        {
            /// <summary>
            /// 版本号
            /// </summary>
            public Version Version { get; set; }
            /// <summary>
            /// 发布日期
            /// </summary>
            public DateTime Time { get; set; }
            /// <summary>
            /// 新增功能
            /// </summary>
            public string[] Features { get; set; }
            /// <summary>
            /// 优化调整
            /// </summary>
            public string[] Optimizations { get; set; }
            /// <summary>
            /// 问题修复
            /// </summary>
            public string[] BugFixs { get; set; }
        }
    }
}