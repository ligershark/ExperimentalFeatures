using Microsoft.VisualStudio.Telemetry;

namespace ExperimentalFeatures
{
    public static class Telemetry
    {
        private const string _namespace = "WebTools/Experiments/";

        public static void ResetInvoked()
        {
            var telEvent = CreateTelemetryEvent("Reset");
            PostEventToDefaultSession(telEvent);
        }

        public static void RecordTimeToClose(int minutes)
        {
            var telEvent = CreateTelemetryEvent("TimeToClose");
            telEvent.Properties.Add("minutes", minutes);
            PostEventToDefaultSession(telEvent);
        }

        public static void InstallSuccess(string extensionId)
        {
            var telEvent = CreateTelemetryEvent("InstallSuccess");
            telEvent.Properties.Add("id", extensionId);
            PostEventToDefaultSession(telEvent);
        }

        public static void InstallFailure(string extensionId)
        {
            var telEvent = CreateTelemetryEvent("InstallFailure");
            telEvent.Properties.Add("id", extensionId);
            PostEventToDefaultSession(telEvent);
        }

        public static void UninstallSuccess(string extensionId)
        {
            var telEvent = CreateTelemetryEvent("UninstallSuccess");
            telEvent.Properties.Add("id", extensionId);
            PostEventToDefaultSession(telEvent);
        }

        public static void UninstallFailure(string extensionId)
        {
            var telEvent = CreateTelemetryEvent("UninstallFailure");
            telEvent.Properties.Add("id", extensionId);
            PostEventToDefaultSession(telEvent);
        }

        private static void PostEventToDefaultSession(TelemetryEvent vsTelemetryEvent)
        {
            TelemetryService.DefaultSession.PostEvent(vsTelemetryEvent);
        }

        private static TelemetryEvent CreateTelemetryEvent(string name)
        {
            return new TelemetryEvent(_namespace + name);
        }
    }
}
