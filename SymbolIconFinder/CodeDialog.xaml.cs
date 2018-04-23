using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SymbolIconFinder
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

            LoadCode(xaml);
        }

        private void LoadCode(string xaml)
        {
            tbxCode.Text = "\\u" + xaml.Replace("&#x", "").Replace(";","");
        }

        private void LoadButtonXaml(string xaml)
        {
            tbxButton.Text = "<Button x:Name=\"btnIcon\" Content=\"&#xE700;\" FontFamily=\"Segoe MDL2 Assets\"/>";
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
            }
        }
    }
}
