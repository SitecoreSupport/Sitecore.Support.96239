namespace Sitecore.Support.Data.DataProviders.Sql
{
  using Sitecore.Data;
  using Sitecore.Data.DataProviders.Sql;
  using Sitecore.Globalization;
  using Sitecore.Support.Data.SqlServer;
  using System.Collections.Generic;
  public class FieldSharingConverter : SqlDataProvider.FieldSharingConverter
  {
    private readonly SqlServerDataProvider owner;

    public FieldSharingConverter(SqlServerDataProvider owner) : base(owner)
    {
      this.owner = owner;
    }

    protected override bool MoveDataToVersionedFromShared(ID fieldId, ID rootitemId)
    {
      this.DeleteUnversionedFields(fieldId, rootitemId);
      this.DeleteVersionedFields(fieldId, rootitemId);
      List<ID> list = this.ListItemIdsFromSharedFields(fieldId);
      foreach (ID current in list)
      {
        List<Language> list2 = this.ListLanguagesFromVersionedFields(current);
        foreach (Language current2 in list2)
        {
          List<Sitecore.Data.Version> versionsFromVersionedFields = this.GetVersionsFromVersionedFields(current, current2);
          foreach (Sitecore.Data.Version current3 in versionsFromVersionedFields)
          {
            this.owner.Api.Execute(" INSERT INTO {0}VersionedFields{1}({0}ItemId{1}, {0}Language{1}, {0}Version{1}, {0}FieldId{1}, {0}Value{1}, {0}Created{1}, {0}Updated{1})      SELECT {0}ItemId{1}, {2}language{3}, {2}version{3}, {0}FieldId{1}, {0}Value{1}, {0}Created{1}, {0}Updated{1}        FROM {0}SharedFields{1} sf        WHERE sf.{0}ItemId{1} = {2}itemId{3} AND sf.{0}FieldId{1} = {2}fieldId{3}", new object[]
            {
                            "language",
                            current2,
                            "version",
                            current3,
                            "fieldId",
                            fieldId,
                            "itemId",
                            current
            });
          }
        }
      }
      this.DeleteSharedFields(fieldId, rootitemId);
      return true;
    }

    protected override bool MoveDataToVersionedFromUnversioned(ID fieldId, ID rootitemId)
    {
      this.DeleteSharedFields(fieldId, rootitemId);
      this.DeleteVersionedFields(fieldId, rootitemId);
      List<ID> list = this.ListItemIdsFromUnversionedFields(fieldId);
      foreach (ID current in list)
      {
        List<Language> list2 = this.ListLanguagesFromUnversionedFields(current);
        foreach (Language current2 in list2)
        {
          List<Sitecore.Data.Version> versionsFromVersionedFields = this.GetVersionsFromVersionedFields(current, current2);
          foreach (Sitecore.Data.Version current3 in versionsFromVersionedFields)
          {
            this.owner.Api.Execute(" INSERT INTO {0}VersionedFields{1}({0}ItemId{1}, {0}Language{1}, {0}Version{1}, {0}FieldId{1}, {0}Value{1}, {0}Created{1}, {0}Updated{1})      SELECT {0}ItemId{1}, {0}Language{1}, {2}version{3}, {0}FieldId{1}, {0}Value{1}, {0}Created{1}, {0}Updated{1}        FROM {0}UnversionedFields{1} uf        WHERE uf.{0}ItemId{1} = {2}itemId{3} AND uf.{0}FieldId{1} = {2}fieldId{3} AND uf.{0}Language{1} = {2}language{3}", new object[]
            {
                            "language",
                            current2,
                            "version",
                            current3,
                            "fieldId",
                            fieldId,
                            "itemId",
                            current
            });
          }
        }
      }
      this.DeleteUnversionedFields(fieldId, rootitemId);
      return true;
    }

    private List<ID> ListItemIdsFromSharedFields(ID fieldId)
    {
      List<ID> list = new List<ID>();
      string sql = " SELECT {0}ItemId{1}   FROM {0}SharedFields{1} (NOLOCK)  WHERE {0}FieldId{1} = {2}fieldId{3}";
      using (DataProviderReader dataProviderReader = this.owner.Api.CreateReader(sql, new object[]
      {
                "fieldId",
                fieldId
      }))
      {
        while (dataProviderReader.Read())
        {
          list.Add(this.owner.Api.GetId(0, dataProviderReader));
        }
      }
      return list;
    }

    private List<ID> ListItemIdsFromUnversionedFields(ID fieldId)
    {
      List<ID> list = new List<ID>();
      string sql = "   SELECT {0}ItemId{1}     FROM {0}UnversionedFields{1} (NOLOCK)    WHERE {0}FieldId{1} = {2}fieldId{3} GROUP BY {0}ItemId{1}";
      using (DataProviderReader dataProviderReader = this.owner.Api.CreateReader(sql, new object[]
      {
                "fieldId",
                fieldId
      }))
      {
        while (dataProviderReader.Read())
        {
          list.Add(this.owner.Api.GetId(0, dataProviderReader));
        }
      }
      return list;
    }

    private List<Language> ListLanguagesFromVersionedFields(ID itemId)
    {
      return this.ListLanguagesFromTable("VersionedFields", itemId);
    }

    private List<Language> ListLanguagesFromUnversionedFields(ID itemId)
    {
      return this.ListLanguagesFromTable("UnversionedFields", itemId);
    }

    private List<Language> ListLanguagesFromTable(string tableName, ID itemId)
    {
      List<Language> list = new List<Language>();
      using (DataProviderReader dataProviderReader = this.owner.Api.CreateReader("   SELECT {0}Language{1}     FROM {0}VersionedFields{1} (NOLOCK)    WHERE {0}ItemId{1} = {2}itemId{3} GROUP BY {0}Language{1}", new object[]
      {
                "itemId",
                itemId
      }))
      {
        while (dataProviderReader.Read())
        {
          list.Add(this.owner.Api.GetLanguage(0, dataProviderReader));
        }
      }
      return list;
    }

    private List<Sitecore.Data.Version> GetVersionsFromVersionedFields(ID itemId, Language language)
    {
      List<Sitecore.Data.Version> list = new List<Sitecore.Data.Version>();
      using (DataProviderReader dataProviderReader = this.owner.Api.CreateReader("   SELECT {0}Version{1}     FROM {0}VersionedFields{1} (NOLOCK)    WHERE {0}ItemId{1} = {2}itemId{3} AND {0}Language{1} = {2}language{3} GROUP BY {0}Version{1}", new object[]
      {
                "itemId",
                itemId,
                "language",
                language
      }))
      {
        while (dataProviderReader.Read())
        {
          list.Add(Sitecore.Data.Version.Parse(this.owner.Api.GetInt(0, dataProviderReader)));
        }
      }
      return list;
    }
  }
}