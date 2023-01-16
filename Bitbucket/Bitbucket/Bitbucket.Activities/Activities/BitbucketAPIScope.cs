using Bitbucket.Activities.Properties;
using Pathoschild.Http.Client;
using System;
using System.Activities;
using System.Activities.Statements;
using System.ComponentModel;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;

namespace Bitbucket.Activities
{
    [LocalizedDisplayName(nameof(Resources.BitbucketAPIScope_DisplayName))]
    [LocalizedDescription(nameof(Resources.BitbucketAPIScope_Description))]
    public class BitbucketAPIScope : ContinuableAsyncNativeActivity
    {
        #region Properties

        [Browsable(false)]
        public ActivityAction<IObjectContainer​> Body { get; set; }

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.BitbucketAPIScope_AccountUsername_DisplayName))]
        [LocalizedDescription(nameof(Resources.BitbucketAPIScope_AccountUsername_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> AccountUsername { get; set; }

        [LocalizedDisplayName(nameof(Resources.BitbucketAPIScope_AppPassword_DisplayName))]
        [LocalizedDescription(nameof(Resources.BitbucketAPIScope_AppPassword_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<SecureString> AppPassword { get; set; }

        // A tag used to identify the scope in the activity context
        internal static string ParentContainerPropertyTag => "ScopeActivity";

        // Object Container: Add strongly-typed objects here and they will be available in the scope's child activities.
        private readonly IObjectContainer _objectContainer;

        #endregion


        #region Constructors

        public BitbucketAPIScope(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;

            Body = new ActivityAction<IObjectContainer>
            {
                Argument = new DelegateInArgument<IObjectContainer>(ParentContainerPropertyTag),
                Handler = new Sequence { DisplayName = Resources.Do }
            };
        }

        public BitbucketAPIScope() : this(new ObjectContainer())
        {

        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (AccountUsername == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(AccountUsername)));
            if (AppPassword == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(AppPassword)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<NativeActivityContext>> ExecuteAsync(NativeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var username = AccountUsername.Get(context);
            var password = AppPassword.Get(context);
            var passwordString = new System.Net.NetworkCredential(string.Empty, password).Password;

            //Prepare default header
            var authHeaderString = username + ":" + passwordString; // This is the format used in the HTTP request header.
            var base64EncodedCredentials = Base64Encode(authHeaderString);

            // Create client and assign default request header
            var defaultHttpClient = new HttpClient();
            defaultHttpClient.DefaultRequestHeaders
                .Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedCredentials);

            var client = new FluentClient(new Uri("https://api.bitbucket.org/2.0/"), defaultHttpClient);

            // Send to child activities
            _objectContainer.Add(client);

            return (ctx) =>
            {
                // Schedule child activities
                if (Body != null)
                    ctx.ScheduleAction<IObjectContainer>(Body, _objectContainer, OnCompleted, OnFaulted);

                // Outputs
            };
        }
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            var encodedCredentials = System.Convert.ToBase64String(plainTextBytes);
            return encodedCredentials;
        }
        #endregion


        #region Events

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            faultContext.CancelChildren();
            Cleanup();
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            Cleanup();
        }

        #endregion


        #region Helpers

        private void Cleanup()
        {
            var disposableObjects = _objectContainer.Where(o => o is IDisposable);
            foreach (var obj in disposableObjects)
            {
                if (obj is IDisposable dispObject)
                    dispObject.Dispose();
            }
            _objectContainer.Clear();
        }

        #endregion
    }
}

