using System.Collections;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;

namespace Kinetix.DataAccess.Sql.SqlServer;

/// <summary>
/// Collection de paramètres pour les commandes Sql Server.
/// </summary>
internal class SqlServerParameterCollection : SqlParameterCollection
{
    /// <summary>
    /// Nom de la colonne dans le type table.
    /// </summary>
    private const string ColDataTypeName = "n";

    /// <summary>
    /// Nom du type SQL Server dédié aux int.
    /// </summary>
    private const string IntDataType = "type_int_list";

    /// <summary>
    /// Nom du type SQL Server dédié aux varchar.
    /// </summary>
    private const string VarCharDataType = "type_varchar_list";

    /// <summary>
    /// Nom du type SQL Server dédié aux uniqueidentifier.
    /// </summary>
    private const string UniqueIdentifierDataType = "type_uniqueidentifier_list";

    /// <summary>
    /// Taille du champ du type SQL Server dédié aux varchar.
    /// </summary>
    private const int VarCharLength = 255;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="command">Commande SQL.</param>
    public SqlServerParameterCollection(IDbCommand command)
        : base(command)
    {
    }

    /// <inheritdoc />
    public override SqlDataParameter AddInParameter(string parameterName, IEnumerable<int> list)
    {
        return AddInParameter(parameterName, list, IntDataType, SqlDbType.Int);
    }

    /// <inheritdoc />
    public override SqlDataParameter AddInParameter(string parameterName, IEnumerable<string> list)
    {
        return AddInParameter(parameterName, list, VarCharDataType, SqlDbType.VarChar);
    }

    /// <inheritdoc />
    public override SqlDataParameter AddInParameter(string parameterName, IEnumerable<Guid> list)
    {
        return AddInParameter(parameterName, list, UniqueIdentifierDataType, SqlDbType.UniqueIdentifier);
    }

    /// <inheritdoc />
    public override SqlDataParameter AddTableParameter<T>(ICollection<T> collection)
    {
        var parameter = new SqlServerParameterBeanCollection<T>(null, collection, false).CreateParameter(InnerCommand);
        List.Add(parameter);
        return parameter;
    }

    private SqlDataParameter AddInParameter(string parameterName, IEnumerable list, string typeName, SqlDbType sqlDbType)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            throw new ArgumentNullException("parameterName");
        }

        if (list == null)
        {
            throw new ArgumentNullException("list");
        }

        var metaData = sqlDbType == SqlDbType.VarChar ? new SqlMetaData(ColDataTypeName, sqlDbType, VarCharLength) : new SqlMetaData(ColDataTypeName, sqlDbType);
        var dataRecordList = new List<SqlDataRecord>();
        foreach (var item in list)
        {
            var record = new SqlDataRecord(metaData);
            record.SetValue(0, item);
            dataRecordList.Add(record);
        }

        if (dataRecordList.Count == 0)
        {
            var record = new SqlDataRecord(metaData);
            record.SetValue(0, null);
            dataRecordList.Add(record);
        }

        var parameter = new SqlDataParameter(InnerCommand.CreateParameter())
        {
            ParameterName = ParamValue + parameterName,
            Direction = ParameterDirection.Input,
            Value = dataRecordList
        };

        ((SqlParameter)parameter.InnerParameter).SqlDbType = SqlDbType.Structured;
        ((SqlParameter)parameter.InnerParameter).TypeName = typeName;

        Add(parameter);

        return parameter;
    }
}
