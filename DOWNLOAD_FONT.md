# 下载 Material Symbols 字体

## 自动下载脚本（PowerShell）

```powershell
# 设置变量
$fontUrl = "https://github.com/google/material-design-icons/raw/master/variablefont/MaterialSymbolsRounded%5BFILL%2CGRAD%2Copsz%2Cwght%5D.ttf"
$fontPath = "Resources\Fonts\MaterialSymbolsRounded.ttf"

# 创建 Fonts 文件夹（如果不存在）
New-Item -ItemType Directory -Force -Path "Resources\Fonts" | Out-Null

# 下载字体文件
Write-Host "正在下载 Material Symbols 字体..." -ForegroundColor Cyan
Invoke-WebRequest -Uri $fontUrl -OutFile $fontPath

if (Test-Path $fontPath) {
    $fileSize = (Get-Item $fontPath).Length / 1MB
    Write-Host "? 字体下载成功！" -ForegroundColor Green
    Write-Host "文件路径: $fontPath" -ForegroundColor Yellow
    Write-Host "文件大小: $([Math]::Round($fileSize, 2)) MB" -ForegroundColor Yellow
} else {
    Write-Host "? 字体下载失败！" -ForegroundColor Red
}
```

## 手动下载步骤

### 方案 1: GitHub 直接下载

1. 访问：https://github.com/google/material-design-icons
2. 导航到：`variablefont/MaterialSymbolsRounded[FILL,GRAD,opsz,wght].ttf`
3. 点击 "Download" 下载
4. 重命名为：`MaterialSymbolsRounded.ttf`
5. 复制到：`Resources/Fonts/` 文件夹

### 方案 2: Google Fonts 下载

1. 访问：https://fonts.google.com/icons
2. 搜索 "Material Symbols Rounded"
3. 点击 "Download font" 按钮
4. 解压缩下载的 ZIP 文件
5. 找到 `.ttf` 字体文件
6. 重命名为：`MaterialSymbolsRounded.ttf`
7. 复制到：`Resources/Fonts/` 文件夹

### 方案 3: CDN 直接链接（推荐）

使用以下 PowerShell 命令直接下载：

```powershell
# 进入项目根目录
cd E:\source\zuoleme

# 下载字体
Invoke-WebRequest -Uri "https://github.com/google/material-design-icons/raw/master/variablefont/MaterialSymbolsRounded%5BFILL%2CGRAD%2Copsz%2Cwght%5D.ttf" -OutFile "Resources\Fonts\MaterialSymbolsRounded.ttf"
```

## 验证安装

运行以下命令检查字体是否正确安装：

```powershell
# 检查文件是否存在
Test-Path "Resources\Fonts\MaterialSymbolsRounded.ttf"

# 查看文件信息
Get-Item "Resources\Fonts\MaterialSymbolsRounded.ttf" | Select-Object Name, Length, LastWriteTime
```

预期输出：
```
Name                          Length LastWriteTime
----                          ------ -------------
MaterialSymbolsRounded.ttf   1234567 2024-01-01 12:00:00
```

## 构建和测试

1. 确保字体文件在 `Resources/Fonts/` 文件夹中
2. 清理并重新构建项目：

```bash
dotnet clean
dotnet restore
dotnet build -f net10.0-windows10.0.19041.0
```

3. 运行应用验证图标显示正常

## 常见问题

### Q: 字体下载失败？
A: 尝试使用备用链接或手动下载

### Q: 图标不显示？
A: 检查以下几点：
1. 字体文件路径是否正确
2. MauiProgram.cs 中是否正确注册字体
3. XAML 中 FontFamily 是否设置为 "MaterialIcons"
4. Unicode 字符是否正确

### Q: 字体文件太大？
A: Material Symbols 字体包含所有图标，约 1-2 MB，这是正常的

### Q: 可以使用其他字体图标库吗？
A: 可以，推荐的替代方案：
- Font Awesome (需要许可证)
- Ionicons
- Fluent UI Icons
- Segoe Fluent Icons (Windows 11)

## 下一步

字体安装完成后：
1. 运行应用测试所有图标
2. 如有显示问题，查看输出窗口的错误信息
3. 参考 FONT_ICONS_GUIDE.md 了解更多使用方法
