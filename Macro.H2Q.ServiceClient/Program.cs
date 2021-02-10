    using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Macro.H2Q.ServiceClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                byte[] messageReturn = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x0C, 0x01, 0x05, 0x00, 0x00, 0x00, 0x07, 0x67, 0x65, 0x74, 0x69, 0x6E, 0x66, 0x6F, 0x01, 0x00, 0x00, 0x43, 0x12 };

                //////////string input = "1";
                //////////char[] values = input.ToCharArray();
                //////////foreach (char letter in values)
                //////////{
                //////////    // Get the integral value of the character.
                //////////    int value = Convert.ToInt32(letter);
                //////////    // Convert the integer value to a hexadecimal value in string form.
                //////////    Console.WriteLine($"Hexadecimal value of {letter} is {value:X}");
                //////////}

                Console.WriteLine("Ingrese mensaje:");
                string messa = Console.ReadLine();
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                string server = ConfigurationManager.AppSettings["IPServer"].ToString();
                Int32 port = Int32.Parse(ConfigurationManager.AppSettings["Port"].ToString());
                TcpClient client = new TcpClient(server, port);

                string message = "HOLA AMIGOS";
                message = messa;

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                //////byte resp = 0x01;
                //////messa = resp.ToString();
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);
                //stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[16];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}
