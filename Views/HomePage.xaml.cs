using zuoleme.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using System.Threading.Tasks;

namespace zuoleme.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly MainViewModel _viewModel;
        private bool _isAnimating = false;

        public HomePage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
            
            // 启用页面缓存，避免每次切换都重新创建
            Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Page.SetToolbarPlacement(
                this, 
                Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.ToolbarPlacement.Bottom);
            
            // 监听健康状态变化，更新颜色
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.HealthProgressColor) ||
                    e.PropertyName == nameof(_viewModel.HealthBackgroundStartColor) ||
                    e.PropertyName == nameof(_viewModel.ButtonBackgroundColor))
                {
                    UpdateColors();
                }
            };
            
            UpdateColors();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // 页面显示时不需要重新加载数据，因为有消息机制自动同步
        }

        private void UpdateColors()
        {
            try
            {
                // 更新 ProgressBar 颜色
                var progressColorString = _viewModel.HealthProgressColor;
                if (!string.IsNullOrEmpty(progressColorString))
                {
                    HealthProgressBar.ProgressColor = Color.FromArgb(progressColorString);
                }

                // 更新健康卡片背景渐变
                var startColorString = _viewModel.HealthBackgroundStartColor;
                var endColorString = _viewModel.HealthBackgroundEndColor;
                
                if (!string.IsNullOrEmpty(startColorString) && !string.IsNullOrEmpty(endColorString))
                {
                    var gradient = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 1)
                    };
                    
                    gradient.GradientStops.Add(new GradientStop 
                    { 
                        Color = Color.FromArgb(startColorString), 
                        Offset = 0.0f 
                    });
                    
                    gradient.GradientStops.Add(new GradientStop 
                    { 
                        Color = Color.FromArgb(endColorString), 
                        Offset = 1.0f 
                    });
                    
                    HealthCard.Background = gradient;
                }

                // 更新按钮背景渐变和加号图标颜色
                var buttonColorString = _viewModel.ButtonBackgroundColor;
                if (!string.IsNullOrEmpty(buttonColorString))
                {
                    var buttonColor = Color.FromArgb(buttonColorString);
                    
                    // 创建更深的渐变色
                    var darkerColor = Color.FromRgba(
                        (int)(buttonColor.Red * 0.8 * 255),
                        (int)(buttonColor.Green * 0.8 * 255),
                        (int)(buttonColor.Blue * 0.8 * 255),
                        (int)(buttonColor.Alpha * 255)
                    );
                    
                    var buttonGradient = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 1)
                    };
                    
                    buttonGradient.GradientStops.Add(new GradientStop 
                    { 
                        Color = buttonColor, 
                        Offset = 0.0f 
                    });
                    
                    buttonGradient.GradientStops.Add(new GradientStop 
                    { 
                        Color = darkerColor, 
                        Offset = 1.0f 
                    });
                    
                    MainButton.Background = buttonGradient;
                    
                    // 更新阴影颜色
                    MainButton.Shadow = new Shadow
                    {
                        Brush = new SolidColorBrush(buttonColor),
                        Opacity = 0.5f,
                        Radius = 20,
                        Offset = new Point(0, 8)
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新颜色失败: {ex.Message}");
            }
        }

        private async void OnMainButtonTapped(object sender, EventArgs e)
        {
            if (_isAnimating) return;
            _isAnimating = true;

            try
            {
                // 触觉反馈（如果设备支持）
                try
                {
                    HapticFeedback.Default.Perform(HapticFeedbackType.Click);
                }
                catch { }

                // 1. 按下动画：缩小 + 轻微旋转
                await Task.WhenAll(
                    MainButton.ScaleToAsync(0.85, 100, Easing.CubicOut),
                    MainButton.RotateToAsync(5, 100, Easing.CubicOut)
                );

                // 执行添加记录命令
                if (_viewModel.AddRecordCommand.CanExecute(null))
                {
                    _viewModel.AddRecordCommand.Execute(null);
                }

                // 2. 弹起动画：放大反弹 + 旋转回正 + 脉冲效果
                var bounceAnimation = MainButton.ScaleToAsync(1.15, 150, Easing.CubicOut);
                var rotateBackAnimation = MainButton.RotateToAsync(0, 150, Easing.CubicOut);
                
                await Task.WhenAll(bounceAnimation, rotateBackAnimation);

                // 3. 回弹到正常大小
                await MainButton.ScaleToAsync(1.0, 100, Easing.CubicIn);

                // 4. 成功脉冲动画（3次快速闪烁）
                _ = Task.Run(async () =>
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            await MainButton.ScaleToAsync(1.05, 80, Easing.SinInOut);
                            await MainButton.ScaleToAsync(1.0, 80, Easing.SinInOut);
                        }
                    });
                });

                // 5. Plus 图标旋转动画
                _ = Task.Run(async () =>
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await PlusIcon.RotateToAsync(360, 500, Easing.SpringOut);
                        PlusIcon.Rotation = 0;
                    });
                });

                // 6. 爱心粒子飞散效果（使用健康状态颜色）
                await CreateParticleExplosion();

                // 长震动反馈表示成功
                try
                {
                    HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
                }
                catch { }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"动画执行失败: {ex.Message}");
            }
            finally
            {
                _isAnimating = false;
            }
        }

        private async Task CreateParticleExplosion()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    ParticleContainer.IsVisible = true;
                    ParticleContainer.Children.Clear();

                    // 获取按钮的屏幕位置
                    var buttonBounds = MainButton.Bounds;
                    var buttonCenterX = buttonBounds.X + buttonBounds.Width / 2;
                    var buttonCenterY = buttonBounds.Y + buttonBounds.Height / 2 + 100; // 调整偏移

                    // 获取当前健康状态的主色调作为粒子颜色
                    var particleColorString = _viewModel.ButtonBackgroundColor;
                    var particleColor = !string.IsNullOrEmpty(particleColorString) 
                        ? Color.FromArgb(particleColorString) 
                        : Color.FromArgb("#E91E63"); // 默认粉色

                    var random = new Random();
                    var particleCount = 12; // 粒子数量

                    var animationTasks = new List<Task>();

                    for (int i = 0; i < particleCount; i++)
                    {
                        // 创建爱心粒子（使用字体图标）
                        var particle = new Label
                        {
                            Text = "\ue87d", // MaterialIcons.Favorite
                            FontFamily = "MaterialIcons",
                            FontSize = random.Next(24, 40),
                            TextColor = particleColor, // 使用健康状态颜色
                            Opacity = 1.0
                        };

                        // 计算飞散方向（360度均匀分布）
                        var angle = (360.0 / particleCount) * i;
                        var radians = angle * Math.PI / 180.0;
                        var distance = random.Next(100, 200);
                        
                        var targetX = buttonCenterX + Math.Cos(radians) * distance;
                        var targetY = buttonCenterY + Math.Sin(radians) * distance;

                        // 设置初始位置（按钮中心）
                        AbsoluteLayout.SetLayoutBounds(particle, new Rect(buttonCenterX, buttonCenterY, 40, 40));
                        AbsoluteLayout.SetLayoutFlags(particle, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);

                        ParticleContainer.Children.Add(particle);

                        // 创建飞散动画
                        var flyAnimation = Task.Run(async () =>
                        {
                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                // 飞出 + 淡出 + 旋转
                                var translateTask = particle.TranslateToAsync(
                                    targetX - buttonCenterX, 
                                    targetY - buttonCenterY, 
                                    800, 
                                    Easing.CubicOut);
                                
                                var fadeTask = particle.FadeToAsync(0, 800, Easing.CubicIn);
                                var rotateTask = particle.RotateToAsync(random.Next(180, 360), 800, Easing.Linear);
                                var scaleTask = particle.ScaleToAsync(0.5, 800, Easing.CubicIn);

                                await Task.WhenAll(translateTask, fadeTask, rotateTask, scaleTask);
                            });
                        });

                        animationTasks.Add(flyAnimation);
                    }

                    // 等待所有粒子动画完成
                    await Task.WhenAll(animationTasks);

                    // 清理粒子
                    ParticleContainer.Children.Clear();
                    ParticleContainer.IsVisible = false;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"粒子动画失败: {ex.Message}");
            }
        }
    }
}
