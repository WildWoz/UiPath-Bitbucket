using System.ComponentModel;


namespace Bitbucket.Enums
{
    public enum RequestTypes
    {
        [Description("Read Repository (GET)")]
        GET,
        [Description("Create Repository (POST)")]
        POST,
        [Description("Delete Repository (DELETE)")]
        DELETE
    }
}
