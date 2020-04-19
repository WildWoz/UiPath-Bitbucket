using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using System.ComponentModel.Design;
using Bitbucket.Activities.Design.Designers;
using Bitbucket.Activities.Design.Properties;
using UiPath.Shared.Activities.Design.Editors;

namespace Bitbucket.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            #region Setup

            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            #endregion Setup


            builder.AddCustomAttributes(typeof(BitbucketAPIScope), categoryAttribute);
            builder.AddCustomAttributes(typeof(BitbucketAPIScope), new DesignerAttribute(typeof(BitbucketAPIScopeDesigner)));
            builder.AddCustomAttributes(typeof(BitbucketAPIScope), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(GetRepositories), categoryAttribute);
            builder.AddCustomAttributes(typeof(GetRepositories), new DesignerAttribute(typeof(GetRepositoriesDesigner)));
            builder.AddCustomAttributes(typeof(GetRepositories), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(ManageRepository), categoryAttribute);
            builder.AddCustomAttributes(typeof(ManageRepository), new DesignerAttribute(typeof(ManageRepositoryDesigner)));
            builder.AddCustomAttributes(typeof(ManageRepository), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(CommitFile), categoryAttribute);
            builder.AddCustomAttributes(typeof(CommitFile), new DesignerAttribute(typeof(CommitFileDesigner)));
            builder.AddCustomAttributes(typeof(CommitFile), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(GetWorkspaces), categoryAttribute);
            builder.AddCustomAttributes(typeof(GetWorkspaces), new DesignerAttribute(typeof(GetWorkspacesDesigner)));
            builder.AddCustomAttributes(typeof(GetWorkspaces), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
