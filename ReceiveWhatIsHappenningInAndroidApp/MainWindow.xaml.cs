using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;

namespace ReceiveWhatIsHappenningInAndroidApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Thread thread1 = new Thread(() =>
            {
                Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint iPEnd = new IPEndPoint(IPAddress.Parse("192.168.1.103"), 8888);
                listenerSocket.Bind(iPEnd);
                int clientID = 1;
                while (true)
                {
                    listenerSocket.Listen(0);
                    Socket clientSocket = listenerSocket.Accept();
                    Thread clientThread = new Thread(() => ClientConnection(clientSocket, clientID));
                    clientThread.Start();
                    clientID++;
                }
            });
            Thread thread2 = new Thread(() =>
            {
                while (true)
                {
                    InitializeComponent();
                }
            });
            thread1.Start();
            thread2.Start();
        }
        public delegate void UpdateTextCallback(string message);
        private void UpdateText(string message)
        {
            richTextBox.AppendText(message + "\n");
        }
        private void ClientConnection(Socket clientSocket, int clientID)
        {
            byte[] Buffer = new byte[clientSocket.SendBufferSize];
            int readByte = 1;
            do
            {
                try
                {
                    readByte = clientSocket.Receive(Buffer);
                }
                catch (Exception e)
                {

                    richTextBox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), new object[] { "Connection aborted. " + e.ToString() });
                }
                richTextBox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), new object[] { "Number of characters in message: " + readByte.ToString() });
               
                byte[] readData = new byte[readByte];
                Array.Copy(Buffer, readData, readByte);

                richTextBox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), new object[] { "From client numer " + clientID + "\n We got: " + System.Text.Encoding.UTF8.GetString(readData) });
                //try
                //{
                //    clientSocket.Send(new byte[2] { 79, 75 });// O K
                //}
                //catch (Exception e)
                //{
                //    richTextBox.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), new object[] { "Connection aborted. " + e.ToString() });

                //}
            } while (readByte > 0);
        }

        async void LoadData()
        {
            string page = "http://www.google.com.au";
            using (HttpClient httpClient = new HttpClient())
            using (HttpResponseMessage responseMessage = await httpClient.GetAsync(page))
            using (HttpContent httpContent = responseMessage.Content)
            {
                string data = await httpContent.ReadAsStringAsync();
                if (data != null)
                {
                    richTextBox.AppendText(data);
                }
            }
        }

        public class Screenshot
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
