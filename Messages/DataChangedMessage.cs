namespace zuoleme.Messages
{
    /// <summary>
    /// 数据变更消息
    /// </summary>
    public class DataChangedMessage
    {
        public DataChangeType ChangeType { get; set; }
        public string? Description { get; set; }

        public DataChangedMessage(DataChangeType changeType, string? description = null)
        {
            ChangeType = changeType;
            Description = description;
        }
    }

    /// <summary>
    /// 数据变更类型
    /// </summary>
    public enum DataChangeType
    {
        /// <summary>
        /// 添加记录
        /// </summary>
        RecordAdded,
        
        /// <summary>
        /// 删除记录
        /// </summary>
        RecordDeleted,
        
        /// <summary>
        /// 清空所有数据
        /// </summary>
        AllDataCleared,
        
        /// <summary>
        /// 导入数据
        /// </summary>
        DataImported,
        
        /// <summary>
        /// 设置变更
        /// </summary>
        SettingsChanged
    }
}
