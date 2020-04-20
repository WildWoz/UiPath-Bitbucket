using System;
using System.Activities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bitbucket.Activities.Properties;
using Newtonsoft.Json.Linq;
using Pathoschild.Http.Client;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using UiPath.Shared.Activities.Utilities;

namespace Bitbucket.Activities
{
    [LocalizedDisplayName(nameof(Resources.GetWorkspaces_DisplayName))]
    [LocalizedDescription(nameof(Resources.GetWorkspaces_Description))]
    public class GetWorkspaces : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetWorkspaces_WorkspacesJObject_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetWorkspaces_WorkspacesJObject_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<JObject> WorkspacesJObject { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetWorkspaces_WorkspacesSlugList_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetWorkspaces_WorkspacesSlugList_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<List<string>> WorkspacesSlugList { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetWorkspaces_WorkspacesUUIDList_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetWorkspaces_WorkspacesUUIDList_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<List<string>> WorkspacesUUIDList { get; set; }
        #endregion

        
        #region Constructors

        public GetWorkspaces()
        {
            Constraints.Add(item: ActivityConstraints.HasParentType<GetWorkspaces, BitbucketAPIScope>(validationMessage: string.Format(Resources.ValidationScope_Error, Resources.BitbucketAPIScope_DisplayName)));
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Object Container: Use objectContainer.Get<T>() to retrieve objects from the scope
            var objectContainer = context.GetFromContext<IObjectContainer>(BitbucketAPIScope.ParentContainerPropertyTag);
            var client = objectContainer.Get<FluentClient>();

            // Create request URI
            var uri = "workspaces";

            // Initialsie
            var response = new JObject();
            var workspaceSlugList = new List<string>();
            var workspaceUUIDList = new List<string>();
            var exceptionHandler = new ApiExceptionHandler();

            try
            {
                // Perform request
                response = await AsyncRequests.GetRequest(client, uri, cancellationToken);

                // Create slug list
                workspaceSlugList = JObjectParser.JObjectToSlugList(response);

                // Create UUID list
                workspaceUUIDList = JObjectParser.JObjectToUUIDList(response);

            }
            catch (ApiException ex) // Catches any API exception and returns the message
            {
                await exceptionHandler.ParseExceptionAsync(ex);
            }

            // Outputs
            return (ctx) =>
            {
                WorkspacesJObject.Set(ctx, response);
                WorkspacesSlugList.Set(ctx, workspaceSlugList);
                WorkspacesUUIDList.Set(ctx, workspaceUUIDList);
            };
        }

        #endregion
    }
}

