using Microsoft.VisualStudio.Telemetry;

namespace ExperimentalFeatures
{
    public static class Telemetry
    {
        private const string _namespace = "WebTools/Experiments/";

        public static void ResetInvoked()
        {
            var telEvent = new UserTaskEvent(_namespace + "ResetInvoked", TelemetryResult.Success);
            TelemetryService.DefaultSession.PostEvent(telEvent);
        }

        public static void RecordTimeToClose(int minutes)
        {
            var telEvent = new OperationEvent(_namespace + "TimeToClose", TelemetryResult.Success);
            telEvent.Properties.Add("minutes", minutes);
            TelemetryService.DefaultSession.PostEvent(telEvent);
        }

        public static void Install(string extensionId, bool success)
        {
            var telEvent = new OperationEvent(_namespace + "Install", success ? TelemetryResult.Success : TelemetryResult.Failure);
            telEvent.Properties.Add("id", extensionId);
            TelemetryService.DefaultSession.PostEvent(telEvent);
        }

        public static void Uninstall(string extensionId, bool success)
        {
            var telEvent = new OperationEvent(_namespace + "Uninstall", success ? TelemetryResult.Success : TelemetryResult.Failure);
            telEvent.Properties.Add("id", extensionId);
            TelemetryService.DefaultSession.PostEvent(telEvent);
        }
    }
}
