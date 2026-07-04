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
        /// 新增记录
        /// </summary>
        RecordAdded,

        /// <summary>
        /// 删除记录
        /// </summary>
        RecordDeleted,

        /// <summary>
        /// 更新记录（如备注）
        /// </summary>
        RecordUpdated,

        /// <summary>
        /// 清空全部数据
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
