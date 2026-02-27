using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.ParameterPersistence.Models;

namespace Emby.ParameterPersistence.Services
{
    /// <summary>
    /// 参数存储服务接口
    /// </summary>
    public interface IParameterStorageService
    {
        /// <summary>
        /// 获取所有参数
        /// </summary>
        Task<List<ParameterModel>> GetAllParametersAsync();

        /// <summary>
        /// 根据命名空间和键名获取参数
        /// </summary>
        Task<ParameterModel> GetParameterAsync(string nameSpace, string key);

        /// <summary>
        /// 根据命名空间获取参数列表
        /// </summary>
        Task<List<ParameterModel>> GetParametersByNamespaceAsync(string nameSpace);

        /// <summary>
        /// 创建参数
        /// </summary>
        Task<ParameterModel> CreateParameterAsync(ParameterModel parameter);

        /// <summary>
        /// 更新参数
        /// </summary>
        Task<ParameterModel> UpdateParameterAsync(string nameSpace, string key, string value, string description = null);

        /// <summary>
        /// 删除参数
        /// </summary>
        Task<bool> DeleteParameterAsync(string nameSpace, string key);

        /// <summary>
        /// 批量操作
        /// </summary>
        Task<List<ParameterModel>> BatchOperationAsync(List<OperationItem> operations);

        /// <summary>
        /// 搜索参数
        /// </summary>
        Task<List<ParameterModel>> SearchParametersAsync(string keyword);
    }
}

