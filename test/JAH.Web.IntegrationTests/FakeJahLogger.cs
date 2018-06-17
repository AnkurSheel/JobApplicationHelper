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

        public FakeJahLogger()
        {
            _logger = new LoggerConfiguration().WriteTo.TextWriter(_stringWriter).CreateLogger();
        }

        public ITestOutputHelper OutputHelper { private get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void WritePerf(LogDetail info)
        {
        }

        public void WriteDiagnostics(LogDetail info)
        {
        }

        public void WriteUsage(LogDetail info)
        {
            Write(info);
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

        private void Write(LogDetail info)
        {
            var buf = _stringWriter.GetStringBuilder();
            buf.Clear();
            _logger.Write(LogEventLevel.Information, "{@LogDetail}", info);
            OutputHelper.WriteLine(_stringWriter.ToString());
            //LogMessageHelper.AddMessage(_stringWriter.ToString());
        }
    }
}
