# 清空数据功能实现

## ? 功能完成

已成功实现设置页面的"清空所有数据"功能，包含完整的确认流程和用户反馈。

## ?? 实现内容

### 1. RecordService 更新

添加了清空所有记录的方法：

```csharp
public void ClearAllRecords()
{
    try
    {
        _records.Clear();
        SaveRecords();
        System.Diagnostics.Debug.WriteLine("所有记录已清空");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"清空记录失败: {ex.Message}");
        throw;
    }
}

public int GetRecordCount()
{
    return _records.Count;
}
```

**功能说明**：
- `ClearAllRecords()` - 清空内存中的所有记录并保存到文件
- `GetRecordCount()` - 获取当前记录总数（用于确认对话框）

### 2. SettingsViewModel 更新

#### 添加依赖注入
```csharp
private readonly RecordService _recordService;

public SettingsViewModel(HealthService healthService, RecordService recordService)
{
    _healthService = healthService;
    _recordService = recordService;
    // ...
}
```

#### 添加清空数据命令
```csharp
public ICommand ClearDataCommand { get; }

ClearDataCommand = new Command(async () => await ClearAllData());
```

#### 实现清空数据逻辑
```csharp
private async Task ClearAllData()
{
    try
    {
        var recordCount = _recordService.GetRecordCount();
        
        // 1. 检查是否有数据
        if (recordCount == 0)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "提示", 
                "暂无数据需要清空", 
                "确定");
            return;
        }

        // 2. 确认对话框
        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            "确认清空数据",
            $"确定要清空所有记录吗？\n\n当前共有 {recordCount} 条记录\n此操作不可撤销！",
            "清空",
            "取消");

        // 3. 执行清空
        if (confirm)
        {
            _recordService.ClearAllRecords();
            
            // 4. 成功反馈
            await Application.Current.MainPage.DisplayAlert(
                "成功",
                "所有记录已清空",
                "确定");
        }
    }
    catch (Exception ex)
    {
        // 5. 错误处理
        await Application.Current!.MainPage!.DisplayAlert(
            "错误",
            "清空数据失败，请重试",
            "确定");
    }
}
```

### 3. SettingsPage.xaml 更新

#### 替换 Emoji 为字体图标
- ?? → `MaterialIcons.Settings` (设置图标)
- ?? → `MaterialIcons.Favorite` (爱心图标)
- ?? → `MaterialIcons.Save` (保存图标)
- ?? → `MaterialIcons.Download` (下载图标)
- ??? → `MaterialIcons.DeleteForever` (删除图标)
- ?? → `MaterialIcons.Lock` (锁图标)
- ?? → `MaterialIcons.Book` (书本图标)
- ?? → `MaterialIcons.Notifications` (通知图标)
- ?? → `MaterialIcons.Info` (信息图标)
- ?? → `MaterialIcons.Info` (信息图标)
- ? → `MaterialIcons.ChevronRight` (右箭头图标)

#### 添加清空数据点击事件
```xaml
<!-- Clear All Data -->
<Grid Padding="16" ColumnDefinitions="Auto,*,Auto">
    <Grid.GestureRecognizers>
        <TapGestureRecognizer Command="{Binding ClearDataCommand}"/>
    </Grid.GestureRecognizers>
    <Label Grid.Column="0" 
           Text="{x:Static constants:MaterialIcons.DeleteForever}" 
           FontFamily="MaterialIcons"
           FontSize="24" 
           TextColor="#EF5350"/>
    <Label Grid.Column="1" 
           Text="清空所有数据" 
           FontSize="16" 
           TextColor="#EF5350" 
           FontAttributes="Bold"/>
    <Label Grid.Column="2" 
           Text="{x:Static constants:MaterialIcons.ChevronRight}" 
           FontFamily="MaterialIcons"
           FontSize="28" 
           TextColor="{DynamicResource Gray400}"/>
</Grid>
```

## ?? 用户交互流程

### 步骤 1: 点击"清空所有数据"
用户在设置页面点击红色的"清空所有数据"选项

### 步骤 2: 显示确认对话框
```
标题: 确认清空数据
内容: 确定要清空所有记录吗？
      
      当前共有 X 条记录
      此操作不可撤销！
      
按钮: [清空] [取消]
```

### 步骤 3a: 用户确认
- 执行清空操作
- 显示成功提示：
  ```
  标题: 成功
  内容: 所有记录已清空
  按钮: [确定]
  ```

### 步骤 3b: 用户取消
- 关闭对话框
- 不执行任何操作
- 数据保持不变

### 特殊情况: 没有数据
如果当前没有任何记录，显示提示：
```
标题: 提示
内容: 暂无数据需要清空
按钮: [确定]
```

## ??? 安全特性

1. **二次确认** - 显示确认对话框，避免误操作
2. **数据统计** - 显示将要删除的记录数量
3. **明确提示** - "此操作不可撤销"警告文字
4. **醒目设计** - 红色文字和图标，提醒用户注意
5. **错误处理** - 捕获并提示操作失败
6. **空数据检查** - 没有数据时给予友好提示

## ?? 视觉效果

### 清空数据选项样式
- **图标**: ??? DeleteForever (红色)
- **文字**: "清空所有数据" (红色、加粗)
- **位置**: 数据管理区域最后一项
- **分隔**: 上方有分隔线

### Material Design 风格
- 使用 Material Icons 字体图标
- 红色 (#EF5350) 表示危险操作
- 加粗文字增加视觉重量
- 圆角卡片设计

## ?? 数据同步

清空数据后，其他页面会自动更新：
- **主页**: 统计卡片显示 0
- **主页**: 最近记录列表为空
- **统计页**: 所有统计清零
- **日历页**: 所有日期无记录标记

**注意**: 清空后需要切换到其他页面再回来才能看到更新（或者添加刷新机制）

## ?? 后续优化建议

### 1. 添加数据备份提示
```csharp
bool confirm = await DisplayAlert(
    "确认清空数据",
    "确定要清空所有记录吗？\n\n建议先导出数据备份\n此操作不可撤销！",
    "我已备份，继续清空",
    "取消");
```

### 2. 集成导出功能
在确认对话框中添加"导出备份"按钮：
```csharp
var action = await DisplayActionSheet(
    "清空所有数据",
    "取消",
    null,
    "先导出备份再清空",
    "直接清空");
```

### 3. 添加撤销功能
实现软删除或临时回收站：
```csharp
// 移动到回收站而不是直接删除
_recordService.MoveToTrash(_records);
// 30天后自动永久删除
```

### 4. 统计信息展示
清空前显示更多统计：
```
总记录数: 150 条
最早记录: 2024年1月1日
最近记录: 2024年12月31日
数据时长: 365 天
```

## ? 测试清单

- [x] 有数据时点击清空
- [x] 无数据时点击清空
- [x] 确认对话框正确显示记录数
- [x] 点击"取消"不清空数据
- [x] 点击"清空"成功清空数据
- [x] 清空后显示成功提示
- [x] 错误时显示错误提示
- [x] 图标正确显示
- [x] 红色警告色正确应用
- [ ] 清空后其他页面自动刷新（待优化）

## ?? 功能完成

清空数据功能已完全实现，包括：
- ? 安全的二次确认机制
- ? 友好的用户提示
- ? 完善的错误处理
- ? Material Design 图标
- ? 醒目的红色警告设计

**现在可以安全地清空所有记录数据了！** ??
