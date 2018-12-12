# Azure Alert Hub
A project to collect, filter and forward alerts in Azure to a Service Management system such as ServiceNow (for incident management), mute further alerts until a confirmation is sent from the Service Management system to prevent event flooding, and provide a return-channel to get an incident update to close the alert.

## Flow 1: Alert Action from Azure Monitor triggers the Azure Function AzureAlertFunction
![Alert Flow 1](img/AlertFlow1.png "text")

## Flow 2: Service Management system calls Azure Function AzureAlertConfirmFunction to confirm the incident has been closed.
![Alert Flow 2](img/AlertFlow1.png "text")

# Installation / Configuration
Deploy the Azure Functions project here to Azure, and configure the environment variable StorageConnectionString to point to an Azure Storage account to store the event data, along with the other environment variables to configure the communication to the Service Management system.

After the functions are configure, configure the Function AzureAlertFunction to be the trigger for any Azure Monitor alerts.  When the alerts are triggered, you will see them in the Storage Account under Tables > Alerts and AlertIncidents.

# Usage

You will also see that multiple events only increases the Counter property, and does not create new entries.  This goes the same for calling the Service Management system.

After events have been stored, you can confirm them by calling the **AzureAlertConfirmFunction** with the incident id, which should then remove all entires of the event from the Alerts and AlertIncidents tables, which will allow a new event to start the process all over again.

