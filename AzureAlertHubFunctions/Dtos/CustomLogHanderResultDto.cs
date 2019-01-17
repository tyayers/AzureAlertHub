using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Dtos
{
    public class CustomLogHanderResultDto
    {
        public bool Handled = false;
        public List<AlertResult> Results = new List<AlertResult>(); 
    }
}
