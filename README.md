# 做了么 APP

一款跨平台的私密记录应用，支持 iOS、Android、Windows 等多个平台。

## 功能特点

### 第一版功能
- ? **一键记录**：点击中央大按钮快速添加记录
- ?? **统计展示**：实时显示今日、本周、本月和总计数据
- ?? **历史记录**：查看所有记录的详细时间
- ??? **滑动删除**：左滑记录项可以删除
- ?? **数据持久化**：所有数据保存在本地，保护隐私
- ?? **现代化界面**：使用 .NET MAUI 原生控件实现的现代化界面
- ?? **底部导航**：三个标签页（主页、统计、设置）轻松切换

## 技术栈

- **.NET MAUI**：微软最新的跨平台框架
- **MVVM 架构**：Model-View-ViewModel 设计模式
- **依赖注入**：使用 DI 容器管理服务
- **数据持久化**：JSON 文件存储
- **现代 UI**：Border、Shadow、RoundRectangle 等现代控件
- **Shell 导航**：使用 MAUI Shell 实现底部 TabBar 导航

## UI 设计特性

### 现代化设计元素
- ?? **阴影效果**：卡片和按钮带有精美的阴影
- ?? **圆角设计**：所有卡片和按钮使用圆角矩形
- ?? **Emoji 图标**：使用 Unicode Emoji 作为视觉元素
- ?? **响应式布局**：完美适配各种屏幕尺寸
- ?? **Material 配色**：符合 Material Design 规范的配色方案
- ?? **底部导航栏**：便捷的三页切换导航

## 项目结构

```
zuoleme/
├── Models/              # 数据模型
│   └── Record.cs       # 记录模型
├── Services/           # 业务服务
│   └── RecordService.cs # 记录管理服务
├── ViewModels/         # 视图模型
│   └── MainViewModel.cs # 主视图模型
├── Views/              # 页面视图
│   ├── HomePage.xaml   # 主页（记录）
│   ├── StatsPage.xaml  # 统计页
│   └── SettingsPage.xaml # 设置页
├── AppShell.xaml       # Shell 导航配置
└── Resources/          # 资源文件
    └── Styles/         # 样式和颜色
```

## 界面说明

### ?? 主页（HomePage）
- **顶部标题**：带心形 Emoji 的"做了么"标题
- **浮动操作按钮（FAB）**：140x140 粉色圆形按钮，带阴影效果
- **快速统计**：今日和总计两个卡片
- **最近记录**：显示最近 5 条记录
- **快速操作**：点击按钮添加记录，左滑删除

### ?? 统计页（StatsPage）
- **统计卡片**：四个精美卡片，分别显示：
  - ?? **今日**：带日历 Emoji，粉色主题
  - ?? **本周**：带图表 Emoji，紫色主题  
  - ?? **本月**：带日历 Emoji，橙色主题
  - ?? **总计**：带趋势 Emoji，深灰色主题
- **所有记录列表**：查看和管理所有记录
- **滑动删除**：左滑删除记录

### ?? 设置页（SettingsPage）
- **应用信息**：显示应用名称和版本
- **数据管理**：
  - ?? 导出数据（待实现）
  - ?? 导入数据（待实现）
  - ??? 清空所有数据（待实现）
- **隐私与安全**：
  - ?? 密码保护（待实现）
- **关于**：
  - ?? 使用说明（待实现）
  - ?? 版本更新（待实现）
  - ?? 隐私政策（待实现）

### 交互操作
- **添加记录**：在主页点击中央浮动操作按钮（FAB）
- **删除记录**：在记录项上向左滑动，点击"删除"
- **切换页面**：点击底部导航栏的图标

## 数据存储

所有数据存储在应用的私有目录中：
- **文件位置**：`FileSystem.AppDataDirectory/records.json`
- **格式**：JSON
- **隐私保护**：数据仅存储在本地设备，不上传云端

## Material Design 配色方案

### 主题色
- **Primary（主色）**：#E91E63 (Material Pink)
- **Secondary（次要色）**：#9C27B0 (Material Purple)
- **Tertiary（第三色）**：#FF6F00 (Material Orange)

### 灰度色
- **背景色**：#FAFAFA (Gray 50)
- **卡片背景**：#FFFFFF (White)
- **文字颜色**：#212121 - #757575 (Gray 900 - Gray 600)

## NuGet 包

```xml
<PackageReference Include="Microsoft.Maui.Controls" Version="10.0.11" />
<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="10.0.0" />
```

## 运行要求

- **.NET 10**
- **iOS**：15.0+
- **Android**：API 21+
- **Windows**：Windows 10 (17763+)
- **macOS**：macOS 15.0+ (Catalyst)

## 如何运行

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

## UI 组件展示

### 使用的 .NET MAUI 控件
1. **Shell** - 应用导航和底部 TabBar
2. **TabBar** - 底部标签栏导航
3. **Border** - 带阴影和圆角的边框容器
4. **Shadow** - 阴影效果
5. **RoundRectangle** - 圆角矩形形状
6. **SwipeView** - 滑动操作视图
7. **CollectionView** - 高性能列表
8. **Grid** - 网格布局

### Emoji 图标
- ?? - 心形（主题图标）
- ?? - 房子（主页）
- ?? - 图表（统计）
- ?? - 齿轮（设置）
- ?? - 日历（今日）
- ?? - 日历（本月）
- ?? - 趋势图（总计）
- ?? - 卷轴（历史）
- ? - 闹钟（时间）
- ?? - 软盘（导出）
- ?? - 下载（导入）
- ??? - 垃圾桶（删除）
- ?? - 锁（密码）
- ?? - 书（说明）
- ?? - 铃铛（更新）
- ?? - 信息（政策）
- ? - 右箭头（导航）

## 未来计划

- [ ] 添加备注功能
- [ ] 数据导出和导入功能
- [ ] 统计图表可视化
- [ ] 提醒功能
- [ ] 深色模式
- [ ] 密码保护功能实现
- [ ] 云同步（可选）
- [ ] 更多统计维度（年度、自定义时间段）
- [ ] 数据可视化（趋势图、日历热力图）
- [ ] 清空数据功能实现
- [ ] 使用说明页面
- [ ] 版本更新检查

## 技术亮点

? **纯 .NET MAUI** - 不依赖第三方 UI 库  
? **Shell 导航** - 使用 MAUI Shell 实现现代导航  
? **现代设计** - 使用最新的 MAUI 控件（Border、Shadow 等）  
? **三页架构** - 主页、统计、设置清晰分离  
? **跨平台一致** - 所有平台统一的视觉体验  
? **轻量高效** - 无额外依赖，启动快速  
? **易于维护** - 代码简洁，结构清晰  
? **MVVM 模式** - 数据绑定和命令模式  

## 隐私声明

本应用所有数据仅存储在您的设备本地，不会上传到任何服务器。我们重视并保护您的隐私。
