using Core.CrossCuttingConcerns.Serilog.ConfigurationModels;
using Core.CrossCuttingConcerns.Serilog.Messages;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcerns.Serilog.Logger;

public class MsSqlLogger : LoggerServiceBase
{
    public MsSqlLogger(IConfiguration configuration)
    {
        MsSqlConfiguration logConfig = configuration.GetSection("SeriLogConfigurations:MsSqlConfiguration").Get<MsSqlConfiguration>()
            ?? throw new Exception(SerilogMessages.NullOptionsMessage);

        MSSqlServerSinkOptions sinkOptions = new()
        {
            TableName = logConfig.TableName,
            AutoCreateSqlTable = logConfig.AutoCreateSqlTable
        };

        ColumnOptions columnOptions = new();
        global::Serilog.Core.Logger seriLogConfig = new LoggerConfiguration()
            .WriteTo.MSSqlServer(connectionString: logConfig.ConnectionString, sinkOptions: sinkOptions, columnOptions: columnOptions)
            .CreateLogger();

        Logger = seriLogConfig;
    }
}
