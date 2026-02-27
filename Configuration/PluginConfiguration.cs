using MediaBrowser.Model.Plugins;

namespace Emby.ParameterPersistence.Configuration
{
    /// <summary>
    /// 插件配置类
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// 是否启用日志记录
        /// </summary>
        public bool EnableLogging { get; set; }

        /// <summary>
        /// 最大参数数量限制
        /// </summary>
        public int MaxParameterCount { get; set; }

        public PluginConfiguration()
        {
            EnableLogging = true;
            MaxParameterCount = 10000;
        }
    }
}

