﻿using System;

namespace ExperimentalFeatures
{
    public class Constants
    {
        public const string LiveFeedPath = "https://raw.githubusercontent.com/ligershark/ExperimentalFeatures/master/extensions.json";
        public static readonly string LiveFeedCachePath = Environment.ExpandEnvironmentVariables("%localAppData%\\" + Vsix.Name + "\\feed.json");
        public static readonly string LogFile = Environment.ExpandEnvironmentVariables("%localAppData%\\" + Vsix.Name + "\\installer.log");

        public const double UpdateInterval = 3;
    }
}