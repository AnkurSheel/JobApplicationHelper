using JAH.Logger;

namespace JAH.Web.IntegrationTests
{
    internal class FakeJahLogger : IJahLogger
    {
        public void WritePerf(LogDetail info)
        {
        }

        public void WriteDiagnostics(LogDetail info)
        {
        }

        public void WriteUsage(LogDetail info)
        {
        }

        public void WriteError(LogDetail info)
        {
        }
    }
}