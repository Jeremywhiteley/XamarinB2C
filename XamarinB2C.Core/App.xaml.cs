using System.Collections.Generic;
using Microsoft.Identity.Client;
using Xamarin.Forms;

namespace XamarinB2C
{
    public partial class App : Application
    {
        public static bool UseMockDataStore = true;
        public static string BackendUrl = "https://localhost:5000";
        public static IDictionary<string, string> LoginParameters => null;

        public static PublicClientApplication PCA = null;

		// Azure AD B2C Coordinates
		public static string Tenant = "MedB2CTenent.onmicrosoft.com";//"fabrikamb2c.onmicrosoft.com";
		public static string ClientID = "dfd431b5-d2e9-4429-9165-870e450d5994";//"90c0fe63-bcf2-44d5-8fb7-b8bbc0b29dc6";
		public static string PolicySignUpSignIn = "B2C_1_B2C_SUSI";//"b2c_1_susi";
		public static string PolicyEditProfile = "b2c_1_edit_profile";
		public static string PolicyResetPassword = "b2c_1_reset";

		public static string[] Scopes = { "https://MedB2CTenent.onmicrosoft.com/demoapi/read" };//{ "https://fabrikamb2c.onmicrosoft.com/demoapi/demo.read" };
		public static string ApiEndpoint = "http://b2capidev.azurewebsites.net/api/values";//"https://fabrikamb2chello.azurewebsites.net/hello";

		public static string AuthorityBase = $"https://login.microsoftonline.com/tfp/{Tenant}/";
		public static string Authority = $"{AuthorityBase}{PolicySignUpSignIn}";
		public static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}";
		public static string AuthorityPasswordReset = $"{AuthorityBase}{PolicyResetPassword}";


		public static readonly string EndpointUri = "https://b2cdev.documents.azure.com:443/";
		public static readonly string DatabaseName = "ToDoList";
		public static readonly string CollectionName = "Items";
		public static string accKey = "4qIWjbeQc3QB8ukxRQXtrYer4VZLVzlwcwXxYVrXt1kqf0s4obpLER49oC3Fx9RTOV2VS9q0vsLCf4S6yAgQTQ==";


		public static UIParent UiParent = null;
        public App()
        {
            InitializeComponent();
			PCA = new PublicClientApplication(ClientID, Authority);
			PCA.RedirectUri = $"msal{ClientID}://auth";
            MainPage = new NavigationPage(new MainPage());
        }

    }
}
