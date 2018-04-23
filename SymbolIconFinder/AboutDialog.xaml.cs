using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Store;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SymbolIconFinder
{
    public sealed partial class AboutDialog : ContentDialog
    {
        private StoreContext context = null;

        public AboutDialog()
        {
            this.InitializeComponent();

            GetAppVersion();           
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
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://docs.microsoft.com/en-us/windows/uwp/design/style/segoe-ui-symbol-font"));
        }

        private async void showmoreapps_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:Publisher?name=Red David"));
        }

        private async void hl_feedback_Click(object sender, RoutedEventArgs e)
        {
            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
            {
                // Launch feedback
                var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
                await launcher.LaunchAsync();
            }
            else
            {
                EmailMessage emailMessage = new EmailMessage();
                emailMessage.To.Add(new EmailRecipient("redappsupport@outlook.com"));
                emailMessage.Subject = "[FEEDBACK] Symbol Icon Finder";
            }
        }

        private void btn_share_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private async void btn_rate_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9P650NF68J50"));
        }

        private async void btn_fb_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.facebook.com/reddvidapps"));
        }

        private async void btn_twitter_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.twitter.com/reddvidapps"));
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {          
            this.Hide();
        }

        private void hl_donate_Click(object sender, RoutedEventArgs e)
        {
            PurchaseAddOn("9P650NF68J50");
        }

        private  async void PurchaseAddOn(string storeId)
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


    }
}
