using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace AddDeviceToAzure
{
    class Program
    {
        static RegistryManager registryManager;

		//This connection string has complete control over IoTHub - can create devices,
		//delete devices, read messages etc.,
        static string connectionString = "<IoTHubOwnerConnectionString>";

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddDeviceAsync().Wait();
            Console.ReadLine();
        }
        private static async Task AddDeviceAsync()
        {
            string PulseCheckerdeviceId = "PulseChecker";
            string DoctorConsoledeviceId = "DoctorConsole";
            Device PulseCheckerdevice;
            Device DoctorConsoledevice;
            try
            {
                PulseCheckerdevice = await registryManager.AddDeviceAsync(new Device(PulseCheckerdeviceId));
                DoctorConsoledevice = await registryManager.AddDeviceAsync(new Device(DoctorConsoledeviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                PulseCheckerdevice = await registryManager.GetDeviceAsync(PulseCheckerdeviceId);
                DoctorConsoledevice = await registryManager.GetDeviceAsync(DoctorConsoledeviceId);
            }
            Console.WriteLine("Generated pulse checker device key: {0}", PulseCheckerdevice.Authentication.SymmetricKey.PrimaryKey);
            Console.WriteLine("Generated doctor console device key: {0}", DoctorConsoledevice.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
