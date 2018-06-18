using System;
using System.IO;

using JAH.Logger;

using Serilog;
using Serilog.Events;

using Xunit.Abstractions;

namespace JAH.Web.IntegrationTests
{
    public class FakeJahLogger : JahLogger, IDisposable
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stringWriter?.Dispose();
            }
        }

        protected override void Write(ILogger logger, LogDetail info, LogEventLevel logEventLevel)
        {
            var buf = _stringWriter.GetStringBuilder();
            buf.Clear();

            _logger.Write(logEventLevel, "{@LogDetail}", info);
            _outputHelper.WriteLine(_stringWriter.ToString());
        }
    }
}
