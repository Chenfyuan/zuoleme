# 数据同步刷新功能实现

## ? 问题解决

### 问题描述
清空数据后，切换到统计页面和首页，依旧还有记录显示，数据没有实时更新。

### 根本原因
所有页面（HomePage、StatsPage）共享同一个 `MainViewModel` 单例实例，当在设置页面清空数据后：
1. `RecordService` 中的数据已清空
2. 但 `MainViewModel` 中的 `ObservableCollection` 仍然保留旧数据
3. 没有机制通知 ViewModel 重新加载数据

## ?? 解决方案

### 使用消息通知机制

采用 **CommunityToolkit.Mvvm** 的 `WeakReferenceMessenger` 实现跨组件通信。

#### 架构流程
```
RecordService (数据变更)
    ↓ 发送消息
WeakReferenceMessenger
    ↓ 广播
MainViewModel (订阅者)
    ↓ 收到消息
自动刷新数据
    ↓ 更新UI
HomePage / StatsPage
```

## ?? 新增依赖

### NuGet 包
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
```

### 功能
- `WeakReferenceMessenger` - 消息总线
- 弱引用设计，避免内存泄漏
- 支持强类型消息

## ?? 实现细节

### 1. 创建消息类

**Messages/DataChangedMessage.cs**

```csharp
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

public enum DataChangeType
{
    RecordAdded,      // 添加记录
    RecordDeleted,    // 删除记录
    AllDataCleared,   // 清空所有数据
    DataImported,     // 导入数据
    SettingsChanged   // 设置变更
}
```

### 2. RecordService 发送消息

```csharp
using CommunityToolkit.Mvvm.Messaging;

public void AddRecord(Record record)
{
    _records.Add(record);
    SaveRecords();
    SendDataChangedMessage(DataChangeType.RecordAdded); // 发送消息
}

public void DeleteRecord(Guid id)
{
    var record = _records.FirstOrDefault(r => r.Id == id);
    if (record != null)
    {
        _records.Remove(record);
        SaveRecords();
        SendDataChangedMessage(DataChangeType.RecordDeleted); // 发送消息
    }
}

public void ClearAllRecords()
{
    _records.Clear();
    SaveRecords();
    SendDataChangedMessage(DataChangeType.AllDataCleared); // 发送消息
}

private void SendDataChangedMessage(DataChangeType changeType)
{
    try
    {
        WeakReferenceMessenger.Default.Send(new DataChangedMessage(changeType));
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"发送数据变更消息失败: {ex.Message}");
    }
}
```

### 3. MainViewModel 订阅消息

```csharp
using CommunityToolkit.Mvvm.Messaging;

public MainViewModel(RecordService recordService, HealthService healthService)
{
    _recordService = recordService;
    _healthService = healthService;
    // ...
    
    LoadData();
    SubscribeToDataChanges(); // 订阅消息
}

private void SubscribeToDataChanges()
{
    WeakReferenceMessenger.Default.Register<DataChangedMessage>(this, (recipient, message) =>
    {
        System.Diagnostics.Debug.WriteLine($"收到数据变更消息: {message.ChangeType}");
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LoadData(); // 自动刷新数据
        });
    });
}
```

**关键点**：
- 使用 `WeakReferenceMessenger.Default` 全局实例
- 订阅者是 `this`（MainViewModel 实例）
- 收到消息后在主线程刷新数据
- 弱引用确保 ViewModel 可以正常垃圾回收

### 4. 命令中移除手动刷新

```csharp
private void AddRecord()
{
    try
    {
        var record = new Record();
        _recordService.AddRecord(record);
        // LoadData() 不再需要手动调用
        // 会通过 WeakReferenceMessenger 自动刷新
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"添加记录失败: {ex.Message}");
    }
}

