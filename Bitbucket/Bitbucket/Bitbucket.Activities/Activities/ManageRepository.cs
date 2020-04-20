using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using Bitbucket.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using UiPath.Shared.Activities.Utilities;
using Pathoschild.Http.Client;
using Bitbucket.Enums;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Web;

namespace Bitbucket.Activities
{
    [LocalizedDisplayName(nameof(Resources.ManageRepository_DisplayName))]
    [LocalizedDescription(nameof(Resources.ManageRepository_Description))]
    public class ManageRepository : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.ManageRepository_WorkspaceUUIDOrSlug_DisplayName))]
        [LocalizedDescription(nameof(Resources.ManageRepository_WorkspaceUUIDOrSlug_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> WorkspaceUUIDOrSlug { get; set; }

        [LocalizedDisplayName(nameof(Resources.ManageRepository_RepositoryUUIDOrSlug_DisplayName))]
        [LocalizedDescription(nameof(Resources.ManageRepository_RepositoryUUIDOrSlug_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> RepositoryUUIDOrSlug { get; set; }

        [LocalizedDisplayName(nameof(Resources.ManageRepository_JsonResult_DisplayName))]
        [LocalizedDescription(nameof(Resources.ManageRepository_JsonResult_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<JObject> JsonResult { get; set; }

        [LocalizedDisplayName(nameof(Resources.ManageRepository_Action_DisplayName))]
        [LocalizedDescription(nameof(Resources.ManageRepository_Action_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        [TypeConverter(typeof(EnumNameConverter<RequestTypes>))]
        public RequestTypes Action { get; set; }

        #endregion


        #region Constructors

        public ManageRepository()
        {
            Constraints.Add(item: ActivityConstraints.HasParentType<ManageRepository, BitbucketAPIScope>(validationMessage: string.Format(Resources.ValidationScope_Error, Resources.BitbucketAPIScope_DisplayName)));
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (WorkspaceUUIDOrSlug == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(WorkspaceUUIDOrSlug)));
            if (RepositoryUUIDOrSlug == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(RepositoryUUIDOrSlug)));

            // When Create repository (POST) is selected make sure user is using slug not UUID
            if (Action == RequestTypes.POST && Validation.IsUUID(RepositoryUUIDOrSlug.GetArgumentLiteralValue())) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error_ManageRepository_RepositoryUUIDOrSlug_NotUUID, nameof(RepositoryUUIDOrSlug)));
            
            // Check for valid UUID or Slug value
            if (RepositoryUUIDOrSlug.GetArgumentLiteralValue() == "") metadata.AddValidationError(Resources.ValidationValue_Error_ManageRepository_RepositoryUUIDOrSlug);

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {

            // Object Container: Use objectContainer.Get<T>() to retrieve objects from the scope
            var objectContainer = context.GetFromContext<IObjectContainer>(BitbucketAPIScope.ParentContainerPropertyTag);
            var client = objectContainer.Get<FluentClient>();

            // Inputs
            var workspaceUUIDOrSlug = WorkspaceUUIDOrSlug.Get(context);
            var repositoryUUIDOrSlug = RepositoryUUIDOrSlug.Get(context);
            var requestType = Enum.GetName(typeof(RequestTypes), (int)Action);

            // Validate whether Workspace UUID or Slug provided (assume name will never be a GUID format)
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
            var uri = "repositories/" + workspaceUUIDOrSlug + "/" + repositoryUUIDOrSlug;

            // Initialise
            var response = new JObject();
            var exceptionHandler = new ApiExceptionHandler();

            // Execution Logic for all types of API requests available for this endpoint
            try
            {
                switch (requestType)
                {
                    case "GET":
                        {
                            response = await AsyncRequests.GetRequest(client, uri, cancellationToken);
                            break;
                        }
                    case "POST":
                        {
                            response = await AsyncRequests.PostRequest_NoBody(client, uri, cancellationToken);
                            break;
                        }
                    case "DELETE":
                        {
                            response = await AsyncRequests.DeleteRequest(client, uri, cancellationToken);
                            break;
                        }
                }
            }
            catch (ApiException ex) // Catches any API exception and returns the message
            {
                await exceptionHandler.ParseExceptionAsync(ex);
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

