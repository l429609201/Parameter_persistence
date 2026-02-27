using System;

namespace Emby.ParameterPersistence.Models
{
    /// <summary>
    /// 参数模型
    /// </summary>
    public class ParameterModel
    {
        /// <summary>
        /// 参数唯一标识
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 命名空间（用于分类管理）
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// 参数键名
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 参数类型（string, number, boolean, json）
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 参数描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        public ParameterModel()
        {
            Id = Guid.NewGuid().ToString();
            Type = "string";
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

