using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure.NotificationHubs;

namespace ReadCriticalQueue
{
	class Program
	{
		static void Main(string[] args)
		{

			//Console.WriteLine("Receive critical messages. Ctrl-C to exit.\n");
			HandlePatientMessages().Wait();
			HandleHospitalMessages();


			Console.ReadLine();
		}
		private static void HandleHospitalMessages() {
			var connectionString = "Endpoint=sb://patientmr.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=TycKNyOj7Zn0l6hQ4QWLY3qaxhZOm1ErGb925FERKDI=";
			var queueName = "bpcriticalqueue";

			var client = QueueClient.CreateFromConnectionString(connectionString, queueName);

			client.OnMessage(message =>
			{
				try
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Stream stream = message.GetBody<Stream>();
					StreamReader reader = new StreamReader(stream, Encoding.ASCII);
					string s = reader.ReadToEnd();
					s = String.Format("Pulse: {0}..<br/><br/>", s);
					Console.WriteLine(s);
					Console.WriteLine("Connecting emergency services... Sending Message to Doctor..\n");					
					string strmessage = String.Concat(s, "<font color='Red'> Patient pulse critically went down.</font><br/> <br/> All systems activated. <br/> <br/>Message sent to hospital emergency service and family members.<br/> <br/>Please be available for all communications.<br/>----------EOM----------<br/> <br/>");
					MessageToDevice.SendMessageToHospital(strmessage);					

				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			});

			Console.WriteLine("Receive critical messages. Ctrl-C to exit.\n");

		}
		private static void HandlePatientMessages()
		{
			var connectionString = "Endpoint=sb://patientmr.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=TycKNyOj7Zn0l6hQ4QWLY3qaxhZOm1ErGb925FERKDI=";
			var queueName = "patientmessagesqueue";

			var client = QueueClient.CreateFromConnectionString(connectionString, queueName);

			client.OnMessage(message =>
			{
				try
				{
					sendMessage(message).Wait();				
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			});
		}

		public static async Task sendMessage(BrokeredMessage message)
		{
			try
			{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Stream stream = message.GetBody<Stream>();
				StreamReader reader = new StreamReader(stream, Encoding.ASCII);
				string s = reader.ReadToEnd();
				s = String.Format("Message from hospital<br/><br/>{0}", s);

				Console.WriteLine(s);
				Console.WriteLine("Connecting emergency services... Sending Message to Patient..\n{0}", s);
				MessageToDevice.SendMessageToPatient(s);
				bool isSuccess = await sendToNotificationHub("gcm", s, string.Empty);
				if (!isSuccess)
					Console.WriteLine("Sending notification failed");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
		private static async Task<bool> sendToNotificationHub(string pns, string message, string to_tag)
		{

			Microsoft.Azure.NotificationHubs.NotificationOutcome outcome = null;
			var notif = "{ \"data\" : {\"message\":\"" + message + "\"}}";
			outcome = await Notifications.Instance.Hub.SendGcmNativeNotificationAsync(notif);

			if (outcome != null)
			{
				if (!((outcome.State == Microsoft.Azure.NotificationHubs.NotificationOutcomeState.Abandoned) ||
					(outcome.State == Microsoft.Azure.NotificationHubs.NotificationOutcomeState.Unknown)))
				{
					return true;
				}
			}
			return false;
			//var notif = "{ \"data\" : {\"message\":\"" + message + "\"}}";
			//await Notifications.Instance.Hub.SendGcmNativeNotificationAsync(notif, to_tag);
		}

	}

	public class Notifications
	{
		public static Notifications Instance = new Notifications();
		public NotificationHubClient Hub { get; set; }

		private string signature = "Endpoint=sb://aknotificationnamespace.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=PXO4Tjqw+fjSY4ZFLpy61649Q9U+EIoMfbgu38owlRc=";

		private string hubName = "OptumHackathonNotificationHub";
		
		private Notifications()
		{
			Hub = NotificationHubClient.CreateClientFromConnectionString(signature, hubName);
		}
	}
}
