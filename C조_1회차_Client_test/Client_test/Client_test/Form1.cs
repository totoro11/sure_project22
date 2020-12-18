using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Net;

namespace Client_test
{
    public partial class clacWiondow : Form
    {

        Socket client_socket;   //클라이언트 소켓 선언
        bool isConnected;   //참-거짓 값을 이용하여 통신 오류 발생 시 대처
        byte[] bytes = new byte[1024];
        string data;


        public clacWiondow()
        {
            InitializeComponent();
            isConnected = false;    //통신 시작되면 true
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button21_Click(object sender, EventArgs e)
        {
            try
            {
                start();
                Thread listen_thread = new Thread(do_receive);
                listen_thread.Start();  //클라이언트 스레드 시작
            }
            catch (Exception ex)
            {
                ex.ToString();
                MessageBox.Show(ex.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button17_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void Claculator_Load(object sender, EventArgs e)
        {

        }

        private void btnResult_Click(object sender, EventArgs e)
        {

        }

        private void btnEqualSign_Click(object sender, EventArgs e)
        {
            try
            {
                if (isConnected == false) return;
                byte[] msg = Encoding.UTF8.GetBytes(" Client : " + inputCalcTextBox.Text + "<eof>");    //UTF8로 인코딩 상대에게 Client : 데이터 + "<eof>" 송신
                int bytesSent = client_socket.Send(msg);    //계산기 = 버튼을 이용해서 내용 송신
                treeListBox.Items.Add(" Client : " + inputCalcTextBox.Text);   //treeListBox에 Client : 내가 보낸 데이터 출력
                inputCalcTextBox.Clear();
                inputCalcTextBox.Text = "";
            }
            catch (Exception ex)
            {
                ex.ToString();
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                treeListBox.SelectedIndex = treeListBox.Items.Count - 1;
            }
        }


        public void start()
        {
            try
            {
                if (isConnected == true) return;
                //소켓 생성
                client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);    
                client_socket.Connect(new IPEndPoint(IPAddress.Parse(ipTextBox.Text), 8088));
                treeListBox.Items.Add(String.Format("소켓 연결되었습니다 ", client_socket.RemoteEndPoint.ToString()));
                isConnected = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                treeListBox.SelectedIndex = treeListBox.Items.Count - 1;
            }
        }

        void do_receive()
        {
            while (isConnected) //무한루프 설정
            {
                try
                {
                    while (true)    //통신이 시작되었을때
                    {
                        byte[] bytes = new byte[1024];
                        int bytesRec = client_socket.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<eof>") > -1) break;  //<eof>가 있으면 브레이크
                    }
                    data = data.Substring(0, data.Length - 5);
                    bool ST = data.Contains("CC##");
                    CheckForIllegalCrossThreadCalls = false;
                    Invoke((MethodInvoker)delegate  // ? 없으면 크로스스레드 오류 발생 (https://blog.naver.com/spb02293/221671835503 참조)
                    {
                        treeListBox.Items.Add(data);
                        if (ST == true)
                        {
                           // treeListBox.Items.Add(" c# 명령어 값 전달");
                        }
                    }
                    );
                    data = "";
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    MessageBox.Show(ex.ToString());

                    isConnected = false;
                    while (isConnected == false)
                    {
                        start();
                        Thread listen_thread = new Thread(do_receive);
                        listen_thread.Start();
                    }
                }
                finally
                {
                    treeListBox.SelectedIndex = treeListBox.Items.Count - 1;
                }
            }
        }

    }
}
