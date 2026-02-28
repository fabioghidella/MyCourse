using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions.Infrastructure;

namespace MyCourse.Models.Services.Infrastructure;

public class SqliteDatabaseAccessor : IDatabaseAccessor
{
    private readonly ILogger<SqliteDatabaseAccessor> logger;
    private readonly IOptionsMonitor<ConnectionStringsOptions> connectionStringOptions;

    public SqliteDatabaseAccessor(ILogger<SqliteDatabaseAccessor> logger, IOptionsMonitor<ConnectionStringsOptions> connectionStringOptions)
    {
        this.logger = logger;
        this.connectionStringOptions = connectionStringOptions;
    }

    public async Task<int> CommandAsync(FormattableString formattableCommand, CancellationToken token)
    {
        try
        {
            using SqliteConnection conn = await GetOpenedConnection(token);
            using SqliteCommand cmd = GetCommand(formattableCommand, conn);
            int affectedRows = await cmd.ExecuteNonQueryAsync(token);
            return affectedRows;
        }
        catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
        {
            throw new ConstraintViolationException(exc);
        }
    }

    public async Task<T> QueryScalarAsync<T>(FormattableString formattableQuery, CancellationToken token)
    {
        try
        {
            using SqliteConnection conn = await GetOpenedConnection(token);
            using SqliteCommand cmd = GetCommand(formattableQuery, conn);
            object result = await cmd.ExecuteScalarAsync();
            return (T)Convert.ChangeType(result, typeof(T));
        }
        catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
        {
            throw new ConstraintViolationException(exc);
        }
    }

    public async Task<DataSet> QueryAsync(FormattableString formattableQuery, CancellationToken token)
    {
        logger.LogInformation(formattableQuery.Format, formattableQuery.GetArguments());

        using SqliteConnection conn = await GetOpenedConnection(token);
        using SqliteCommand cmd = GetCommand(formattableQuery, conn);

        //Send the query to the database and get a SqliteDataReader
        //to read the results

        try
        {
            using var reader = await cmd.ExecuteReaderAsync(token);
            DataSet dataSet = new();

            //Create one DataTable for each result set
            //returned by the SqliteDataReader
            do
            {
                DataTable dataTable = new();
                dataSet.Tables.Add(dataTable);
                dataTable.Load(reader);
            } while (!reader.IsClosed);

            return dataSet;
        }
        catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
        {
            throw new ConstraintViolationException(exc);
        }

    }

    private static SqliteCommand GetCommand(FormattableString formattableQuery, SqliteConnection conn)
    {
        //Build SqliteParameters from the FormattableString arguments
        var queryArguments = formattableQuery.GetArguments();
        List<SqliteParameter> sqliteParameters = new();
        for (var i = 0; i < queryArguments.Length; i++)
        {
            if (queryArguments[i] is Sql)
            {
                continue;
            }
            SqliteParameter parameter = new(name: i.ToString(), value: queryArguments[i] ?? DBNull.Value);
            sqliteParameters.Add(parameter);
            queryArguments[i] = "@" + i;
        }
        string query = formattableQuery.ToString();

        SqliteCommand cmd = new(query, conn);
        //Attach the SqliteParameters to the SqliteCommand
        cmd.Parameters.AddRange(sqliteParameters);
        return cmd;
    }

    private async Task<SqliteConnection> GetOpenedConnection(CancellationToken token)
    {
        //Connect to the SQLite database, send the query, and read the results
        int retries = 3;
        while (true)
        {
            try
            {
                retries--;
                SqliteConnection conn = new(connectionStringOptions.CurrentValue.Default);
                await conn.OpenAsync(token);
                return conn;
            }
            catch (Exception) when (retries > 0)
            {
                await Task.Delay(1000);
            }
        }
    }
}
