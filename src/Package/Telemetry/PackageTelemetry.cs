using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Telemetry;

namespace ExperimentalFeatures.Telemetry
{
    public class PackageTelemetry
    {
        public static PackageTelemetry TelemetrySession { get; private set; }

        static PackageTelemetry()
        {
            TelemetrySession = new PackageTelemetry();
        }

        public PackageTelemetry()
        {

        }

        public void PackageLoaded()
        {
            var telEvent = new TelemetryEvent(TelemetryConstants.PackageLoadedEvent);
            this.PostEventToDefaultSession(telEvent);
        }

        private void PostEventToDefaultSession(TelemetryEvent vsTelemetryEvent)
        {
            TelemetryService.DefaultSession.PostEvent(vsTelemetryEvent);
        }

        private static class TelemetryConstants
        {
            public const string ExperimentsEventPrefix = "WebTools/Experiments/";

            public const string PackageLoadedEvent = ExperimentsEventPrefix + "PackageLoaded";
        }
    }
}
