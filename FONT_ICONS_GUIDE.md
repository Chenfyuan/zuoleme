# 字体图标替换指南

## ?? 添加 Material Symbols 字体

### 步骤 1: 下载字体文件

1. 访问 Google Fonts: https://fonts.google.com/icons
2. 搜索 "Material Symbols Rounded"
3. 下载字体文件（推荐使用 Rounded 变体）

**或者使用直接下载链接：**
- 下载地址：https://github.com/google/material-design-icons/raw/master/font/MaterialIconsRound-Regular.otf
- 重命名为：`MaterialSymbolsRounded.ttf`

### 步骤 2: 添加字体到项目

1. 将字体文件复制到 `Resources/Fonts/` 文件夹
2. 文件名：`MaterialSymbolsRounded.ttf`

### 步骤 3: 注册字体（已完成）

在 `MauiProgram.cs` 中已添加：

```csharp
.ConfigureFonts(fonts =>
{
    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
    fonts.AddFont("MaterialSymbolsRounded.ttf", "MaterialIcons");
});
```

## ?? 使用方法

### 方法 1: 使用常量类（推荐）

```xaml
xmlns:constants="clr-namespace:zuoleme.Constants"

<Label Text="{x:Static constants:MaterialIcons.Home}"
       FontFamily="MaterialIcons"
       FontSize="24"/>
```

### 方法 2: 直接使用 Unicode

```xaml
<Label Text="&#xe88a;"
       FontFamily="MaterialIcons"
       FontSize="24"/>
```

### 方法 3: 使用 FontImageSource

```xaml
<Image>
    <Image.Source>
        <FontImageSource Glyph="{x:Static constants:MaterialIcons.Home}"
                        FontFamily="MaterialIcons"
                        Size="24"
                        Color="White"/>
    </Image.Source>
</Image>
```

## ?? 图标映射表

| 原 Emoji | Material Icon | Unicode | 常量名称 |
|---------|---------------|---------|---------|
| ?? | favorite | \ue87d | MaterialIcons.Favorite |
| ?? | home | \ue88a | MaterialIcons.Home |
| ?? | assessment | \ue85c | MaterialIcons.Assessment |
| ?? | calendar_today | \ue935 | MaterialIcons.CalendarToday |
| ?? | settings | \ue8b8 | MaterialIcons.Settings |
| + | add | \ue145 | MaterialIcons.Add |
| ?? | trending_up | \ue8e5 | MaterialIcons.TrendingUp |
| ? | schedule | \ue8b5 | MaterialIcons.Schedule |
| ?? | description | \ue873 | MaterialIcons.Description |
| ?? | lightbulb | \ue0f0 | MaterialIcons.Lightbulb |
| ?? | save | \ue161 | MaterialIcons.Save |
| ?? | download | \uf090 | MaterialIcons.Download |
| ??? | delete_forever | \ue92b | MaterialIcons.DeleteForever |
| ?? | lock | \ue897 | MaterialIcons.Lock |
| ?? | book | \ue865 | MaterialIcons.Book |
| ?? | notifications | \ue7f4 | MaterialIcons.Notifications |
| ?? | info | \ue88e | MaterialIcons.Info |
| ? | chevron_right | \ue5cc | MaterialIcons.ChevronRight |

## ?? 优势

### Emoji 的问题
- ? 不同平台显示效果不一致
- ? Windows 上某些 Emoji 不支持
- ? 难以调整大小和颜色
- ? 可能导致字体加载错误

### 字体图标的优势
- ? 跨平台一致性
- ? 可自由调整大小
- ? 可自定义颜色
- ? 性能更好
- ? 更专业的视觉效果
- ? 支持 Material Design 规范

## ?? 迁移计划

### 阶段 1: 底部导航栏
- ? 主页图标
- ? 统计图标  
- ? 日历图标
- ? 设置图标

### 阶段 2: 主页
- ? 爱心图标（标题和按钮）
- ? 加号图标
- ? 统计卡片图标
- ? 时间图标
- ? 历史记录图标

### 阶段 3: 设置页
- ? 所有功能图标
- ? 数据管理图标
- ? 隐私安全图标

## ?? Material Design 颜色建议

```xaml
<!-- 主色 -->
<Color x:Key="IconPrimary">#E91E63</Color>

<!-- 辅助色 -->
<Color x:Key="IconSecondary">#9C27B0</Color>

<!-- 成功 -->
<Color x:Key="IconSuccess">#4CAF50</Color>

<!-- 警告 -->
<Color x:Key="IconWarning">#FF9800</Color>

<!-- 错误 -->
<Color x:Key="IconError">#EF5350</Color>

<!-- 信息 -->
<Color x:Key="IconInfo">#2196F3</Color>
```

## ?? 注意事项

1. **字体文件大小**：Material Icons 字体约 300KB，不会显著增加应用大小
2. **版权**：Material Icons 使用 Apache License 2.0，可免费商用
3. **备用方案**：如果字体加载失败，可设置 fallback 文本
4. **性能**：字体图标比图片更轻量，渲染更快

## ?? 快速测试

```xaml
<VerticalStackLayout Spacing="10" Padding="20">
    <Label Text="{x:Static constants:MaterialIcons.Home}"
           FontFamily="MaterialIcons"
           FontSize="48"
           TextColor="#E91E63"/>
    
    <Label Text="{x:Static constants:MaterialIcons.Favorite}"
           FontFamily="MaterialIcons"
           FontSize="48"
           TextColor="#E91E63"/>
    
    <Label Text="{x:Static constants:MaterialIcons.Settings}"
           FontFamily="MaterialIcons"
           FontSize="48"
           TextColor="#E91E63"/>
</VerticalStackLayout>
```

## ?? 资源链接

- Material Symbols: https://fonts.google.com/icons
- 图标搜索: https://fonts.google.com/icons?icon.set=Material+Symbols
- GitHub: https://github.com/google/material-design-icons
- 文档: https://developers.google.com/fonts/docs/material_symbols
