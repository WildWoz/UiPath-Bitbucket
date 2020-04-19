using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bitbucket.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using UiPath.Shared.Activities.Utilities;
using Pathoschild.Http.Client;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Bitbucket.Activities
{
    [LocalizedDisplayName(nameof(Resources.GetRepositories_DisplayName))]
    [LocalizedDescription(nameof(Resources.GetRepositories_Description))]
    public class GetRepositories : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetRepositories_WorkspaceUUIDOrSlug_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetRepositories_WorkspaceUUIDOrSlug_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> WorkspaceUUIDOrSlug { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetRepositories_RepositoriesJObject_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetRepositories_RepositoriesJObject_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<JObject> RepositoriesJObject { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetRepositories_RepositoriesSlugList_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetRepositories_RepositoriesSlugList_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<List<string>> RepositoriesSlugList { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetRepositories_RepositoriesUUIDList_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetRepositories_RepositoriesUUIDList_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<List<string>> RepositoriesUUIDList { get; set; }
        #endregion

        #region Constructors

        public GetRepositories()
        {
            Constraints.Add(item: ActivityConstraints.HasParentType<GetRepositories, BitbucketAPIScope>(validationMessage: string.Format(Resources.ValidationScope_Error, Resources.BitbucketAPIScope_DisplayName)));
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (WorkspaceUUIDOrSlug == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(WorkspaceUUIDOrSlug)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Object Container: Use objectContainer.Get<T>() to retrieve objects from the scope
            var objectContainer = context.GetFromContext<IObjectContainer>(BitbucketAPIScope.ParentContainerPropertyTag);
            var client = objectContainer.Get<FluentClient>();

            // Inputs
            var workspaceUUIDOrSlug = WorkspaceUUIDOrSlug.Get(context);

            // Validate whether Workspace UUID or Slug provided (assume name will never be a GUID format)
            if (Validation.IsUUID(workspaceUUIDOrSlug))
            {
                HttpUtility.UrlEncode(workspaceUUIDOrSlug);
            }

            // Create request URI
            var uri = "repositories/" + workspaceUUIDOrSlug;

            // Perform request
            var response = await AsyncRequests.GetRequest(client, uri, cancellationToken);

            // Create slug list
            var repositorySlugList = JObjectParser.JObjectToSlugList(response);

            // Create UUID list
            var repositoryUUIDList = JObjectParser.JObjectToUUIDList(response);

            // Outputs
            return (ctx) =>
            {
                RepositoriesJObject.Set(ctx, response);
                RepositoriesSlugList.Set(ctx, repositorySlugList);
                RepositoriesUUIDList.Set(ctx, repositoryUUIDList);
            };
        }
        #endregion
    }
}

