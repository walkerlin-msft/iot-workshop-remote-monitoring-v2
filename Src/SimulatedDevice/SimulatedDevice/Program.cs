using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Configuration;

namespace SimulatedDevice
{
    class Program
    {
        private static DeviceClient _deviceClient;
        private static bool _isStopped = false;
        private static string _deviceName;
        static void Main(string[] args)
        {

            // String containing Hostname, Device Id & Device Key in one of the following formats:
            //  "HostName=<iothub_host_name>;DeviceId=<device_id>;SharedAccessKey=<device_key>"
            string deviceConnectionString = ConfigurationManager.AppSettings["IoTDevice.ConnectionString"];
            Console.WriteLine("deviceConnectionString={0}\n", deviceConnectionString);

            try
            {
                _deviceName = getDeviceId(deviceConnectionString);

                Console.WriteLine("Simulated Device - {0}\n", _deviceName);

                /* Create the DeviceClient instance */
                _deviceClient = "<Put your code here>";

                /* Task for sending message */
                sendMessageToCloudAsync();

                /* Task for receiving message */
                receiveCloudToDeviceMessageAsync();

            }
            catch (FormatException ex)
            {
                Console.WriteLine("Please make sure you have pasted the correct connection string of IoT Hub!!\n\n FormatException={0}", ex.ToString());
            }

            /* Wait for any key to terminate the console App */
            Console.ReadLine();
        }

        private static async void sendMessageToCloudAsync()
        {
            int minTemperature = 0;
            int minHumidity = 20;

            Random rand = new Random();

            int i = 1;
            while (true)
            {
                if (_isStopped == false)
                {
                    int currentTemperature = minTemperature + (rand.Next() % 61);// 0~60
                    int currentHumidity = minHumidity + (rand.Next() % 61);// 20~80

                    var telemetryDataPoint = new
                    {
                        deviceId = _deviceName,
                        msgId = "message id " + i,
                        temperature = currentTemperature,
                        humidity = currentHumidity,
                        time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") // ISO8601 format, https://zh.wikipedia.org/wiki/ISO_8601
                    };

                    var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                    var message = new Message(Encoding.ASCII.GetBytes(messageString));
                    message.Properties.Add("SensorType", "thermometer");

                    // <Send Event Async Here>
                    await _deviceClient.SendEventAsync(xxxx);

                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                    i++;
                }
                else
                {
                    Console.WriteLine("{0} > Turn Off", DateTime.Now);
                }

                Task.Delay(5000).Wait();
            }
        }

        private static async void receiveCloudToDeviceMessageAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                // <Receive Async Here>


                string msg = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                try
                {
                    C2DCommand c2dCommand = JsonConvert.DeserializeObject<C2DCommand>(msg);
                    processCommand(c2dCommand);
                }
                catch (Exception e)
                {
                    Console.WriteLine("CANNOT PROCESS THIS COMMAND! {0}", e.ToString());
                }

                // <C2D Complete Async Here>
                await _deviceClient.CompleteAsync(xxxx);

            }
        }

        private static void processCommand(C2DCommand c2dCommand)
        {
            switch (c2dCommand.command)
            {
                case C2DCommand.COMMAND_TEMPERATURE_ALERT:
                    displayReceivedCommand(c2dCommand, ConsoleColor.Yellow);
                    break;
                case C2DCommand.COMMAND_TURN_ONOFF:
                    displayReceivedCommand(c2dCommand, ConsoleColor.Green);
                    _isStopped = c2dCommand.value.Equals("0"); // 0 means turn the machine off, otherwise is turning on.
                    break;
                default:
                    Console.WriteLine("IT IS NOT A SUPPORTED COMMAND!");
                    break;
            }
        }

        private static void displayReceivedCommand(C2DCommand c2dCommand, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("Received message: {0}, {1}, {2}\n", c2dCommand.command, c2dCommand.value, c2dCommand.time);
            Console.ResetColor();
        }

        private static string getDeviceId(string connectionString)
        {
            string[] fields = connectionString.Split(';');

            return fields[1].Substring(fields[1].IndexOf("=") + 1);
        }
    }
}
