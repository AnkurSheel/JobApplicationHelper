namespace JAH.Logger
{
    public interface IJahLogger
    {
        void WritePerf(LogDetail info);

        void WriteDiagnostics(LogDetail info);

        void WriteUsage(LogDetail info);

        void WriteError(LogDetail info);
    }
}
