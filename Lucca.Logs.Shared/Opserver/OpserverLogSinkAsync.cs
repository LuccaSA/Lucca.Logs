using Dapper;
using Lucca.Logs.Shared.Exceptional;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Serilog.Events;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Lucca.Logs.Shared.Opserver
{
    public class OpserverLogSinkAsync : ILogEventSinkAsync
    {
        private readonly IOptions<LuccaLoggerOptions> _options;

        private readonly string _tableName;
        private readonly TimeSpan _rollupPeriod = TimeSpan.FromDays(1);

        public OpserverLogSinkAsync(IOptions<LuccaLoggerOptions> options)
        {
            _options = options;
            _tableName = options.Value.ExceptionTableName;
        }

        public async Task EmitAsync(LogEvent logEvent)
        {
            var error = logEvent.ToExceptionalError();
            if (error != null)
            {
                await LogErrorAsync(error);
            }
        }

        /// <summary>
        /// Asynchronously logs the error to SQL.
        /// If the roll-up conditions are met, then the matching error will have a 
        /// DuplicateCount += @DuplicateCount (usually 1, unless in retry) rather than a distinct new row for the error.
        /// </summary>
        /// <param name="error">The error to log.</param>
        private async Task<bool> LogErrorAsync(Error error)
        {
            var cs = _options.Value.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
            {
                return false;
            }

            using var c = new SqlConnection(cs);

            if (error.ErrorHash.HasValue)
            {
                var queryParams = GetUpdateParams(error);
                // if we found an error that's a duplicate, jump out
                if (await c.ExecuteAsync(SqlLogUpdate, queryParams).ConfigureAwait(false) > 0)
                {
                    error.GUID = queryParams.Get<Guid>("@newGUID");
                    return true;
                }
            }

            error.FullJson = error.ToJson();
            return (await c.ExecuteAsync(SqlLogInsert, GetInsertParams(error)).ConfigureAwait(false)) > 0;
        }

        private object GetInsertParams(Error error) => new
        {
            error.GUID,
            ApplicationName = error.ApplicationName.Truncate(50),
            Category = error.Category.Truncate(100),
            MachineName = error.MachineName.Truncate(50),
            error.CreationDate,
            Type = error.Type.Truncate(100),
            error.IsProtected,
            Host = error.Host.Truncate(100),
            Url = error.UrlPath.Truncate(500),
            HTTPMethod = error.HTTPMethod.Truncate(10),
            error.IPAddress,
            Source = error.Source.Truncate(100),
            Message = error.Message.Truncate(1000),
            error.Detail,
            error.StatusCode,
            error.FullJson,
            error.ErrorHash,
            error.DuplicateCount,
            error.LastLogDate
        };

        private DynamicParameters GetUpdateParams(Error error)
        {
            var queryParams = new DynamicParameters(new
            {
                error.DuplicateCount,
                error.ErrorHash,
                error.CreationDate,
                ApplicationName = error.ApplicationName.Truncate(50),
                minDate = DateTime.UtcNow.Subtract(_rollupPeriod)
            });
            queryParams.Add("@newGUID", dbType: DbType.Guid, direction: ParameterDirection.Output);
            return queryParams;
        }

        private string _sqlLogUpdate;
        private string _sqlLogInsert;


        private string SqlLogUpdate => _sqlLogUpdate ??= $@"
Update {_tableName} 
   Set DuplicateCount = DuplicateCount + @DuplicateCount,
       LastLogDate = (Case When LastLogDate Is Null Or @CreationDate > LastLogDate Then @CreationDate Else LastLogDate End),
       @newGUID = GUID
 Where Id In (Select Top 1 Id
                From {_tableName} 
               Where ErrorHash = @ErrorHash
                 And ApplicationName = @ApplicationName
                 And DeletionDate Is Null
                 And CreationDate >= @minDate)";

        private string SqlLogInsert => _sqlLogInsert ??= $@"
Insert Into {_tableName} (GUID, ApplicationName, Category, MachineName, CreationDate, Type, IsProtected, Host, Url, HTTPMethod, IPAddress, Source, Message, Detail, StatusCode, FullJson, ErrorHash, DuplicateCount, LastLogDate)
Values (@GUID, @ApplicationName, @Category, @MachineName, @CreationDate, @Type, @IsProtected, @Host, @Url, @HTTPMethod, @IPAddress, @Source, @Message, @Detail, @StatusCode, @FullJson, @ErrorHash, @DuplicateCount, @LastLogDate)";

    }
}