private void DeleteRecord(Guid id)
{
    try
    {
        _recordService.DeleteRecord(id);
        // LoadData() 会自动调用
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"删除记录失败: {ex.Message}");
    }
}
```

## ?? 工作流程

### 场景 1: 清空数据

1. **用户操作**: 在设置页点击"清空所有数据"
2. **确认对话框**: 用户确认
3. **RecordService**: `ClearAllRecords()` 清空数据
4. **发送消息**: `WeakReferenceMessenger.Send(DataChangedMessage(AllDataCleared))`
5. **MainViewModel**: 收到消息，调用 `LoadData()`
6. **UI 更新**: 
   - HomePage 统计卡片显示 0
   - HomePage 记录列表清空
   - StatsPage 所有统计清零

### 场景 2: 添加记录

1. **用户操作**: 点击"来一发"按钮
2. **MainViewModel**: `AddRecordCommand` 执行
3. **RecordService**: `AddRecord()` 添加记录
4. **发送消息**: `WeakReferenceMessenger.Send(DataChangedMessage(RecordAdded))`
5. **MainViewModel**: 收到消息，重新加载数据
6. **UI 更新**: 所有页面数据同步更新

### 场景 3: 删除记录

1. **用户操作**: 滑动删除记录
2. **MainViewModel**: `DeleteRecordCommand` 执行
3. **RecordService**: `DeleteRecord()` 删除记录
4. **发送消息**: `WeakReferenceMessenger.Send(DataChangedMessage(RecordDeleted))`
5. **MainViewModel**: 收到消息，刷新数据
6. **UI 更新**: 所有页面同步

## ?? 优势

### 1. 解耦合
- Service 层不需要知道 ViewModel
- ViewModel 不需要主动轮询数据
- 组件之间通过消息松耦合

### 2. 自动同步
- 任何数据变更都会自动通知所有订阅者
- 不需要手动管理刷新逻辑
- 避免忘记刷新导致的不一致

### 3. 性能优化
- 使用弱引用，避免内存泄漏
- 只有数据变更时才刷新，不会频繁更新
- 主线程调用确保 UI 线程安全

### 4. 可扩展
- 轻松添加新的数据变更类型
- 其他组件可以订阅相同消息
- 支持消息过滤和条件处理

## ?? 调试信息

### 控制台输出
```
收到数据变更消息: AllDataCleared
所有记录已清空
```

可以通过调试输出验证消息是否正常发送和接收。

## ?? 数据流图

```
┌─────────────────┐
│  SettingsPage   │
│  (清空数据)      │
└────────┬────────┘
         │ ClearDataCommand
         ↓
┌─────────────────┐
│ SettingsViewModel│
└────────┬────────┘
         │ _recordService.ClearAllRecords()
         ↓
┌─────────────────┐
│  RecordService  │
│  清空 _records   │
│  保存文件        │
└────────┬────────┘
         │ WeakReferenceMessenger.Send()
         ↓
┌─────────────────────────────────┐
│   WeakReferenceMessenger         │
│   (全局消息总线)                  │
└────────┬────────────────────────┘
         │ 广播 DataChangedMessage
         ↓
┌─────────────────┐
│  MainViewModel  │
│  (订阅者)        │
└────────┬────────┘
         │ LoadData()
         ↓
┌─────────────────┐     ┌─────────────────┐
│   HomePage      │     │   StatsPage     │
│   更新 UI        │     │   更新 UI        │
└─────────────────┘     └─────────────────┘
```

## ? 测试结果

### 测试步骤
1. ? 添加几条记录
2. ? 切换到设置页
3. ? 点击"清空所有数据"
4. ? 确认清空
5. ? 切换到主页 → 数据为 0
6. ? 切换到统计页 → 所有统计为 0
7. ? 添加新记录 → 所有页面同步更新

### 预期行为
- 数据清空后，所有页面立即同步
- 不需要手动刷新或重新进入页面
- 统计数字正确显示为 0
- 记录列表正确显示为空

## ?? 总结

通过引入 `CommunityToolkit.Mvvm` 的消息机制，成功实现了：
- ? 跨页面数据自动同步
- ? 清空数据后实时更新UI
- ? 解耦的架构设计
- ? 内存安全的弱引用

**现在数据变更会立即反映到所有页面！** ??
