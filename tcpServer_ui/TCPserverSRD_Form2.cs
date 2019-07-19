using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace tcpServer_ui
{
    public partial class TCPserverSRD_Form2 : Form
    {
        public TCPserverSRD_Form2()
        {
            InitializeComponent();
        }

        public NetworkStream stream;
        public StreamReader STR;
        public StreamWriter STW;
        public System.Net.Sockets.TcpListener server;
        public System.Net.Sockets.TcpClient client;
        public int i;
        public string str;
        public String data = null;
        public Byte[] readByte = new Byte[1024];
         
        private void startBtn_Click(object sender, EventArgs e)
        {
            startServer();
        }

        private void startServer()
        {
                try
                {
                    server = new TcpListener(IPAddress.Any, 8888);
                    server.Start();
                    stopBtn.Enabled = true;
                    startBtn.Enabled = false;
               
                    txtReceive.AppendText(">> Waiting For Connection...\n");
                    if (InvokeRequired)
                    {
                        this.Invoke(new Action(() => AcceptClients()));
                        return;
                    }
                    AcceptClients();
                    STR = new StreamReader(client.GetStream());
                    STW = new StreamWriter(client.GetStream());
                    STW.AutoFlush = true;

                    txtReceive.AppendText("\n**Connection Established**\n");
                    txtReceive.AppendText("==========\n");

                    receivingWorker.RunWorkerAsync();
                    sendingWorker.WorkerSupportsCancellation = true;
                    receivingWorker.WorkerSupportsCancellation = true;
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
        }

        private void AcceptClients()
        {
            txtReceive.AppendText("==========\n");
            client = server.AcceptTcpClient();
        }

        private void sendData()
        {
            sendingWorker.RunWorkerAsync();
        }
        
        private void sendingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (InvokeRequired)
                {
                    STW.WriteLine(data.ToUpper());
                    this.txtReceive.Invoke(new MethodInvoker(delegate()
                    {
                        if (data != "['][CLOSE][']\r\n")
                        txtReceive.AppendText("\nSENT: " + data.ToUpper() + "\n");
                    }));
                }
                else
                {
                    MessageBox.Show("Failed to send the message!");
                }
                sendingWorker.CancelAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        private void receivingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (client.Connected)
            {
                try
                {
                    data = STR.ReadLine();
                    this.txtReceive.Invoke(new MethodInvoker(delegate()
                    {
                        if (data != "['][CLOSE][']\r\n")
                        txtReceive.AppendText("\nRECEIVED: " + data + "\n");
                    }));

                    sendData();

                    if (data == "['][CLOSE][']\r\n")
                    {
                        break;
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            STR.Close();
            STW.Close();
            receivingWorker.CancelAsync();
            client.Close();
            server.Stop();

            if (InvokeRequired)
            {
                this.stopBtn.Invoke(new MethodInvoker(delegate()
                    {
                        stopBtn.Enabled = false;
                    }));
                this.startBtn.Invoke(new MethodInvoker(delegate()
                    {
                        startBtn.Enabled = true;
                    }));
            }
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            try
            {
                str = "['][CLOSE][']\r\n";
                STW.WriteLine(str);
                STR.Close();
                STW.Close();
                receivingWorker.CancelAsync();
                client.Close();
                server.Stop();
                stopBtn.Enabled = false;
                startBtn.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }   
    }
}
