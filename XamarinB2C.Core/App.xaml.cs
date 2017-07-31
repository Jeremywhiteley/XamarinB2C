using System.Collections.Generic;
using Microsoft.Identity.Client;
using Xamarin.Forms;

namespace XamarinB2C
{
    public partial class App : Application
    {
        public static PublicClientApplication PCA = null;

		// Azure AD B2C Coordinates
		public static string Tenant = "MedB2CTenent.onmicrosoft.com";
		public static string ClientID = "dfd431b5-d2e9-4429-9165-870e450d5994";
		public static string PolicySignUpSignIn = "B2C_1_B2C_SUSI";
		public static string PolicyEditProfile = "b2c_1_edit_profile";
		public static string PolicyResetPassword = "b2c_1_reset";

		public static string[] Scopes = { "https://MedB2CTenent.onmicrosoft.com/demoapi/read" };
		public static string ApiEndpoint = "http://b2capidev.azurewebsites.net/api/values";

		public static string AuthorityBase = $"https://login.microsoftonline.com/tfp/{Tenant}/";
		public static string Authority = $"{AuthorityBase}{PolicySignUpSignIn}";
		public static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}";
		public static string AuthorityPasswordReset = $"{AuthorityBase}{PolicyResetPassword}";

		public static string databaseId = "ToDoList";
		public static string collectionId = "Items";
		public static string accURL = "https://b2cdev.documents.azure.com:443/";

		public static UIParent UiParent = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:XamarinB2C.App"/> class.
        /// </summary>
        public App()
        {
            InitializeComponent();
			PCA = new PublicClientApplication(ClientID, Authority);
			PCA.RedirectUri = $"msal{ClientID}://auth";
            MainPage = new NavigationPage(new MainPage());
        }

    }
}
