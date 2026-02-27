using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.ParameterPersistence.Models;
using Emby.ParameterPersistence.Services;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;

namespace Emby.ParameterPersistence.Api
{
    // ======================== Request Models ========================

    [Route("/ParameterPersistence/Query", "GET", Summary = "Query parameters")]
    public class QueryRequest : IReturn<ParameterResponse>
    {
        [ApiMember(Name = "Namespace", Description = "Namespace", IsRequired = false, DataType = "string", ParameterType = "query")]
        public string Namespace { get; set; }

        [ApiMember(Name = "Key", Description = "Parameter key", IsRequired = false, DataType = "string", ParameterType = "query")]
        public string Key { get; set; }

        [ApiMember(Name = "Keyword", Description = "Search keyword", IsRequired = false, DataType = "string", ParameterType = "query")]
        public string Keyword { get; set; }
    }

    [Route("/ParameterPersistence/Create", "POST", Summary = "Create parameters")]
    public class CreateRequest : IReturn<ParameterResponse>
    {
        public string Namespace { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public List<ParameterItem> Parameters { get; set; }
    }

    [Route("/ParameterPersistence/Update", "POST", Summary = "Update parameters")]
    public class UpdateRequest : IReturn<ParameterResponse>
    {
        public string Namespace { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public List<ParameterItem> Parameters { get; set; }
    }

    [Route("/ParameterPersistence/Delete", "POST", Summary = "Delete parameters")]
    public class DeleteRequest : IReturn<ParameterResponse>
    {
        public string Namespace { get; set; }
        public string Key { get; set; }
        public List<string> Keys { get; set; }
        public List<ParameterItem> Parameters { get; set; }
    }

    public class ParameterItem
    {
        public string Namespace { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }

    // ======================== Controller ========================

    public class ParameterController : IService
    {
        private readonly ILogger _logger;
        private readonly IAuthorizationContext _authContext;
        private static ParameterStorageService _storageService;
        private static readonly object _lock = new object();

        public ParameterController(
            ILogManager logManager,
            IApplicationPaths appPaths,
            IAuthorizationContext authContext,
            IJsonSerializer jsonSerializer)
        {
            _logger = logManager.GetLogger("ParameterPersistence");
            _authContext = authContext;

            if (_storageService == null)
            {
                lock (_lock)
                {
                    if (_storageService == null)
                    {
                        _storageService = new ParameterStorageService(appPaths, logManager, jsonSerializer);
                    }
                }
            }
        }

        private void ValidateAuthentication(IRequest request)
        {
            if (request == null) return;
            try
            {
                var auth = _authContext.GetAuthorizationInfo(request);
                if (auth == null) return;
            }
            catch { }
        }


        // ======================== GET Query ========================

        public async Task<object> Get(QueryRequest request)
        {
            try
            {
                ValidateAuthentication(Request);

                if (!string.IsNullOrEmpty(request.Key))
                {
                    var ns = string.IsNullOrEmpty(request.Namespace) ? "default" : request.Namespace;
                    var parameter = await _storageService.GetParameterAsync(ns, request.Key);

                    if (parameter == null)
                    {
                        return new ParameterResponse { Success = false, Message = "参数不存在" };
                    }

                    return new ParameterResponse { Success = true, Data = parameter };
                }

                List<ParameterModel> parameters;

                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    parameters = await _storageService.SearchParametersAsync(request.Keyword);
                }
                else if (!string.IsNullOrEmpty(request.Namespace))
                {
                    parameters = await _storageService.GetParametersByNamespaceAsync(request.Namespace);
                }
                else
                {
                    parameters = await _storageService.GetAllParametersAsync();
                }

                return new ParameterResponse { Success = true, DataList = parameters, Total = parameters.Count };
            }
            catch (Exception ex)
            {
                _logger.Error($"查询参数失败: {ex.Message}");
                return new ParameterResponse { Success = false, Message = $"查询参数失败: {ex.Message}" };
            }
        }

        // ======================== POST Create ========================

        public async Task<object> Post(CreateRequest request)
        {
            try
            {
                ValidateAuthentication(Request);

                if (request.Parameters != null && request.Parameters.Count > 0)
                {
                    var results = new List<ParameterModel>();
                    var errors = new List<string>();

                    foreach (var item in request.Parameters)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(item.Key))
                            {
                                errors.Add("参数键名不能为空");
                                continue;
                            }

                            var ns = string.IsNullOrEmpty(item.Namespace) ? "default" : item.Namespace;
                            var parameter = new ParameterModel
                            {
                                Namespace = ns,
                                Key = item.Key,
                                Value = item.Value ?? "",
                                Type = item.Type ?? "string",
                                Description = item.Description
                            };

                            var result = await _storageService.CreateParameterAsync(parameter);
                            results.Add(result);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"创建参数 {item.Key} 失败: {ex.Message}");
                            _logger.Error($"批量创建参数失败 - Key: {item.Key}, Error: {ex.Message}");
                        }
                    }

                    var message = $"成功创建 {results.Count} 个参数";
                    if (errors.Count > 0) message += $"，失败 {errors.Count} 个";

