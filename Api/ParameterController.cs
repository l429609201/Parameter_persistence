using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Emby.ParameterPersistence.Models;
using Emby.ParameterPersistence.Services;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Services;

namespace Emby.ParameterPersistence.Api
{
    /// <summary>
    /// 获取参数列表请求
    /// </summary>
    [Route("/ParameterPersistence/Parameters", "GET", Summary = "获取所有参数")]
    public class ParameterRequest : IReturn<ParameterResponse>
    {
        [ApiMember(Name = "Namespace", Description = "命名空间", IsRequired = false, DataType = "string", ParameterType = "query")]
        public string Namespace { get; set; }

        [ApiMember(Name = "Keyword", Description = "搜索关键词", IsRequired = false, DataType = "string", ParameterType = "query")]
        public string Keyword { get; set; }
    }

    /// <summary>
    /// 查询参数接口（统一单个和批量）
    /// </summary>
    [Route("/ParameterPersistence/Parameters/Query", "POST", Summary = "查询参数")]
    public class QueryParameterRequest : IReturn<ParameterResponse>
    {
        [ApiMember(Name = "Namespace", Description = "命名空间", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Namespace { get; set; }

        [ApiMember(Name = "Key", Description = "参数键名（单个查询）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Key { get; set; }

        [ApiMember(Name = "Keys", Description = "参数键名列表（批量查询）", IsRequired = false, DataType = "array", ParameterType = "body")]
        public List<string> Keys { get; set; }
    }

    /// <summary>
    /// 创建参数接口（统一单个和批量）
    /// </summary>
    [Route("/ParameterPersistence/Parameters/Create", "POST", Summary = "创建参数")]
    public class CreateParameterRequest : IReturn<ParameterResponse>
    {
        [ApiMember(Name = "Namespace", Description = "命名空间（单个创建）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Namespace { get; set; }

        [ApiMember(Name = "Key", Description = "参数键名（单个创建）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Key { get; set; }

        [ApiMember(Name = "Value", Description = "参数值（单个创建）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Value { get; set; }

        [ApiMember(Name = "Type", Description = "参数类型（单个创建）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Type { get; set; }

        [ApiMember(Name = "Description", Description = "参数描述（单个创建）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Description { get; set; }

        [ApiMember(Name = "Parameters", Description = "参数列表（批量创建）", IsRequired = false, DataType = "array", ParameterType = "body")]
        public List<ParameterItem> Parameters { get; set; }
    }

    /// <summary>
    /// 更新参数接口（统一单个和批量）
    /// </summary>
    [Route("/ParameterPersistence/Parameters/Update", "POST", Summary = "更新参数")]
    public class UpdateParameterRequest : IReturn<ParameterResponse>
    {
        [ApiMember(Name = "Namespace", Description = "命名空间（单个更新）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Namespace { get; set; }

        [ApiMember(Name = "Key", Description = "参数键名（单个更新）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Key { get; set; }

        [ApiMember(Name = "Value", Description = "参数值（单个更新）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Value { get; set; }

        [ApiMember(Name = "Description", Description = "参数描述（单个更新）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Description { get; set; }

        [ApiMember(Name = "Parameters", Description = "参数列表（批量更新）", IsRequired = false, DataType = "array", ParameterType = "body")]
        public List<ParameterItem> Parameters { get; set; }
    }

    /// <summary>
    /// 删除参数接口（统一单个和批量）
    /// </summary>
    [Route("/ParameterPersistence/Parameters/Delete", "POST", Summary = "删除参数")]
    public class DeleteParameterRequest : IReturn<ParameterResponse>
    {
        [ApiMember(Name = "Namespace", Description = "命名空间（单个删除）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Namespace { get; set; }

        [ApiMember(Name = "Key", Description = "参数键名（单个删除）", IsRequired = false, DataType = "string", ParameterType = "body")]
        public string Key { get; set; }

        [ApiMember(Name = "Keys", Description = "参数键名列表（批量删除-同一命名空间）", IsRequired = false, DataType = "array", ParameterType = "body")]
        public List<string> Keys { get; set; }

        [ApiMember(Name = "Parameters", Description = "参数列表（批量删除-不同命名空间）", IsRequired = false, DataType = "array", ParameterType = "body")]
        public List<ParameterItem> Parameters { get; set; }
    }

    /// <summary>
    /// 参数项（用于批量操作）
    /// </summary>
    public class ParameterItem
    {
        public string Namespace { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 参数持久化API服务
    /// </summary>
    public class ParameterController : IService
    {
        private readonly IParameterStorageService _storageService;
        private readonly ILogger _logger;
        private readonly IAuthorizationContext _authContext;

        public ParameterController(
            IParameterStorageService storageService,
            ILogManager logManager,
            IAuthorizationContext authContext)
        {
            _storageService = storageService;
            _logger = logManager.GetLogger(Plugin.Instance.Name);
            _authContext = authContext;
        }

        /// <summary>
        /// 验证Token和管理员权限
        /// </summary>
        private void ValidateAuthentication(IRequest request)
        {
            var auth = _authContext.GetAuthorizationInfo(request);
            
            if (auth == null || string.IsNullOrEmpty(auth.Token))
            {
                throw new SecurityException("未提供有效的Token");
            }

            if (!auth.User.Policy.IsAdministrator)
            {
                throw new SecurityException("需要管理员权限");
            }
        }

        /// <summary>
        /// GET /ParameterPersistence/Parameters
        /// 获取参数列表或搜索参数
        /// </summary>
        public async Task<object> Get(ParameterRequest request)
        {
            try
            {
                ValidateAuthentication(Request);

                List<ParameterModel> parameters;

                // 搜索功能
                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    parameters = await _storageService.SearchParametersAsync(request.Keyword);
                }
                // 按命名空间筛选
                else if (!string.IsNullOrEmpty(request.Namespace))
                {
                    parameters = await _storageService.GetParametersByNamespaceAsync(request.Namespace);
                }
                // 获取所有参数
                else
                {
                    parameters = await _storageService.GetAllParametersAsync();
                }

                return new ParameterResponse
                {
                    Success = true,
                    DataList = parameters,
                    Total = parameters.Count
                };
            }
            catch (SecurityException ex)
            {
                _logger.Error($"权限验证失败: {ex.Message}");
                return new ParameterResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"获取参数失败: {ex.Message}");
                return new ParameterResponse
                {
                    Success = false,
                    Message = $"获取参数失败: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// POST /ParameterPersistence/Parameters/Query
        /// 查询参数（统一单个和批量）
        /// </summary>
        public async Task<object> Post(QueryParameterRequest request)
        {
            try
            {
                ValidateAuthentication(Request);

                // 单个查询
                if (!string.IsNullOrEmpty(request.Key))
                {
                    var nameSpace = string.IsNullOrEmpty(request.Namespace) ? "default" : request.Namespace;
                    var parameter = await _storageService.GetParameterAsync(nameSpace, request.Key);

                    if (parameter == null)
                    {
                        return new ParameterResponse
                        {
                            Success = false,
                            Message = "参数不存在"
                        };
                    }

                    return new ParameterResponse
                    {
                        Success = true,
                        Data = parameter
                    };
                }
                // 批量查询
                else if (request.Keys != null && request.Keys.Count > 0)
                {
                    var nameSpace = string.IsNullOrEmpty(request.Namespace) ? "default" : request.Namespace;
                    var results = new List<ParameterModel>();

                    foreach (var key in request.Keys)
                    {
                        var parameter = await _storageService.GetParameterAsync(nameSpace, key);
                        if (parameter != null)
                        {
                            results.Add(parameter);
                        }
                    }

                    return new ParameterResponse
                    {
                        Success = true,
                        DataList = results,
                        Total = results.Count
                    };
                }
                // 查询整个命名空间
                else if (!string.IsNullOrEmpty(request.Namespace))
                {
                    var parameters = await _storageService.GetParametersByNamespaceAsync(request.Namespace);

                    return new ParameterResponse
                    {
                        Success = true,
                        DataList = parameters,
                        Total = parameters.Count
                    };
                }
                else
                {
                    return new ParameterResponse
                    {
                        Success = false,
                        Message = "请提供 Key、Keys 或 Namespace 参数"
                    };
                }
            }
            catch (SecurityException ex)
            {
                _logger.Error($"权限验证失败: {ex.Message}");
                return new ParameterResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"查询参数失败: {ex.Message}");
                return new ParameterResponse
                {
                    Success = false,
                    Message = $"查询参数失败: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// POST /ParameterPersistence/Parameters/Create
        /// 创建参数（统一单个和批量）
        /// </summary>
        public async Task<object> Post(CreateParameterRequest request)
        {
            try
            {
                ValidateAuthentication(Request);

                // 批量创建
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

                            var nameSpace = string.IsNullOrEmpty(item.Namespace) ? "default" : item.Namespace;

                            var parameter = new ParameterModel
                            {
                                Namespace = nameSpace,
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
                    if (errors.Count > 0)
                    {
                        message += $"，失败 {errors.Count} 个";
                    }

                    return new ParameterResponse
                    {
                        Success = errors.Count < request.Parameters.Count,
                        DataList = results,
                        Total = results.Count,
                        Message = message
                    };
                }
                // 单个创建
                else if (!string.IsNullOrEmpty(request.Key))
                {
                    var nameSpace = string.IsNullOrEmpty(request.Namespace) ? "default" : request.Namespace;

                    var parameter = new ParameterModel
                    {
                        Namespace = nameSpace,
                        Key = request.Key,
                        Value = request.Value ?? "",
                        Type = request.Type ?? "string",
                        Description = request.Description
                    };

                    var result = await _storageService.CreateParameterAsync(parameter);

                    return new ParameterResponse
                    {
                        Success = true,
                        Data = result,
                        Message = "参数创建成功"
                    };
                }
                else
                {
                    return new ParameterResponse
                    {
                        Success = false,
                        Message = "请提供 Key 或 Parameters 参数"
                    };
                }
            }
            catch (SecurityException ex)
            {
                _logger.Error($"权限验证失败: {ex.Message}");
                return new ParameterResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"创建参数失败: {ex.Message}");
                return new ParameterResponse
                {
                    Success = false,
                    Message = $"创建参数失败: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// POST /ParameterPersistence/Parameters/Update
        /// 更新参数（统一单个和批量）
        /// </summary>
        public async Task<object> Post(UpdateParameterRequest request)
        {
            try
            {
                ValidateAuthentication(Request);

                // 批量更新
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

                            var nameSpace = string.IsNullOrEmpty(item.Namespace) ? "default" : item.Namespace;

                            var result = await _storageService.UpdateParameterAsync(
                                nameSpace,
                                item.Key,
                                item.Value ?? "",
                                item.Description);

                            results.Add(result);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"更新参数 {item.Key} 失败: {ex.Message}");
                            _logger.Error($"批量更新参数失败 - Key: {item.Key}, Error: {ex.Message}");
                        }
                    }

                    var message = $"成功更新 {results.Count} 个参数";
                    if (errors.Count > 0)
                    {
                        message += $"，失败 {errors.Count} 个";
                    }

                    return new ParameterResponse
                    {
                        Success = errors.Count < request.Parameters.Count,
                        DataList = results,
                        Total = results.Count,
                        Message = message
                    };
                }
                // 单个更新
                else if (!string.IsNullOrEmpty(request.Key))
                {
                    var nameSpace = string.IsNullOrEmpty(request.Namespace) ? "default" : request.Namespace;

                    var result = await _storageService.UpdateParameterAsync(
                        nameSpace,
                        request.Key,
                        request.Value ?? "",
                        request.Description);

                    return new ParameterResponse
                    {
                        Success = true,
                        Data = result,
                        Message = "参数更新成功"
                    };
                }
                else
                {
                    return new ParameterResponse
                    {
                        Success = false,
                        Message = "请提供 Key 或 Parameters 参数"
                    };
                }
            }
            catch (SecurityException ex)
            {
                _logger.Error($"权限验证失败: {ex.Message}");
                return new ParameterResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"更新参数失败: {ex.Message}");
                return new ParameterResponse
                {
                    Success = false,
                    Message = $"更新参数失败: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// POST /ParameterPersistence/Parameters/Delete
        /// 删除参数（统一单个和批量）
        /// </summary>
        public async Task<object> Post(DeleteParameterRequest request)
        {
            try
            {
                ValidateAuthentication(Request);

                // 批量删除（完整参数列表）
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

                            var nameSpace = string.IsNullOrEmpty(item.Namespace) ? "default" : item.Namespace;
                            var result = await _storageService.DeleteParameterAsync(nameSpace, item.Key);

                            if (result)
                            {
                                successCount++;
                            }
                            else
                            {
                                errors.Add($"参数 {item.Key} 不存在");
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"删除参数 {item.Key} 失败: {ex.Message}");
                            _logger.Error($"批量删除参数失败 - Key: {item.Key}, Error: {ex.Message}");
                        }
                    }

                    var message = $"成功删除 {successCount} 个参数";
                    if (errors.Count > 0)
                    {
                        message += $"，失败 {errors.Count} 个";
                    }

                    return new ParameterResponse
                    {
                        Success = successCount > 0,
                        Total = successCount,
                        Message = message
                    };
                }
                // 批量删除（简化方式-同一命名空间）
                else if (request.Keys != null && request.Keys.Count > 0)
                {
                    var nameSpace = string.IsNullOrEmpty(request.Namespace) ? "default" : request.Namespace;
                    var successCount = 0;

                    foreach (var key in request.Keys)
                    {
                        var result = await _storageService.DeleteParameterAsync(nameSpace, key);
                        if (result)
                        {
                            successCount++;
                        }
                    }

                    return new ParameterResponse
                    {
                        Success = successCount > 0,
                        Total = successCount,
                        Message = $"成功删除 {successCount} 个参数"
                    };
                }
                // 单个删除
                else if (!string.IsNullOrEmpty(request.Key))
                {
                    var nameSpace = string.IsNullOrEmpty(request.Namespace) ? "default" : request.Namespace;
                    var result = await _storageService.DeleteParameterAsync(nameSpace, request.Key);

                    return new ParameterResponse
                    {
                        Success = result,
                        Message = result ? "参数删除成功" : "参数不存在"
                    };
                }
                else
                {
                    return new ParameterResponse
                    {
                        Success = false,
                        Message = "请提供 Key、Keys 或 Parameters 参数"
                    };
                }
            }
            catch (SecurityException ex)
            {
                _logger.Error($"权限验证失败: {ex.Message}");
                return new ParameterResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"删除参数失败: {ex.Message}");
                return new ParameterResponse
                {
                    Success = false,
                    Message = $"删除参数失败: {ex.Message}"
                };
            }
        }

        private IRequest RequestContext => Request;
        public IRequest Request { get; set; }
    }
}

