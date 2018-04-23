using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Store;
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
        private StoreContext context = null;

        public MainPage()
        {
            this.InitializeComponent();

            GetItems();

            SetText();

            ConsumeAddOn("9P650NF68J50");

            // Get CTRL+F
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
        }

        private async void ConsumeAddOn(string storeId)
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

            // This is an example for a Store-managed consumable, where you specify the actual number
            // of units that you want to report as consumed so the Store can update the remaining
            // balance. For a developer-managed consumable where you maintain the balance, specify 1
            // to just report the add-on as fulfilled to the Store.
            uint quantity = 10;
            string addOnStoreId = "9NKP2L5LWQX1";
            Guid trackingId = Guid.NewGuid();

            StoreConsumableResult result = await context.ReportConsumableFulfillmentAsync(addOnStoreId, quantity, trackingId);
        }

        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("Down"))
            {
                var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
                {
                    int mouseWheelMove = GetMouseWheelMovement();
                    switch (args.VirtualKey)
                    {
                        case VirtualKey.F:
                            searchBox.Focus(FocusState.Programmatic);
                            break;
                    }
                }
            }
        }

        private int GetMouseWheelMovement()
        {
            return 0;
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

            //try
            //{
            //    foreach (var item in e.AddedItems)
            //    {
            //        GridViewItem lvi = (sender as GridView).ContainerFromItem(item) as GridViewItem;
            //        lvi.ContentTemplate = (DataTemplate)this.Resources["Selected"];
            //    }
            //    foreach (var item in e.RemovedItems)
            //    {
            //        GridViewItem lvi = (sender as GridView).ContainerFromItem(item) as GridViewItem;
            //        lvi.ContentTemplate = (DataTemplate)this.Resources["Normal"];
            //    }
            //}
            //catch
            //{
            //}
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

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // searchBox.PlaceholderText = e.NewSize.Width.ToString();
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Grid g = sender as Grid;
            var visGrps = VisualStateManager.GetVisualStateGroups(g);
            var visGrp = visGrps[0];
            visGrp.States[0].Storyboard.Begin();
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Grid g = sender as Grid;
            var visGrps = VisualStateManager.GetVisualStateGroups(g);
            var visGrp = visGrps[0];
            visGrp.States[1].Storyboard.Begin();
        }

        private async void gvIcons_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && gvIcons.SelectedItem != null)
            {
                Icons selectedItem = gvIcons.SelectedItem as Icons;
                // Show Codes dialog
                CodeDialog codeDialog = new CodeDialog(selectedItem.desc, selectedItem.icon, selectedItem.unicode);
                await codeDialog.ShowAsync();
            }
        }

        private void Grid_RightTapped2(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var tappedItem = (UIElement)e.OriginalSource;
            Debug.WriteLine(tappedItem + senderElement.ToString());
            // Set highlight            
            gvIcons.SelectedIndex = gvIcons.Items.IndexOf((senderElement).DataContext);

            OpenFlyOut(gvIcons.SelectedIndex);
            //var attachedFlyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(senderElement);
            //attachedFlyout.ShowAt(tappedItem, e.GetPosition(tappedItem));
        }

        private void OpenFlyOut(int selectedIndex)
        {
            if (selectedIndex != -1)
            {
                GridViewItem lvi = gvIcons.ContainerFromItem(gvIcons.SelectedItem) as GridViewItem;
                Debug.WriteLine(lvi);
                var _Container = gvIcons.ItemContainerGenerator.ContainerFromItem(lvi);
                var _Children = AllChildren(_Container);

                var _FirstName = _Children
                    // only interested in TextBoxes
                    .OfType<Grid>()
                    // only interested in FirstName
                    .First(x => x.Name.Equals("gFrame"));

                FrameworkElement senderElement = _FirstName as FrameworkElement;
                FlyoutBase.ShowAttachedFlyout(senderElement as FrameworkElement);

            }
        }

        public List<Control> AllChildren(DependencyObject parent)
        {
            var _List = new List<Control>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _Child = VisualTreeHelper.GetChild(parent, i);
                if (_Child is Control)
                    _List.Add(_Child as Control);
                _List.AddRange(AllChildren(_Child));
            }
            return _List;
        }
    }
}
