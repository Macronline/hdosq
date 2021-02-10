using H2Q.BC.DataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class TestPage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DALCSQLServer bd = new DALCSQLServer();
        bd.TestConnection();
    }
    protected void Page_Load2(object sender, EventArgs e)
    {

        

        TcpListener server = null;

        try
        {
            // Set the TcpListener on port 13000.
            Int32 port = Int32.Parse( ConfigurationManager.AppSettings["Port"].ToString());
            IPAddress localAddr = IPAddress.Parse(ConfigurationManager.AppSettings["LocalAddr"].ToString());

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
                Response.Write("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                TcpClient client = server.AcceptTcpClient();
                Response.Write("Connected!");
                data = null;
                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();
                int i;
                StreamWriter objWriter2 = null;
                string NameFile = Server.MapPath("~") + Guid.NewGuid().ToString() + ".log";
                objWriter2 = new StreamWriter(NameFile, true);

                
                objWriter2.WriteLine("Cantidad Received bytes: {0}", bytes.Length);
                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    objWriter2.WriteLine("Received bytes: {0}", bytes);
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    objWriter2.WriteLine("Received: {0}", data);

                    ////////////// Process the data sent by the client.
                    ////////////data = data.ToUpper();
                    ////////////byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                    ////////////// Send back a response.
                    ////////////stream.Write(msg, 0, msg.Length);
                    ////////////Response.Write("Sent: {0}", data);
                }

                objWriter2.WriteLine("Close Received");
                objWriter2 = null;
                // Shutdown and end connection
                client.Close();
            }
        }
        catch (SocketException ex)
        {
            Response.Write(string.Format("SocketException: {0}", ex.Message + "." + ex.StackTrace));
        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }

        Response.Write("\nHit enter to continue...");
        Console.Read();

    }
}
//protected void Page_Load_OLD(object sender, EventArgs e)
//{
    //int portNum = 13;
    //bool done = false;
    //var listener = new TcpListener(IPAddress.Any, portNum);
    //listener.Start();

    //while (!done)
    //{
    //    Response.Write("Waiting for connection...");
    //    TcpClient client = listener.AcceptTcpClient();
    //    Response.Write("Connection accepted.");
    //    NetworkStream ns = client.GetStream();
    //    byte[] byteTime = Encoding.ASCII.GetBytes(DateTime.Now.ToString());
    //    try
    //    {
    //        ns.Write(byteTime, 0, byteTime.Length);
    //        ns.Close();
    //        client.Close();
    //    }
    //    catch (Exception e)
    //    {
    //        Response.Write(e.ToString());
    //    }
    //}

    //listener.Stop();
       
//    }
//}