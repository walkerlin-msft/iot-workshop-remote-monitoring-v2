using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmServiceBusConsoleApp
{
    class Program
    {
        /* Service Bus */
        private const string QueueName = "temperatureAlert";// It's hard-coded for this workshop

        /* IoT Hub */
        private static ServiceClient _serviceClient;

        /* Web API */
        private static WebServerConnector _webSC;

        static void Main(string[] args)
        {
            Console.WriteLine("Console App for Alert Service Bus...");

            /* Load the settings from App.config */
            string serviceBusConnectionString = ConfigurationManager.AppSettings["ServiceBus.ConnectionString"];
            Console.WriteLine("serviceBusConnectionString={0}\n", serviceBusConnectionString);

            string iotHubConnectionString = ConfigurationManager.AppSettings["IoTHub.ConnectionString"];
            Console.WriteLine("iotHubConnectionString={0}\n", iotHubConnectionString);

            // Retrieve Web Server URL
            string webServerUrl;
            string isProduction = ConfigurationManager.AppSettings["WebServer.isProduction"];
            if (isProduction.Equals("1"))
                webServerUrl = ConfigurationManager.AppSettings["WebServer.Production"];
            else
                webServerUrl = ConfigurationManager.AppSettings["WebServer.Localhost"];

            _webSC = new WebServerConnector(webServerUrl);

            // Retrieve a Queue Client
            QueueClient queueClient = QueueClient.CreateFromConnectionString(serviceBusConnectionString, QueueName);

            // Retrieve a Service Client of IoT Hub
            _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);

            queueClient.OnMessage(message =>
            {
                Console.WriteLine("\n*******************************************************");
                string msg = message.GetBody<String>();
                try
                {
                    AlarmMessage alarmMessage = JsonConvert.DeserializeObject<AlarmMessage>(msg);

                    ProcessAlarmMessage(alarmMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("****  Exception=" + ex.Message);
                }
            });

            Console.ReadLine();
        }

        private static void ProcessAlarmMessage(AlarmMessage alarmMessage)
        {
            switch (alarmMessage.alarmType)
            {
                case "TempAlert":
                    ActionTemperatureAlert(alarmMessage);
                    break;
                case "EnableDevice":
                    ActionEnableDevice(alarmMessage.ioTHubDeviceID, alarmMessage.reading, alarmMessage.createdAt);
                    break;
                default:
                    Console.WriteLine("AlarmType is Not accpeted!");
                    break;
            }
        }

        private static void ActionTemperatureAlert(AlarmMessage alarmMessage)
        {
            WriteHighlightedMessage(
                    GetDeviceIdHint(alarmMessage.ioTHubDeviceID) +
                    "[" + alarmMessage.createdAt + "]" +
                    " TempAlert! Temp=" + alarmMessage.reading +
                    ", Threshold=" + alarmMessage.threshold,
                    ConsoleColor.Yellow);

            /* Action 1: Send Cloud-to-Device command */
            C2DCommand c2dCommand = new C2DCommand();
            c2dCommand.command = C2DCommand.COMMAND_TEMPERATURE_ALERT;
            c2dCommand.value = alarmMessage.reading;
            c2dCommand.time = alarmMessage.createdAt;

            SendCloudToDeviceCommand(
                _serviceClient,
                alarmMessage.ioTHubDeviceID,
                c2dCommand).Wait();

            /* Action 2: Send to Web dashboard */
            //string webSCResult = _webSC.PostTelemetryAlarm(alarmMessage);
            //Console.WriteLine(webSCResult);

        }

        private static void ActionEnableDevice(string ioTHubDeviceID, string on, string time)
        {
            WriteHighlightedMessage(
                    GetDeviceIdHint(ioTHubDeviceID) +
                    " WindTurbine Enable=" + on +
                    ", Time=" + time,
                    ConsoleColor.Green);

            C2DCommand c2dCommand = new C2DCommand();
            c2dCommand.command = C2DCommand.COMMAND_TURN_ONOFF;
            c2dCommand.value = on;
            c2dCommand.time = time;

            SendCloudToDeviceCommand(
                _serviceClient,
                ioTHubDeviceID,
                c2dCommand).Wait();
        }

        private async static Task SendCloudToDeviceCommand(ServiceClient serviceClient, String deviceId, C2DCommand command)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(command)));
            await serviceClient.SendAsync(deviceId, commandMessage);
        }

        private static void WriteHighlightedMessage(string message, System.ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static string GetDeviceIdHint(string ioTHubDeviceID)
        {
            return "[" + ioTHubDeviceID + " (" + DateTime.UtcNow.ToString("MM-ddTHH:mm:ss") + ")" + "]";
        }
    }
}
