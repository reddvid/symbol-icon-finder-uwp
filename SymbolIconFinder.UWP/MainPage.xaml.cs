using Microsoft.Services.Store.Engagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.UserActivities;
using Windows.Foundation;
using Windows.Services.Store;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SymbolIconFinder.UWP
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

            // ConsumeAddOn("9P650NF68J50");

            // Get CTRL+F
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;

            GenerateActivityAsync();
        }

        UserActivitySession _currentActivity;
        private async Task GenerateActivityAsync()
        {
            // Get the default UserActivityChannel and query it for our UserActivity. If the activity doesn't exist, one is created.
            UserActivityChannel channel = UserActivityChannel.GetDefault();
            UserActivity userActivity = await channel.GetOrCreateUserActivityAsync("MainPage");

            // Populate required properties
            userActivity.VisualElements.DisplayText = "Symbol Icon Finder";
            userActivity.ActivationUri = new Uri("symboliconfinder://");        
            userActivity.VisualElements.Description = "View and search Segoe MDL2 icons";
                     
            // Save
            await userActivity.SaveAsync(); //save the new metadata

            // Dispose of any current UserActivitySession, and create a new one.
            _currentActivity?.Dispose();
            _currentActivity = userActivity.CreateSession();

            Debug.WriteLine("Created user session");
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
            gvIcons.ItemsSource = iconsList;

            CountIcons();
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
                    i.tags.ToLower().Contains(inputText));
                }
                else
                {
                    gvIcons.ItemsSource = iconsList;
                }
            }

            CountIcons();
        }

        private void CountIcons()
        {
            if (gvIcons.Items.Count != 0)
            {
                gvIcons.Visibility = Visibility.Visible;
                NoResultsTxt.Visibility = Visibility.Collapsed;
            }
            else
            {
                gvIcons.Visibility = Visibility.Collapsed;
                NoResultsTxt.Visibility = Visibility.Visible;
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
                    case "xamltext":
                        CopyXamlToClipboard(selectedIcon.unicode);
                        break;
                    case "xamlcontent":
                        CopyXamlToClipboardAsContent(selectedIcon.unicode);
                        break;
                    case "xamlraw":
                        CopyToClipboard(selectedIcon.unicode);
                        break;
                }
            }
        }

        private void CopyXamlToClipboardAsContent(string content)
        {
            Clipboard.Clear();
            Clipboard.Flush();

            var toCopy = new DataPackage();
            toCopy.SetText("Content=\"" + content + "\" FontFamily=\"Segoe MDL2 Assets\"");
            Clipboard.SetContent(toCopy);
        }

        private void CopyXamlToClipboard(string content)
        {
            Clipboard.Clear();
            Clipboard.Flush();

            var toCopy = new DataPackage();
            toCopy.SetText("Text=\"" + content + "\" FontFamily=\"Segoe MDL2 Assets\"");
            Clipboard.SetContent(toCopy);
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
                FindName("CodeGrid");
                CodeGrid.Visibility = Visibility.Visible;

                LoadCodes(selectedItem);
            }
        }

        private void LoadCodes(Icons selectedItem)
        {
            var desc = selectedItem.desc;
            var icon = selectedItem.icon;
            var xaml = selectedItem.unicode;

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

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            // Show About Content/User Dialog
            //AboutDialog aboutDialog = new AboutDialog();
            //await aboutDialog.ShowAsync();

            FindName("AboutGrid");
            AboutGrid.Visibility = Visibility.Visible;

            GetAppVersion();
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
                FindName("CodeGrid");
                CodeGrid.Visibility = Visibility.Visible;

                LoadCodes(selectedItem);
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
                FindName("CodeGrid");
                CodeGrid.Visibility = Visibility.Visible;

                LoadCodes(selectedItem);
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
                var _Container = gvIcons.ContainerFromItem(lvi);
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

        private void CloseAboutBtn_Click(object sender, RoutedEventArgs e)
        {
            UnloadObject(AboutGrid);
        }

        private void GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                DataRequestedEventArgs>(this.ShareTextHandler);

            VersionTextBlock.Text = string.Format("version {0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.Properties.Title = "Symbol Icon Finder";
            request.Data.Properties.Description = "Share Symbol Icon Finder";
            request.Data.Properties.ApplicationName = "Symbol Icon Finder";

            request.Data.SetWebLink(new Uri("https://www.microsoft.com/store/apps/9P650NF68J50"));
            request.Data.SetApplicationLink(new Uri("https://www.microsoft.com/store/apps/9P650NF68J50"));
            request.Data.SetText("Symbol Icon Finder for Windows 10. #Windows10 #UWP");
        }

        private async void btn_source_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://docs.microsoft.com/en-us/windows/uwp/design/style/segoe-ui-symbol-font"));
        }

        private async void showmoreapps_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:Publisher?name=Red David"));
        }

        private async void hl_feedback_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StoreServicesFeedbackLauncher.IsSupported())
                {
                    // Launch feedback
                    var launcher = StoreServicesFeedbackLauncher.GetDefault();
                    await launcher.LaunchAsync();
                }
                else
                {
                    EmailMessage emailMessage = new EmailMessage();
                    emailMessage.To.Add(new EmailRecipient("redappsupport@outlook.com"));
                    emailMessage.Subject = "[FEEDBACK] Symbol Icon Finder";
                    await EmailManager.ShowComposeNewEmailAsync(emailMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void btn_share_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private async void btn_rate_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9P650NF68J50"));
        }

        private async void btn_fb_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.facebook.com/reddvidapps"));
        }

        private async void btn_twitter_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.twitter.com/reddvidapps"));
        }

        private async void hl_donate_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.buymeacoffee.com/redDavid"));
            // PurchaseAddOn("9P650NF68J50");
        }

        private async void PurchaseAddOn(string storeId)
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

            StorePurchaseResult result = await context.RequestPurchaseAsync(storeId);

            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null)
            {
                extendedError = result.ExtendedError.Message;
            }

            switch (result.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                    tbStatus.Text = "You already donated.";
                    break;

                case StorePurchaseStatus.Succeeded:
                    tbStatus.Text = "Donation succeeded. Thank you!";
                    break;

                case StorePurchaseStatus.NotPurchased:
                    tbStatus.Text = "Your purchase did not complete. " +
                        "The user may have cancelled the purchase. ExtendedError: " + extendedError;
                    break;

                case StorePurchaseStatus.NetworkError:
                    tbStatus.Text = "The purchase was unsuccessful due to a network error. " +
                        "ExtendedError: " + extendedError;
                    break;

                case StorePurchaseStatus.ServerError:
                    tbStatus.Text = "The purchase was unsuccessful due to a server error. " +
                        "ExtendedError: " + extendedError;
                    break;

                default:
                    tbStatus.Text = "The purchase was unsuccessful due to an unknown error. " +
                        "ExtendedError: " + extendedError;
                    break;
            }

        }

        private void CloseDialogBtn_Click(object sender, RoutedEventArgs e)
        {
            UnloadObject(CodeGrid);

        }
    }
}
