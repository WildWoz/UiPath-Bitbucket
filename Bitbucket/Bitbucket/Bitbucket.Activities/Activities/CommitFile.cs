using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bitbucket.Activities.Properties;
using Pathoschild.Http.Client;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using UiPath.Shared.Activities.Utilities;
using Bitbucket;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Web.UI;

namespace Bitbucket.Activities
{
    [LocalizedDisplayName(nameof(Resources.CommitFile_DisplayName))]
    [LocalizedDescription(nameof(Resources.CommitFile_Description))]
    public class CommitFile : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.CommitFile_RepositoryLocation_DisplayName))]
        [LocalizedDescription(nameof(Resources.CommitFile_RepositoryLocation_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> RepositoryLocation { get; set; }

        [LocalizedDisplayName(nameof(Resources.CommitFile_FileToUpload_DisplayName))]
        [LocalizedDescription(nameof(Resources.CommitFile_FileToUpload_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> FileToUpload { get; set; }

        [LocalizedDisplayName(nameof(Resources.CommitFile_CommitMessage_DisplayName))]
        [LocalizedDescription(nameof(Resources.CommitFile_CommitMessage_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> CommitMessage { get; set; }

        [LocalizedDisplayName(nameof(Resources.CommitFile_WorkspaceUUIDOrSlug_DisplayName))]
        [LocalizedDescription(nameof(Resources.CommitFile_WorkspaceUUIDOrSlug_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> WorkspaceUUIDOrSlug { get; set; }

        [LocalizedDisplayName(nameof(Resources.CommitFile_RepositoryUUIDOrSlug_DisplayName))]
        [LocalizedDescription(nameof(Resources.CommitFile_RepositoryUUIDOrSlug_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> RepositoryUUIDOrSlug { get; set; }

        [LocalizedDisplayName(nameof(Resources.CommitFile_BranchName_DisplayName))]
        [LocalizedDescription(nameof(Resources.CommitFile_BranchName_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> BranchName { get; set; }

        [LocalizedDisplayName(nameof(Resources.CommitFile_JsonResult_DisplayName))]
        [LocalizedDescription(nameof(Resources.CommitFile_JsonResult_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<JObject> JsonResult { get; set; }
        #endregion


        #region Constructors

        public CommitFile()
        {
            Constraints.Add(item: ActivityConstraints.HasParentType<CommitFile, BitbucketAPIScope>(validationMessage: string.Format(Resources.ValidationScope_Error, Resources.BitbucketAPIScope_DisplayName)));
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (RepositoryLocation == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(RepositoryLocation)));
            if (FileToUpload == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(FileToUpload)));
            if (CommitMessage == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(CommitMessage)));
            if (WorkspaceUUIDOrSlug == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(WorkspaceUUIDOrSlug)));
            if (RepositoryUUIDOrSlug == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(RepositoryUUIDOrSlug)));

            // Check for valid branch name value
            if (BranchName.GetArgumentLiteralValue() == "") metadata.AddValidationError(Resources.ValidationValue_Error_CommitFile_BranchName);
            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Object Container: Use objectContainer.Get<T>() to retrieve objects from the scope
            var objectContainer = context.GetFromContext<IObjectContainer>(BitbucketAPIScope.ParentContainerPropertyTag);
            var client = objectContainer.Get<FluentClient>();

            // Inputs
            var repositoryLocation = RepositoryLocation.Get(context);
            var fileToUpload = FileToUpload.Get(context);
            var commitMessage = CommitMessage.Get(context);
            var repositoryUUIDOrSlug = RepositoryUUIDOrSlug.Get(context);
            var workspaceUUIDOrSlug = WorkspaceUUIDOrSlug.Get(context);
            var branchName = BranchName.Get(context);

            // Validate whether Workspace UUID or Name provided (assume name will never be a GUID format)
            if (Validation.IsUUID(workspaceUUIDOrSlug))
            {
                HttpUtility.UrlEncode(workspaceUUIDOrSlug);
            }

            // Validate whether Repository UUID or Slug provided (assume slug will never be a GUID format)
            if (Validation.IsUUID(repositoryUUIDOrSlug))
            {
                HttpUtility.UrlEncode(repositoryUUIDOrSlug);
            }

            // Create standard request URI
            var uri = "repositories/" + workspaceUUIDOrSlug + "/" + repositoryUUIDOrSlug + "/src";

            // Initialise and populate multipart content
            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(new ByteArrayContent(File.ReadAllBytes(fileToUpload)), repositoryLocation, Path.GetFileName(fileToUpload));
            multipartContent.Add(new StringContent(commitMessage), "message");

            // Check if optional branch name parameter provided. Add to request if not null.
            if (branchName != null)
            {
                multipartContent.Add(new StringContent(branchName), "branch");
            }

            // Execution Logic for all types of API requests available for this endpoint
            var response = new JObject();
            try
            {
                response = await AsyncRequests.PostRequest_WithBody(client, uri, cancellationToken, multipartContent);
            }
            catch (ApiException ex) // Catches any API exception and returns the message
            {
                var responseText = await ex.Response.AsString();
                var exceptionMessageJson = JObject.Parse(responseText).GetValue("error");

                // Outputs - Exception message
                return (ctx) =>
                {
                    JsonResult.Set(ctx, exceptionMessageJson);
                };
            }


            // Outputs - API response as JObject
            return (ctx) =>
            {
                JsonResult.Set(ctx, response);
            };
        }
        #endregion
    }
}

