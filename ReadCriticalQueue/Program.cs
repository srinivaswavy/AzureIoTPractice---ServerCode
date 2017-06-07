using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ServiceBus.Messaging;

namespace ReadCriticalQueue
{
	class Program
	{
		static void Main(string[] args)
		{

			Console.WriteLine("Receive critical messages. Ctrl-C to exit.\n");
			var connectionString = "<ServiceBusConnectionString>";
			var queueName = "<AzureCloudQueueName>";

			var client = QueueClient.CreateFromConnectionString(connectionString, queueName);

			client.OnMessage(message =>
			{
				try
				{
					Stream stream = message.GetBody<Stream>();
					StreamReader reader = new StreamReader(stream, Encoding.ASCII);
					string s = reader.ReadToEnd();
					s = String.Format("Pulse: {0}..<br/><br/>", s);
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine(s);
					Console.WriteLine("Connecting emergency services... Sending Message to Doctor..\n");
					Console.ForegroundColor = ConsoleColor.Red;
					string strmessage = String.Concat(s, "<font color='Red'> Patient pulse critically went down.</font><br/> <br/> All systems activated. <br/> <br/>Message sent to hospital emergency service and family members.<br/> <br/>Please be available for all communications.<br/>----------EOM----------<br/> <br/>");
					MessageToDoctor.SendMessage(strmessage);
					Console.ForegroundColor = ConsoleColor.White;

				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			});

			Console.ReadLine();
		}
	}
}
