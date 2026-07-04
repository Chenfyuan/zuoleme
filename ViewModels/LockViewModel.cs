using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using zuoleme.Services;

namespace zuoleme.ViewModels
{
    public class LockViewModel : INotifyPropertyChanged
    {
        private const int PinLength = 4;

        private readonly PrivacyService _privacyService;
        private string _pinInput = "";
        private string _errorMessage = "";
        private bool _isVerifying;

        public string PinInput
        {
            get => _pinInput;
            private set
            {
                _pinInput = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilledDots));
                OnPropertyChanged(nameof(Dot1Filled));
                OnPropertyChanged(nameof(Dot2Filled));
                OnPropertyChanged(nameof(Dot3Filled));
                OnPropertyChanged(nameof(Dot4Filled));
            }
        }

        public bool Dot1Filled => PinInput.Length >= 1;
        public bool Dot2Filled => PinInput.Length >= 2;
        public bool Dot3Filled => PinInput.Length >= 3;
        public bool Dot4Filled => PinInput.Length >= 4;

        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// 已输入的位数，用于圆点指示器显示
        /// </summary>
        public int FilledDots => PinInput.Length;

        public ICommand DigitCommand { get; }
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// PIN 校验通过时触发
        /// </summary>
        public event Action? Unlocked;

        public LockViewModel(PrivacyService privacyService)
        {
            _privacyService = privacyService;
            DigitCommand = new Command<string>(async (digit) => await OnDigitAsync(digit));
            DeleteCommand = new Command(OnDelete);
        }

        private async Task OnDigitAsync(string? digit)
        {
            if (_isVerifying || string.IsNullOrEmpty(digit)) return;
            if (PinInput.Length >= PinLength) return;

            ErrorMessage = "";
            PinInput += digit;

            if (PinInput.Length == PinLength)
            {
                _isVerifying = true;
                try
                {
                    var success = await _privacyService.VerifyPinAsync(PinInput);
                    if (success)
                    {
                        PinInput = "";
                        Unlocked?.Invoke();
                    }
                    else
                    {
                        ErrorMessage = "密码错误，请重试";
                        PinInput = "";
                    }
                }
                finally
                {
                    _isVerifying = false;
                }
            }
        }

        private void OnDelete()
        {
            if (_isVerifying || PinInput.Length == 0) return;
            ErrorMessage = "";
            PinInput = PinInput.Substring(0, PinInput.Length - 1);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
