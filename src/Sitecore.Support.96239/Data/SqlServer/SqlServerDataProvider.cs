namespace Sitecore.Support.Data.SqlServer
{
  using Sitecore.Data.DataProviders.Sql;
  using Sitecore.Data.SqlServer;
  public class SqlServerDataProvider : Sitecore.Data.SqlServer.SqlServerDataProvider
  {
    private readonly SqlDataApi api;

    public new SqlDataApi Api
    {
      get
      {
        return this.api;
      }
    }

    public SqlServerDataProvider(string connectionString) : base(connectionString)
    {
      this.api = new SqlServerDataApi(connectionString);
    }

    protected override SqlDataProvider.FieldSharingConverter GetFieldSharingConverter()
    {
      return new Sitecore.Support.Data.DataProviders.Sql.FieldSharingConverter(this);
    }
  }
}