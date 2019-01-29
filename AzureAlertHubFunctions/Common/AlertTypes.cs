using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Common
{
    public static class AlertTypes
    {
        public static string Other { get; } = "OTHER";
        public static string DB { get; } = "DB";
        public static string Disk { get; } = "DISK";
    }
}
