using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SymbolIconFinder.UWP
{
    public sealed partial class CodeDialog : ContentDialog
    {
        public CodeDialog(string desc, string icon, string xaml)
        {
            this.InitializeComponent();

            tbDesc.Text = desc;
            tbIcon.Text = icon;
            tbXaml.Text = xaml;

            LoadXAML(xaml);

            LoadSymbolIcon(desc, icon, xaml);

            LoadFontIcon(xaml);

            LoadButtonXaml(xaml);

            LoadTextBlockXaml(xaml);

            LoadCode(xaml);
        }

        private void LoadTextBlockXaml(string xaml)
        {
            tbxTextblock.Text = "<TextBlock x:Name=\"tbTest\" Text=\"" + xaml + "\" FontFamily=\"Segoe MDL2 Assets\"/>";
        }

        private void LoadCode(string xaml)
        {
            tbxCode.Text = "\\u" + xaml.Replace("&#x", "").Replace(";", "");
        }

        private void LoadButtonXaml(string xaml)
        {
            tbxButton.Text = "<Button x:Name=\"btnIcon\" Content=\"" + xaml + "\" FontFamily=\"Segoe MDL2 Assets\"/>";
        }

        private void LoadFontIcon(string xaml)
        {
            tbxFont.Text = "<FontIcon FontFamily=\"Segoe MDL2 Assets\" Glyph=\"" + xaml + "\"/>";
        }

        private void LoadXAML(string xaml)
        {
            tbxXaml.Text = xaml;
        }

        private void LoadSymbolIcon(string desc, string icon, string xaml)
        {
            tbxSymbol.Text = "<SymbolIcon Symbol=\"" + desc + "\"/>";
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private void tbx_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tbx = sender as TextBox;
            if (!String.IsNullOrEmpty(tbx.Text))
            {
                tbx.SelectAll();

                DataPackage d = new DataPackage();

                d.SetText(tbx.Text);

                Clipboard.SetContent(d);
            }
        }
    }
}
