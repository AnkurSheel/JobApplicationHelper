using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace JAH.Logger
{
    public class JahLogger : IJahLogger
    {
        private readonly ILogger _perfLogger;

        private readonly ILogger _usageLogger;

        private readonly ILogger _errorLogger;

        private readonly ILogger _diagnosticLogger;

        private readonly string _messagetemplate;

        public JahLogger(string connectionString)
        {
            try
            {
                // Serilog.Debugging.SelfLog.Enable(msg =>
                // {
                //     Debug.Print(msg);
                //     Debugger.Break();
                // });
                var columnOptions = GetSqlColumnOptions();
                foreach (var option in columnOptions.AdditionalDataColumns)
                {
                    _messagetemplate += "{" + $"{option.ColumnName}" + "}";
                }

                _perfLogger = new LoggerConfiguration()
                              .WriteTo.MSSqlServer(connectionString,
                                                   "PerfLogs",
                                                   autoCreateSqlTable: true,
                                                   columnOptions: GetSqlColumnOptions(),
                                                   batchPostingLimit: 1)
                              .CreateLogger();

                _usageLogger = new LoggerConfiguration()
                               .WriteTo.MSSqlServer(connectionString,
                                                    "UsageLogs",
                                                    autoCreateSqlTable: true,
                                                    columnOptions: GetSqlColumnOptions(),
                                                    batchPostingLimit: 1)
                               .CreateLogger();

                _errorLogger = new LoggerConfiguration()
                               .WriteTo.MSSqlServer(connectionString,
                                                    "ErrorLogs",
                                                    autoCreateSqlTable: true,
                                                    columnOptions: GetSqlColumnOptions(),
                                                    batchPostingLimit: 1)
                               .CreateLogger();

                _diagnosticLogger = new LoggerConfiguration()
                                    .WriteTo.MSSqlServer(connectionString,
                                                         "DiagnosticLogs",
                                                         autoCreateSqlTable: true,
                                                         columnOptions: GetSqlColumnOptions(),
                                                         batchPostingLimit: 1)
                                    .CreateLogger();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected JahLogger()
        {
        }

        public void WritePerf(LogDetail info)
        {
            Write(_perfLogger, info, LogEventLevel.Information);
        }

        public void WriteDiagnostics(LogDetail info)
        {
            var writeDiagnostics = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableDiagnostics"]);
            if (!writeDiagnostics)
            {
                return;
            }

            Write(_diagnosticLogger, info, LogEventLevel.Information);
        }

        public void WriteUsage(LogDetail info)
        {
            Write(_usageLogger, info, LogEventLevel.Information);
        }

        public void WriteError(LogDetail info)
        {
            if (info.Exception != null)
            {
                info.Message = GetMessageFromException(info.Exception);
            }

            Write(_errorLogger, info, LogEventLevel.Error);
        }

        protected virtual void Write(ILogger logger, LogDetail info, LogEventLevel logEventLevel)
        {
            object[] values = info.GetType().GetProperties().Select(property => property.GetValue(info)).ToArray();
            logger.Write(logEventLevel, _messagetemplate, values);
        }

        private static string GetMessageFromException(Exception ex)
        {
            while (true)
            {
                if (ex.InnerException == null)
                {
                    return ex.Message;
                }

                ex = ex.InnerException;
            }
        }

        private ColumnOptions GetSqlColumnOptions()
        {
            var colOptions = new ColumnOptions();
            colOptions.Store.Remove(StandardColumn.Properties);
            colOptions.Store.Remove(StandardColumn.MessageTemplate);
            colOptions.Store.Remove(StandardColumn.Message);
            colOptions.Store.Remove(StandardColumn.Exception);
            colOptions.Store.Remove(StandardColumn.TimeStamp);
            colOptions.Store.Remove(StandardColumn.Level);

            colOptions.AdditionalDataColumns = new Collection<DataColumn>();

            foreach (var property in typeof(LogDetail).GetProperties())
            {
                var columnType = property.PropertyType;
                if (columnType.IsGenericType && columnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                    columnType = columnType.GetGenericArguments()[0];
                }

                if (columnType.IsClass)
                {
                    columnType = typeof(string);
                }

                colOptions.AdditionalDataColumns.Add(new DataColumn { DataType = columnType, ColumnName = property.Name });
            }

            return colOptions;
        }
    }
}
