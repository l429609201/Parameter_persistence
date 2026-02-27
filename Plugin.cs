using System;
using System.Collections.Generic;
using Emby.ParameterPersistence.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Emby.ParameterPersistence
{
    /// <summary>
    /// 参数持久化插件主类
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public static Plugin Instance { get; private set; }

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override string Name => "参数持久化";

        public override string Description => "提供参数持久化API，支持将参数存储在本地JSON文件中";

        public override Guid Id => new Guid("8F6D8C9E-4B2A-4F3E-9D1C-7A8B9C0D1E2F");

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "parameterpersistence",
                    DisplayName = "参数持久化",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html",
                    EnableInMainMenu = true,
                    MenuSection = "advanced",
                    MenuIcon = "settings",
                    IsMainConfigPage = true
                },
                new PluginPageInfo
                {
                    Name = "parameterpersistencejs",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.js"
                }
            };
        }
    }
}

