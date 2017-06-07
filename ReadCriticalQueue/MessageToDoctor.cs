using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace ReadCriticalQueue
{
    class MessageToDoctor
    {
        static ServiceClient serviceClient;
        static string connectionString = "HostName=PatientMR.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=HD0aQJTKXryo/MuLUMfLsE4LrPjtWlIEtdBGNCD5Sg4=";
        static public void SendMessage(string message)
        {
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            SendCloudToDeviceMessageAsync(message).Wait();            
        }
        private async static Task SendCloudToDeviceMessageAsync(string message)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(message));
            commandMessage.Ack = DeliveryAcknowledgement.Full;
            await serviceClient.SendAsync("DoctorConsole", commandMessage);
        }

    }

}
