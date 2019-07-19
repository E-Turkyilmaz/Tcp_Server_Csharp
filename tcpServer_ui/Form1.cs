using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace tcpServer_ui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public NetworkStream stream;
        public StreamReader STR;
        public StreamWriter STW;
        public System.Net.Sockets.TcpListener server;
        public System.Net.Sockets.TcpClient client;
        public int portNumber = 8888;
        public int i;
        public string str;
        public string endStr;
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
                    server = new TcpListener(IPAddress.Any, portNumber);
                    server.Start();
                    stopBtn.Enabled = true;
                    startBtn.Enabled = false;
                    txtReceive.AppendText(">> Waiting For Connection...\n");
                    txtReceive.AppendText(">> Port Number: " + portNumber.ToString() + "\n");
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
            txtReceive.AppendText("==========\n");

        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            if (txtSend.Text != "")
            {
                str = txtSend.Text;
                sendingWorker.RunWorkerAsync();
            }
            
            txtSend.Text = "";
        }
        
        private void sendingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (InvokeRequired)
            {
                STW.WriteLine(str);
                this.txtSend.Invoke(new MethodInvoker(delegate()
                {
                    txtReceive.AppendText("\nSENT: " + str + "\n");
                }));
            }
            else
            {
                MessageBox.Show("Failed to send the message!");
            }
            sendingWorker.CancelAsync();
        }
        
        private void receivingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (client.Connected)
            {
                try
                {
                    stream = client.GetStream();
                    i = stream.Read(readByte, 0, readByte.Length);
                    data = Encoding.UTF8.GetString(readByte, 0, i);
                    this.txtReceive.Invoke(new MethodInvoker(delegate()
                    {
                        if (data != "['][CLOSE][']\r\n")
                        {
                            txtReceive.AppendText("\nRECEIVED: " + data + "\n");
                        }
                    }));

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
            endStr = "['][CLOSE][']\r\n";
            STW.Write(endStr);
            STR.Close();
            STW.Close();
            receivingWorker.CancelAsync();
            client.Close();
            server.Stop();
            stopBtn.Enabled = false;
            startBtn.Enabled = true;
        }   
    }
}