using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Macro.H2Q.ServiceServer
{
    class Program
    {
        static void Main(string[] args)
        {

            Int32 port = Int32.Parse(ConfigurationManager.AppSettings["Port"].ToString());
            IPAddress localAddr = IPAddress.Parse(ConfigurationManager.AppSettings["LocalAddr"].ToString());
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                
                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    
                    Console.WriteLine("Client Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    string Acum = "";
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        for (int p = 0; p < bytes.Length; p++)
                        {
                            //Console.Write("{0}-", bytes[p]);
                            Acum += bytes[p].ToString() + "-";
                        }
                        Console.WriteLine("Received Bytes: {0}", Acum);
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received Ascii: {0}", data);

                        string StrFecha = string.Format("{0}.{1}.{2} {3}.{4}.{5}.{6}", DateTime.Now.Year.ToString() , 
                            DateTime.Now.Month.ToString(), DateTime.Now.Day.ToString() , DateTime.Now.Hour.ToString(),
                            DateTime.Now.Minute.ToString(), DateTime.Now.Second.ToString(), DateTime.Now.Millisecond.ToString()) ;

                        string NameFile = Assembly.GetExecutingAssembly().Location.Replace(Assembly.GetExecutingAssembly().ManifestModule.Name, "")
                + string.Format("Input_{0}.log", StrFecha);
                        //StreamWriter wr = new StreamWriter(NameFile);
                        //wr.WriteLine("Received Bytes:" + bytes.Length.ToString());
                        //wr.WriteLine("Received Acum:" + Acum.Length.ToString());
                        //wr.WriteLine(Acum);
                        //wr.WriteLine("****************************************************************");
                        //wr.WriteLine("Received Ascii:" + data.Length);
                        //wr.WriteLine(data);

                        File.AppendAllText(NameFile, "Received Bytes Lenght:" + bytes.Length.ToString());
                        File.AppendAllText(NameFile, "Received Acum Lenght:" + Acum.Length.ToString());
                        File.AppendAllText(NameFile, Acum);
                        File.AppendAllText(NameFile, "****************************************************************");
                        File.AppendAllText(NameFile, "Received Ascii Lenght:" + data.Length);
                        File.AppendAllText(NameFile, data);
                        
                        // Process the data sent by the client.
                        //byte resp = 0x01;
                        //data = resp.ToString();

                        //byte[] messageReturn = new byte[] { 01 };
                        //byte[] messageReturn = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x0C, 0x01, 0x05, 0x00, 0x00, 0x00, 0x07, 0x67, 0x65, 0x74, 0x69, 0x6E, 0x66, 0x6F, 0x01, 0x00, 0x00, 0x43, 0x12 };
                        byte[] messageReturn = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x26, 0x0C, 0x01, 0x05, 0x00, 0x00, 0x00, 0x1e, 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x2c, 0x20, 0x6c, 0x65, 0x74, 0x73, 0x20, 0x74, 0x65, 0x73, 0x74, 0x20, 0x54, 0x43, 0x50, 0x20, 0x6c, 0x69, 0x6e, 0x6b, 0x20, 0x6d, 0x6f, 0x64, 0x65, 0x01, 0x00, 0x00, 0x1a, 0xef };
                        //00000000000000260C01050000001e48656c6c6f2c206c657473207465737420544350206c696e6b206d6f64650100001aef
                        //000000000000000F0C010500000007676574696E666F0100004312

                        Console.WriteLine("messageReturn.Length: " + messageReturn.Length.ToString());
                        stream.Write(messageReturn, 0, messageReturn.Length);
                        //Console.WriteLine("Sent A: {0}", messageReturn);

                        //data = data.ToUpper();
                        //data = "OK";
                        ////////////byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        ////////////// Send back a response.
                        ////////////stream.Write(msg, 0, msg.Length);
                        ////////////Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    
    }
}
