using System.Collections.Generic;

using Xunit.Abstractions;

namespace JAH.Web.IntegrationTests
{
    public static class LogMessageHelper
    {
        private static readonly List<string> LogMessages = new List<string>();

        public static void AddMessage(string message)
        {
            LogMessages.Add(message);
        }

        public static void WriteOutput(ITestOutputHelper output)
        {
            foreach (var message in LogMessages)
            {
                output.WriteLine(message);
            }

            LogMessages.Clear();
        }
    }
}
