using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace ServerSocketTest
{
    public partial class Form1 : Form
    {
        Socket listen_socket;
        Socket client_socket;
        bool isConnected;
        byte[] bytes = new Byte[1024];
        string data;

        public Form1()
        {
            InitializeComponent();
            isConnected = false;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            start("192.168.121.139", 8088, 10);   //(url, port, 백로그 보관요청 최대수)
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (isConnected == false) return;
                byte[] msg = Encoding.UTF8.GetBytes(" 서버 : " + textBox1.Text + "<eof>"); // 클라이언트 list에 실제 찍히는 msg
                int bytesSent = client_socket.Send(msg);
                listBox1.Items.Add(" 서버 : " + textBox1.Text);
                textBox1.Clear();
                textBox1.Text = "";
            }
            catch (Exception ex)
            {
                ex.ToString();
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				button2_Click(sender, e);
			}
		}

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			listBox1.SelectedIndex = listBox1.Items.Count - 1;
		}

        public void start(string host, int port, int backlog)
        {
            this.listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress address;
            if(host == "0.0.0.0")
            {
                address = IPAddress.Any;
            }
            else
            {
                address = IPAddress.Parse(host);    //위의 host값 가져옴
            }
            IPEndPoint endpoint = new IPEndPoint(address, port);
            try
            {
                listen_socket.Bind(endpoint);
                listen_socket.Listen(backlog);

                client_socket = listen_socket.Accept();
                listBox1.Items.Add("연결 시작");
                isConnected = true;
                Thread listen_thread = new Thread(do_receive);
                listen_thread.Start();
            }
            catch (Exception ex)
            {
                ex.ToString();
                MessageBox.Show(ex.ToString());
            }

        }

        void do_receive()
        {
            while (isConnected)
            {
                try
                {
                    while (true)
                    {
                        byte[] bytes = new byte[1024];
                        int bytesRec = client_socket.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<eof>") > -1) break;
                    }
                    data = data.Substring(0, data.Length - 5);
                    CheckForIllegalCrossThreadCalls = false;
                    Invoke((MethodInvoker)delegate // ? 없으면 크로스스레드 오류 발생 (https://blog.naver.com/spb02293/221671835503 참조)
                    {
                            listBox1.Items.Add(data);
                            if (data.Contains("C#"))
                            {
                                listBox1.Items.Add(" 명령어 값 클라이언트에게 전달 ");
                                byte[] mmsg = Encoding.UTF8.GetBytes(" 클라 : CC## " + "<eof>");
                                int bytesSent = client_socket.Send(mmsg);
                            }
                        }
                    );
                    data = "";
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    MessageBox.Show(ex.ToString());
                }
            }
        }

    }
}
