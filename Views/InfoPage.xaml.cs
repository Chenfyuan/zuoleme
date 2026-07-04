namespace zuoleme.Views
{
    [QueryProperty(nameof(PageTitle), "title")]
    [QueryProperty(nameof(Body), "body")]
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {
            InitializeComponent();
        }

        public string PageTitle
        {
            get => Title;
            set
            {
                Title = value;
                TitleLabel.Text = value;
            }
        }

        public string Body
        {
            get => BodyLabel.Text ?? string.Empty;
            set => BodyLabel.Text = value;
        }
    }
}
