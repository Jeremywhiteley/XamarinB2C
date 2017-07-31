using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using System.Diagnostics;
using Microsoft.Azure.Documents.Client;
using XamarinB2C.Core.Model;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace XamarinB2C
{
    public partial class MainPage : ContentPage
    {

        //Aspuru
        public bool loggingUser;
        public bool loggedUser;
        string resourceToken = null;
        public string UserId { get; private set; }

        private Uri collectionLink = UriFactory.CreateDocumentCollectionUri(App.databaseId, App.collectionId);
        public DocumentClient Client { get; private set; }
        public MainPage()
        {
            InitializeComponent();
            loggingUser = false;
            loggedUser = false;
        }


        protected override async void OnAppearing()
        {

            if (loggingUser == false && loggedUser == false)
            {

                try
                {
                    AuthenticationResult ar = await App.PCA.AcquireTokenSilentAsync(
                        App.Scopes,
                        GetUserByPolicy(App.PCA.Users, App.PolicySignUpSignIn),
                        App.Authority, false);

                    UpdateUserInfo(ar);
                    UpdateSignInState(true);
                }
                catch (Exception ex)
                {
                    UpdateSignInState(false);
                }
            }
        }


        /// <summary>
        /// Event for the sign in sign out.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnSignInSignOut(object sender, EventArgs e)
        {
            loggingUser = true;
            try
            {
                if (btnSignInSignOut.Text == "Sign in")
                {

                    AuthenticationResult ar = await App.PCA.AcquireTokenAsync(
                        App.Scopes,
                        GetUserByPolicy(App.PCA.Users, App.PolicySignUpSignIn), App.UiParent);

                    UpdateUserInfo(ar);
                    UpdateSignInState(true);
                    loggingUser = false;
                    loggedUser = true;
                }
                else
                {
                    foreach (var user in App.PCA.Users)
                    {
                        App.PCA.Remove(user);
                    }
                    UpdateSignInState(false);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("AADB2C90118"))
                    OnPasswordReset();
                // Alert if any exception excludig user cancelling sign-in dialog
                else if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

        /// <summary>
        /// Gets the user by policy.
        /// </summary>
        /// <returns>The user by policy.</returns>
        /// <param name="users">Users.</param>
        /// <param name="policy">Policy.</param>
        private IUser GetUserByPolicy(IEnumerable<IUser> users, string policy)
        {
            foreach (var user in users)
            {
                string userIdentifier = Base64UrlDecode(user.Identifier.Split('.')[0]);
                if (userIdentifier.EndsWith(policy.ToLower())) return user;
            }

            return null;
        }

        /// <summary>
        /// Base64s the URL decode.
        /// </summary>
        /// <returns>The URL decode.</returns>
        /// <param name="s">S.</param>
        private string Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
            var byteArray = Convert.FromBase64String(s);
            var decoded = Encoding.UTF8.GetString(byteArray, 0, byteArray.Count());
            return decoded;
        }

        /// <summary>
        /// Updates the user info.
        /// </summary>
        /// <param name="ar">Ar.</param>
        public void UpdateUserInfo(AuthenticationResult ar)
        {
            JObject user = ParseIdToken(ar.IdToken);
            lblName.Text = user["name"]?.ToString();
            lblId.Text = user["oid"]?.ToString();
        }

        /// <summary>
        /// Parses the identifier token.
        /// </summary>
        /// <returns>The identifier token.</returns>
        /// <param name="idToken">Identifier token.</param>
        JObject ParseIdToken(string idToken)
        {
            // Get the piece with actual user info
            idToken = idToken.Split('.')[1];
            idToken = Base64UrlDecode(idToken);
            return JObject.Parse(idToken);
        }

        /// <summary>
        /// Event for calling webp API to connect cosmosDB and get data from it.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnCallApi(object sender, EventArgs e)
        {
            try
            {

                lblApi.Text = $"Calling API {App.ApiEndpoint}";
                AuthenticationResult ar = await App.PCA.AcquireTokenSilentAsync(App.Scopes, GetUserByPolicy(App.PCA.Users, App.PolicySignUpSignIn), App.Authority, false);
                string token = ar.AccessToken;
                JObject user = ParseIdToken(ar.IdToken);
                // Get data from API
                HttpClient client = new HttpClient();
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, App.ApiEndpoint);
                client.DefaultRequestHeaders.Add("UserID", user["oid"]?.ToString());
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(message);
                var responseString = await response.Content.ReadAsStringAsync();

                var resourceTokenJson = JsonConvert.DeserializeObject<JObject>(responseString);
                resourceToken = resourceTokenJson.GetValue("token").ToString();
                UserId = resourceTokenJson.GetValue("userid").ToString();

                if(resourceToken != null)
                {
                    Client = new DocumentClient(new System.Uri(App.accURL), resourceToken);
					TodoItem item = new TodoItem();
					item.Name = "Test2";
					item.Notes = "Test2  Notes";
					item.Done = true;
					await SaveTodoItemAsync(item,true);
					var result = await GetTodoItemsAsync();
                    if(result!=null){
					    lst.ItemsSource = result;
                     }
				}
                else
                {
                    lblApi.Text = $"Error calling API {App.ApiEndpoint} | {responseString}";
                }
            }
            catch (MsalUiRequiredException ex)
            {
                //Aspuru: Added for debugging
                Debug.WriteLine("Exception:", ex.ToString());
                await DisplayAlert($"Session has expired, please sign out and back in.", ex.ToString(), "Dismiss");
            }
            catch (Exception ex)
            {
                //Aspuru: Added for debugging
                Debug.WriteLine("Exception:", ex.ToString());
                await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

        /// <summary>
        /// Ons the edit profile.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnEditProfile(object sender, EventArgs e)
        {
            try
            {
                AuthenticationResult ar = await App.PCA.AcquireTokenAsync(App.Scopes, GetUserByPolicy(App.PCA.Users, App.PolicyEditProfile), UIBehavior.SelectAccount, string.Empty, null, App.AuthorityEditProfile, App.UiParent);
                UpdateUserInfo(ar);
            }
            catch (Exception ex)
            {
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }
        /// <summary>
        /// Method for password reset using password reset policy using MSAL .
        /// </summary>
        async void OnPasswordReset()
        {
            try
            {
                AuthenticationResult ar = await App.PCA.AcquireTokenAsync(App.Scopes, (IUser)null, UIBehavior.SelectAccount, string.Empty, null, App.AuthorityPasswordReset, App.UiParent);
                UpdateUserInfo(ar);
            }
            catch (Exception ex)
            {
                // Alert if any exception excludig user cancelling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

        /// <summary>
        /// Updates the state of the sign in.
        /// </summary>
        /// <param name="isSignedIn">If set to <c>true</c> is signed in.</param>
        void UpdateSignInState(bool isSignedIn)
        {
            btnSignInSignOut.Text = isSignedIn ? "Sign out" : "Sign in";
            btnEditProfile.IsVisible = isSignedIn;
            btnCallApi.IsVisible = isSignedIn;
            slUser.IsVisible = isSignedIn;
            lblApi.Text = "";
        }


		/// <summary>
		/// Method for getting the list of TotDoItem list
		/// </summary>
		/// <returns>The todo items async.</returns>
		public async Task<List<TodoItem>> GetTodoItemsAsync()
		{
			var Items = new List<TodoItem>();

			try
			{
				var query = Client.CreateDocumentQuery<TodoItem>(collectionLink)
								  .AsDocumentQuery();
				while (query.HasMoreResults)
				{
					Items.AddRange(await query.ExecuteNextAsync<TodoItem>());
				}
			}
			catch (DocumentClientException ex)
			{
				Debug.WriteLine("Error: ", ex.Message);
			}

			return Items;
		}

		/// <summary>
		/// Method for saving the totdoitem object
		/// </summary>
		/// <returns>The todo item async.</returns>
		/// <param name="item">Item.</param>
		/// <param name="isNewItem">If set to <c>true</c> is new item.</param>
		public async Task<bool> SaveTodoItemAsync(TodoItem item, bool isNewItem = false)
		{
			try
			{
				if (isNewItem)
				{
					var result=await Client.CreateDocumentAsync(collectionLink, item);
					return true;
				}
				else
				{
					await Client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(App.databaseId, App.collectionId, item.Id), item);
					return true;
				}
			}
			catch (DocumentClientException ex)
			{
				Debug.WriteLine("Error: ", ex.Message);
				return false;
			}

			return false;
		}
    }


}
