using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SymbolIconFinder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<Icons> iconsList = new ObservableCollection<Icons>();

        public MainPage()
        {
            this.InitializeComponent();

            GetItems();

            SetText();

            // Get CTRL+F
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
        }

        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("Down"))
            {
                var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
                {
                    switch (args.VirtualKey)
                    {
                        case VirtualKey.F:
                            searchBox.Focus(FocusState.Programmatic);
                            break;
                    }
                }
            }
        }

        private void SetText()
        {
            Run run1 = new Run();
            run1.Text = "<SymbolIcon Symbol=\"GlobalNavigationButton\"/>";

            Run run2 = new Run();
            run2.Text = "<FontIcon FontFamily=\"Segoe MDL2 Assets\" Glyph=\"&#xE700;\"/>";

            //t.Inlines.Add(run1);
            //u.Inlines.Add(run2);
        }

        private void GetItems()
        {
            Icons icons = new Icons();
            iconsList = icons.GetIcons();
        }

        private void searchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (sender.Text.Length > 1)
                {
                    string inputText = sender.Text.ToLower();

                    gvIcons.ItemsSource = iconsList.Where(i => 
                    i.desc.ToLower().Contains(inputText) ||
                    i.tags.ToLower().Contains(inputText)
                    );
                }
                else
                {
                    gvIcons.ItemsSource = iconsList;
                }
            }
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var tappedItem = (UIElement)e.OriginalSource;

            // Set highlight            
            gvIcons.SelectedIndex = gvIcons.Items.IndexOf((senderElement).DataContext);

            var attachedFlyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(senderElement);
            attachedFlyout.ShowAt(tappedItem, e.GetPosition(tappedItem));
        }

        private void flyOutItem_Click(object sender, RoutedEventArgs e)
        {
            string item = (sender as MenuFlyoutItem).Tag as string;

            if (!String.IsNullOrEmpty(item) && gvIcons.SelectedItem != null)
            {
                Icons selectedIcon = gvIcons.SelectedItem as Icons;

                switch (item)
                {
                    case "enum":
                        CopyToClipboard(selectedIcon.desc);
                        break;
                    case "xaml":
                        CopyToClipboard(selectedIcon.unicode);
                        break;
                }
            }
        }

        private void CopyToClipboard(string content)
        {
            Clipboard.Clear();
            Clipboard.Flush();

            var toCopy = new DataPackage();
            toCopy.SetText(content);
            Clipboard.SetContent(toCopy);
        }

        private async void btnCodes_Click(object sender, RoutedEventArgs e)
        {
            if (gvIcons.SelectedItem != null)
            {
                Icons selectedItem = gvIcons.SelectedItem as Icons;
                // Show Codes dialog
                CodeDialog codeDialog = new CodeDialog(selectedItem.desc, selectedItem.icon, selectedItem.unicode);
                await codeDialog.ShowAsync();
            }
        }

        private async void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            // Show About Content/User Dialog
            AboutDialog aboutDialog = new AboutDialog();
            await aboutDialog.ShowAsync();  
        }

        private void btnCodes_Loaded(object sender, RoutedEventArgs e)
        {
            ControlAppBarButtonVisibility(gvIcons.SelectedItem);
        }

        private void ControlAppBarButtonVisibility(object selectedItem)
        {
            if (selectedItem != null)
            {
                btnCodes.IsEnabled = true;
            }
            else
            {
                btnCodes.IsEnabled = false;
            }
        }

        private void gvIcons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ControlAppBarButtonVisibility(gvIcons.SelectedItem);
        }

        private async void gvIcons_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (gvIcons.SelectedItem != null)
            {
                Icons selectedItem = gvIcons.SelectedItem as Icons;
                // Show Codes dialog
                CodeDialog codeDialog = new CodeDialog(selectedItem.desc, selectedItem.icon, selectedItem.unicode);
                await codeDialog.ShowAsync();
            }
        }
    }
}
