# 做了么 APP

做了么APP，支持android、ios、windows等，记录生活点滴。

## 📱 应用截图

### 首页展示
<table>
  <tr>
    <td><img src="pic/正常.png" width="200"/><br/><center>首页 - 正常状态</center></td>
    <td><img src="pic/普通.png" width="200"/><br/><center>首页 - 普通状态</center></td>
    <td><img src="pic/高频.png" width="200"/><br/><center>首页 - 高频状态</center></td>
    <td><img src="pic/灰飞烟灭.png" width="200"/><br/><center>首页 - 灰飞烟灭状态</center></td>
  </tr>
</table>

### 其他页面
<table>
  <tr>
    <td><img src="pic/统计.png" width="200"/><br/><center>统计页面</center></td>
    <td><img src="pic/日历.png" width="200"/><br/><center>日历视图</center></td>
    <td><img src="pic/设置.png" width="200"/><br/><center>设置页面</center></td>
    <td><img src="pic/图标.png" width="200"/><br/><center>应用图标</center></td>
  </tr>
</table>

## 功能特点

### 核心功能
-  **一键记录**：点击主按钮即可添加记录
-  **统计展示**：实时显示今日、本周、本月和总计数量
- **历史记录**：查看所有记录的详细时间
-  **滑动删除**：左滑记录即可删除
- **数据持久化**：所有数据保存在本地，保护隐私
-  **现代化设计**：使用 .NET MAUI 原生控件实现的现代化设计
- **底部导航**：快速标签页，首页、统计、设置，快速切换

## 技术栈

- **.NET MAUI**：微软最新的跨平台框架
- **MVVM 架构**：Model-View-ViewModel 设计模式
- **依赖注入**：使用 DI 容器管理服务
- **数据持久化**：JSON 文件存储
- **现代 UI**：Border、Shadow、RoundRectangle 等现代控件
- **Shell 导航**：使用 MAUI Shell 实现底部 TabBar 导航

## UI 设计亮点

### 现代化设计元素
- **阴影效果**：卡片和按钮具有精细的阴影
- **圆角设计**：所有卡片和按钮使用圆角矩形
- **Emoji 图标**：使用 Unicode Emoji 作为视觉元素
- **响应式布局**：自适应不同屏幕尺寸
- **Material 配色**：遵循 Material Design 规范的配色方案
- **底部导航栏**：便捷的页面切换体验

## 项目结构

```
zuoleme/
├── Models/              # 数据模型
│   └── Record.cs       # 记录模型
├── Services/           # 业务服务
│   └── RecordService.cs # 记录数据服务
├── ViewModels/         # 视图模型
│   └── MainViewModel.cs # 主视图模型
├── Views/              # 页面视图
│   └── HomePage.xaml   # 主页（记录）
│   └── StatsPage.xaml  # 统计页
│   └── SettingsPage.xaml # 设置页
├── AppShell.xaml       # Shell 导航定义
└── Resources/          # 资源文件
    └── Styles/         # 样式和颜色
```


## 运行要求

- **.NET 10**
- **iOS**：15.0+
- **Android**：API 21+
- **Windows**：Windows 10 (17763+)
- **macOS**：macOS 15.0+ (Catalyst)

## 构建命令

### Windows
```bash
dotnet build -f net10.0-windows10.0.19041.0
dotnet run -f net10.0-windows10.0.19041.0
```

### Android
```bash
dotnet build -f net10.0-android
dotnet run -f net10.0-android
```

### iOS
```bash
dotnet build -f net10.0-ios
dotnet run -f net10.0-ios
```

## UI 控件展示

### 使用的 .NET MAUI 控件
1. **Shell** - 应用导航和底部 TabBar
2. **TabBar** - 底部标签栏导航
3. **Border** - 带阴影和圆角的边框容器
4. **Shadow** - 阴影效果
5. **RoundRectangle** - 圆角矩形形状
6. **SwipeView** - 滑动操作视图
7. **CollectionView** - 可滚动列表
8. **Grid** - 网格布局



## 未来计划

- [ ] 添加备注功能
- [ ] 数据导出和导入功能
- [ ] 统计图表可视化
- [ ] 提醒功能
- [ ] 深色模式
- [ ] 密码保护隐私实现
- [ ] 云同步数据选项
- [ ] 更多统计维度（获取更远期的时间段）
- [ ] 数据可视化（热力图、折线统计图）
- [ ] 更多数据管理实现
- [ ] 使用说明页面
- [ ] 版本更新记录

## 技术亮点
 **🎨 .NET MAUI** - 完全基于最新 UI 框架  
**Shell 导航** - 使用 MAUI Shell 实现现代导航  
 **现代设计** - 使用最新的 MAUI 控件（Border、Shadow 等）  
 **多页架构** - 首页、统计、设置多功能分离  
 **跨平台一致** - 多平台统一的视觉体验  
 **流畅高效** - 无冗余代码，响应流畅  
 **易于维护** - 代码清晰，结构清晰  
 **MVVM 模式** - 数据绑定和命令模式  

## 隐私声明

本应用所有数据仅存储在您的设备本地，不会上传到任何服务器。请放心添加和管理您的隐私。