                    return new ParameterResponse
                    {
                        Success = errors.Count < request.Parameters.Count,
                        DataList = results,
                        Total = results.Count,
                        Message = message
                    };
                }
                else if (!string.IsNullOrEmpty(request.Key))
                {
                    var ns = string.IsNullOrEmpty(request.Namespace) ? "default" : request.Namespace;
                    var parameter = new ParameterModel
                    {
                        Namespace = ns,
                        Key = request.Key,
                        Value = request.Value ?? "",
                        Type = request.Type ?? "string",
                        Description = request.Description
                    };

                    var result = await _storageService.CreateParameterAsync(parameter);
                    return new ParameterResponse { Success = true, Data = result, Message = "参数创建成功" };
                }
                else
                {
                    return new ParameterResponse { Success = false, Message = "请提供 Key 或 Parameters 参数" };
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"创建参数失败: {ex.Message}");
                return new ParameterResponse { Success = false, Message = $"创建参数失败: {ex.Message}" };
            }
        }

        // ======================== POST Update ========================

        public async Task<object> Post(UpdateRequest request)
        {
            try
            {
                ValidateAuthentication(Request);

                if (request.Parameters != null && request.Parameters.Count > 0)
                {
                    var results = new List<ParameterModel>();
                    var errors = new List<string>();

                    foreach (var item in request.Parameters)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(item.Key))
                            {
                                errors.Add("参数键名不能为空");
                                continue;
                            }

                            var ns = string.IsNullOrEmpty(item.Namespace) ? "default" : item.Namespace;
                            var result = await _storageService.UpdateParameterAsync(ns, item.Key, item.Value ?? "", item.Description);
                            results.Add(result);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"更新参数 {item.Key} 失败: {ex.Message}");
                            _logger.Error($"批量更新参数失败 - Key: {item.Key}, Error: {ex.Message}");
                        }
                    }

                    var message = $"成功更新 {results.Count} 个参数";
                    if (errors.Count > 0) message += $"，失败 {errors.Count} 个";

                    return new ParameterResponse
                    {
                        Success = errors.Count < request.Parameters.Count,
                        DataList = results,
                        Total = results.Count,
                        Message = message
                    };
                }
                else if (!string.IsNullOrEmpty(request.Key))
                {
                    var ns = string.IsNullOrEmpty(request.Namespace) ? "default" : request.Namespace;
                    var result = await _storageService.UpdateParameterAsync(ns, request.Key, request.Value ?? "", request.Description);

                    return new ParameterResponse { Success = true, Data = result, Message = "参数更新成功" };
                }
                else
                {
                    return new ParameterResponse { Success = false, Message = "请提供 Key 或 Parameters 参数" };
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"更新参数失败: {ex.Message}");
                return new ParameterResponse { Success = false, Message = $"更新参数失败: {ex.Message}" };
            }
        }

        // ======================== POST Delete ========================

        public async Task<object> Post(DeleteRequest request)
        {
            try
            {
                ValidateAuthentication(Request);

                if (request.Parameters != null && request.Parameters.Count > 0)
                {
                    var successCount = 0;
                    var errors = new List<string>();

                    foreach (var item in request.Parameters)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(item.Key))
                            {
                                errors.Add("参数键名不能为空");
                                continue;
                            }

                            var ns = string.IsNullOrEmpty(item.Namespace) ? "default" : item.Namespace;
                            var result = await _storageService.DeleteParameterAsync(ns, item.Key);
                            if (result) successCount++;
                            else errors.Add($"参数 {item.Key} 不存在");
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"删除参数 {item.Key} 失败: {ex.Message}");
                            _logger.Error($"批量删除参数失败 - Key: {item.Key}, Error: {ex.Message}");
                        }
                    }

                    var message = $"成功删除 {successCount} 个参数";
                    if (errors.Count > 0) message += $"，失败 {errors.Count} 个";

                    return new ParameterResponse { Success = successCount > 0, Total = successCount, Message = message };
                }
                else if (request.Keys != null && request.Keys.Count > 0)
                {
                    var ns = string.IsNullOrEmpty(request.Namespace) ? "default" : request.Namespace;
                    var successCount = 0;

                    foreach (var key in request.Keys)
                    {
                        var result = await _storageService.DeleteParameterAsync(ns, key);
                        if (result) successCount++;
                    }

                    return new ParameterResponse { Success = successCount > 0, Total = successCount, Message = $"成功删除 {successCount} 个参数" };
                }
                else if (!string.IsNullOrEmpty(request.Key))
                {
                    var ns = string.IsNullOrEmpty(request.Namespace) ? "default" : request.Namespace;
                    var result = await _storageService.DeleteParameterAsync(ns, request.Key);

                    return new ParameterResponse { Success = result, Message = result ? "参数删除成功" : "参数不存在" };
                }
                else
                {
                    return new ParameterResponse { Success = false, Message = "请提供 Key、Keys 或 Parameters 参数" };
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"删除参数失败: {ex.Message}");
                return new ParameterResponse { Success = false, Message = $"删除参数失败: {ex.Message}" };
            }
        }

        private IRequest RequestContext => Request;
        public IRequest Request { get; set; }
    }
}