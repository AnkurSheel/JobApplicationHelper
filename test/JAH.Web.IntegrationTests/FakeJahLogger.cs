using System;
using System.IO;

using JAH.Logger;

using Serilog;
using Serilog.Events;

using Xunit.Abstractions;

namespace JAH.Web.IntegrationTests
{
    public class FakeJahLogger : IJahLogger, IDisposable
    {
        private readonly StringWriter _stringWriter = new StringWriter();

        private readonly Serilog.Core.Logger _logger;

        private ITestOutputHelper _outputHelper;

        public FakeJahLogger()
        {
            _logger = new LoggerConfiguration().WriteTo.TextWriter(_stringWriter).CreateLogger();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetOutputHelper(ITestOutputHelper value)
        {
            _outputHelper = value;
        }

        public void WritePerf(LogDetail info)
        {
        }

        public void WriteDiagnostics(LogDetail info)
        {
        }

        public void WriteUsage(LogDetail info)
        {
            Write(info, "Usage");
        }

        public void WriteError(LogDetail info)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stringWriter?.Dispose();
            }
        }

        private void Write(LogDetail info, string logType)
        {
            var buf = _stringWriter.GetStringBuilder();
            buf.Clear();

            _logger.Write(LogEventLevel.Information, "{logType}: {@LogDetail}", logType, info);
            _outputHelper.WriteLine(_stringWriter.ToString());
        }
    }
}
