using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.ParameterPersistence.Models;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;
using Newtonsoft.Json;

namespace Emby.ParameterPersistence.Services
{
    /// <summary>
    /// 参数存储服务实现
    /// </summary>
    public class ParameterStorageService : IParameterStorageService
    {
        private readonly IApplicationPaths _appPaths;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
        private readonly string _dataFilePath;
        private readonly string _dataDirectory;

        public ParameterStorageService(IApplicationPaths appPaths, ILogManager logManager)
        {
            _appPaths = appPaths;
            _logger = logManager.GetLogger("ParameterPersistence");
            
            // 获取配置路径
            _dataDirectory = Path.Combine(_appPaths.PluginConfigurationsPath, "ParameterPersistence");
            _dataFilePath = Path.Combine(_dataDirectory, "parameters.json");
            
            // 确保目录存在
            EnsureDirectoryExists();
        }

        private void EnsureDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_dataDirectory))
                {
                    Directory.CreateDirectory(_dataDirectory);
                    _logger.Info($"创建参数存储目录: {_dataDirectory}");
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"创建目录失败: {_dataDirectory}", ex);
                throw;
            }
        }

        /// <summary>
        /// 读取参数数据
        /// </summary>
        private async Task<ParameterDataStore> ReadDataAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    return new ParameterDataStore();
                }

                var json = await Task.Run(() => File.ReadAllText(_dataFilePath));
                var data = JsonConvert.DeserializeObject<ParameterDataStore>(json);
                return data ?? new ParameterDataStore();
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"读取参数文件失败: {_dataFilePath}", ex);
                return new ParameterDataStore();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <summary>
        /// 写入参数数据
        /// </summary>
        private async Task WriteDataAsync(ParameterDataStore data)
        {
            await _fileLock.WaitAsync();
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                await Task.Run(() => File.WriteAllText(_dataFilePath, json));
                _logger.Info($"参数数据已保存，共 {data.Parameters.Count} 个参数");
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"写入参数文件失败: {_dataFilePath}", ex);
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<List<ParameterModel>> GetAllParametersAsync()
        {
            var data = await ReadDataAsync();
            return data.Parameters;
        }

        public async Task<ParameterModel> GetParameterAsync(string nameSpace, string key)
        {
            var data = await ReadDataAsync();
            return data.Parameters.FirstOrDefault(p => 
                p.Namespace == nameSpace && p.Key == key);
        }

        public async Task<List<ParameterModel>> GetParametersByNamespaceAsync(string nameSpace)
        {
            var data = await ReadDataAsync();
            return data.Parameters.Where(p => p.Namespace == nameSpace).ToList();
        }

        public async Task<ParameterModel> CreateParameterAsync(ParameterModel parameter)
        {
            var data = await ReadDataAsync();
            
            // 检查是否已存在
            var existing = data.Parameters.FirstOrDefault(p => 
                p.Namespace == parameter.Namespace && p.Key == parameter.Key);
            
            if (existing != null)
            {
                throw new InvalidOperationException($"参数已存在: {parameter.Namespace}.{parameter.Key}");
            }

            parameter.Id = Guid.NewGuid().ToString();
            parameter.CreatedAt = DateTime.UtcNow;
            parameter.UpdatedAt = DateTime.UtcNow;
            
            data.Parameters.Add(parameter);
            await WriteDataAsync(data);
            
            _logger.Info($"创建参数: {parameter.Namespace}.{parameter.Key}");
            return parameter;
        }

        public async Task<ParameterModel> UpdateParameterAsync(string nameSpace, string key, string value, string description = null)
        {
            var data = await ReadDataAsync();
            
            var parameter = data.Parameters.FirstOrDefault(p => 
                p.Namespace == nameSpace && p.Key == key);
            
            if (parameter == null)
            {
                throw new InvalidOperationException($"参数不存在: {nameSpace}.{key}");
            }

            parameter.Value = value;
            if (description != null)
            {
                parameter.Description = description;
            }
            parameter.UpdatedAt = DateTime.UtcNow;
            
            await WriteDataAsync(data);
            
            _logger.Info($"更新参数: {nameSpace}.{key}");
            return parameter;
        }

        public async Task<bool> DeleteParameterAsync(string nameSpace, string key)
        {
            var data = await ReadDataAsync();
            
            var parameter = data.Parameters.FirstOrDefault(p => 
                p.Namespace == nameSpace && p.Key == key);
            
            if (parameter == null)
            {
                return false;
            }

            data.Parameters.Remove(parameter);
            await WriteDataAsync(data);
            
            _logger.Info($"删除参数: {nameSpace}.{key}");
            return true;
        }

        public async Task<List<ParameterModel>> BatchOperationAsync(List<OperationItem> operations)
        {
            var results = new List<ParameterModel>();
            
            foreach (var op in operations)
            {
                try
                {
                    switch (op.Action?.ToLower())
                    {
                        case "create":
                            var createParam = new ParameterModel
                            {
                                Namespace = op.Namespace,
                                Key = op.Key,
                                Value = op.Value,
                                Type = op.Type ?? "string",
                                Description = op.Description
                            };
                            results.Add(await CreateParameterAsync(createParam));
                            break;
                            
                        case "update":
                            results.Add(await UpdateParameterAsync(op.Namespace, op.Key, op.Value, op.Description));
                            break;
                            
                        case "delete":
                            await DeleteParameterAsync(op.Namespace, op.Key);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException($"批量操作失败: {op.Action} {op.Namespace}.{op.Key}", ex);
                }
            }
            
            return results;
        }

        public async Task<List<ParameterModel>> SearchParametersAsync(string keyword)
        {
            var data = await ReadDataAsync();
            
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return data.Parameters;
            }

            keyword = keyword.ToLower();
            return data.Parameters.Where(p =>
                p.Namespace?.ToLower().Contains(keyword) == true ||
                p.Key?.ToLower().Contains(keyword) == true ||
                p.Value?.ToLower().Contains(keyword) == true ||
                p.Description?.ToLower().Contains(keyword) == true
            ).ToList();
        }
    }

    /// <summary>
    /// 参数数据存储结构
    /// </summary>
    internal class ParameterDataStore
    {
        public List<ParameterModel> Parameters { get; set; }

        public ParameterDataStore()
        {
            Parameters = new List<ParameterModel>();
        }
    }
}

