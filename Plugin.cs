using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
            SetVersionFromResource();
        }

        public override string Name => "参数持久化";

        public override string Description => "提供参数持久化API，支持将参数存储在本地JSON文件中";

        public override Guid Id => new Guid("8F6D8C9E-4B2A-4F3E-9D1C-7A8B9C0D1E2F");

        /// <summary>
        /// 从嵌入的 version.json 读取版本号并通过反射写入父类
        /// </summary>
        private void SetVersionFromResource()
        {
            try
            {
                var version = ReadVersionFromResource();
                // BasePlugin.Version 是自动属性，编译器生成的 backing field 名为 <Version>k__BackingField
                var field = typeof(BasePlugin<PluginConfiguration>)
                    .GetField("<Version>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(this, version);
                }
            }
            catch { }
        }

        /// <summary>
        /// 从嵌入的 version.json 中读取版本号
        /// </summary>
        private static Version ReadVersionFromResource()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "Emby.ParameterPersistence.version.json";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return new Version(1, 0, 0);
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        // 简单解析 "version":"x.x.x"，不引入额外依赖
                        var key = "\"version\":\"";
                        var start = json.IndexOf(key, StringComparison.Ordinal);
                        if (start < 0) return new Version(1, 0, 0);
                        start += key.Length;
                        var end = json.IndexOf('"', start);
                        if (end < 0) return new Version(1, 0, 0);
                        var versionStr = json.Substring(start, end - start);
                        return Version.Parse(versionStr);
                    }
                }
            }
            catch
            {
                return new Version(1, 0, 0);
            }
        }

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

