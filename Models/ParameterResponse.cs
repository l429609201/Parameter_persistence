using System.Collections.Generic;

namespace Emby.ParameterPersistence.Models
{
    /// <summary>
    /// API响应模型
    /// </summary>
    public class ParameterResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 单个数据
        /// </summary>
        public ParameterModel Data { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<ParameterModel> DataList { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public int Total { get; set; }

        public ParameterResponse()
        {
            Success = true;
            DataList = new List<ParameterModel>();
        }
    }

    /// <summary>
    /// 批量操作请求模型
    /// </summary>
    public class BatchOperationRequest
    {
        public List<OperationItem> Operations { get; set; }

        public BatchOperationRequest()
        {
            Operations = new List<OperationItem>();
        }
    }

    /// <summary>
    /// 操作项
    /// </summary>
    public class OperationItem
    {
        public string Action { get; set; } // create, update, delete
        public string Namespace { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}

