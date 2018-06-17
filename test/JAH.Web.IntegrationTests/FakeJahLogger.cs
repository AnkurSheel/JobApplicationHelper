using System.IO;

using JAH.Logger;

using Serilog;
using Serilog.Events;

namespace JAH.Web.IntegrationTests
{
    public class FakeJahLogger : IJahLogger
    {
        private readonly StringWriter _stringWriter = new StringWriter();

        public FakeJahLogger()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.TextWriter(_stringWriter).CreateLogger();
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

        private void Write(LogDetail info)
        {
            var buf = _stringWriter.GetStringBuilder();
            buf.Clear();
            Log.Write(LogEventLevel.Information, "{@LogDetail}", info);
            LogMessageHelper.AddMessage(_stringWriter.ToString());
        }
    }
}
