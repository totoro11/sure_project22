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

        private Socket client_socket;   //클라이언트 소켓 선언
        private bool isConnected;   //참-거짓 값을 이용하여 통신 오류 발생 시 대처
        private byte[] bytes = new byte[1024];
        private const int PORT = 8087;
        private string clientMSG = "";


        public clacWiondow()
        {
            InitializeComponent();
            isConnected = false;    //통신 시작되면 true
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //connect 버튼 클릭 이벤트
            try
            {
                //소켓생성
                start();

                //스레드 생성
                Thread listen_thread = new Thread(do_receive);

                //스레드시작
                listen_thread.Start();
            }
            catch (Exception ex)
            {
                ex.ToString();
                MessageBox.Show(ex.ToString());
            }

            /*소켓이 연결이 잘 되면
             1. 버튼 disconnect로 바뀜 / ip,port 입력창 비활성화
             2. connSatate = CONNECTION
             3. 계산식 입력창 활성화
            이미 연결되어있는 상태일때 disconnect버튼을 누르면 소켓 연결해제
             */
            if (client_socket.Connected)
            {
                btnConnect.Text = "DISCONNECT";
                ipTextBox.ReadOnly = true;
                portTextBox.ReadOnly = true;
                connectState.Text = "CONNECTION";
                connectState.ForeColor = Color.Blue;
                inputCalcTextBox.ReadOnly = false;
            }
            else
            {
                btnConnect.Text = "CONNECT";
                ipTextBox.ReadOnly = false;
                portTextBox.ReadOnly = false;
                connectState.Text = "UNCONNECTION";
                connectState.ForeColor = Color.Red;
                inputCalcTextBox.ReadOnly = true;
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
            inputCalcTextBox.Text = "";
        }


        private void btn8_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "8";
        }

        private void iptextBox_TextChanged(object sender, EventArgs e)
        {


        }


        private void btnEqualSign_Click(object sender, EventArgs e)
        {
            // 트리 버튼 클릭시 이벤트 함수
            try
            {
                //소켓 연결안되있을때는 무시
                if (isConnected == false) return;

                //UTF8로 인코딩 상대에게 Client : 데이터 + "<eof>" 송신
                byte[] msg = Encoding.UTF8.GetBytes(" Client : " + inputCalcTextBox.Text + "<eof>");

                //계산기 = 버튼을 이용해서 내용 송신
                int bytesSent = client_socket.Send(msg);

                //connStateListBox에 Client : 내가 보낸 데이터 출력
                connStateListBox.Items.Add(" Client : " + inputCalcTextBox.Text);
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
                connStateListBox.SelectedIndex = connStateListBox.Items.Count - 1;
            }

            treeView1.Nodes.Clear();    // Data Initialized
            String data2 = clientMSG;        // Data Insert
            String[] str = data2.Split('#');    // Data Split

            Stack<TreeNode> stack = new Stack<TreeNode>();

            for (int i = 0; i < str.Length - 1; i++)
            {
                // If Operand
                if (!(str[i].Equals("+") || str[i].Equals("-") || str[i].Equals("*") || str[i].Equals("/")))
                {
                    TreeNode node = new TreeNode(str[i]);
                    stack.Push(node);
                }
                // If Operation
                else
                {
                    TreeNode left = stack.Pop();
                    TreeNode right = stack.Pop();
                    TreeNode node = new TreeNode(str[i]);
                    node.Nodes.Add(left);
                    node.Nodes.Add(right);
                    stack.Push(node);
                }
            }

            treeView1.Nodes.Add(stack.Pop());   // TreeView <- Top of stack
            treeView1.ExpandAll();
            clientMSG = "";
        }




        public void start()
        {
            //소켓 생성함수
            try
            {
                //소켓 연결되있으면 무시
                if (isConnected) return;

                //소켓 연결안되있으면 소켓 생성 시도
                client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client_socket.Connect(new IPEndPoint(IPAddress.Parse(ipTextBox.Text), PORT));

                //만약 소켓이 연결되면 소켓 연결됨을 보여줌
                //소켓 생성이 제대로 안되면 소켓 닫아주기
                if (client_socket.Connected)
                {
                    connStateListBox.Items.Add(String.Format("소켓 연결되었습니다 ", client_socket.RemoteEndPoint.ToString()));
                    isConnected = true;
                }
                else
                {
                    client_socket.Close();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                connStateListBox.SelectedIndex = connStateListBox.Items.Count - 1;
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
                        clientMSG += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                        //<eof>가 있으면 브레이크
                        if (clientMSG.IndexOf("<eof>") > -1) break;
                    }
                    clientMSG = clientMSG.Substring(0, clientMSG.Length - 5);
                    bool ST = clientMSG.Contains("CC##");
                    CheckForIllegalCrossThreadCalls = false;

                    // ? 없으면 크로스스레드 오류 발생 (https://blog.naver.com/spb02293/221671835503 참조)
                    Invoke((MethodInvoker)delegate
                    {
                        connStateListBox.Items.Add(clientMSG);
                        if (ST == true)
                        {
                            // treeListBox.Items.Add(" c# 명령어 값 전달");
                        }
                    }
                    );
                    // clientMSG = "";
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
                    connStateListBox.SelectedIndex = connStateListBox.Items.Count - 1;
                }
            }
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "1";
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "2";
        }

        private void btn3_Click_1(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "3";
        }

        private void btn0_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "0";
        }

        private void btn4_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "4";
        }

        private void btn5_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "5";
        }

        private void btn6_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "6";
        }

        private void btn7_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "7";
        }

        private void btn9_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "9";
        }

        private void btnPeriod_Click(object sender, EventArgs e)
        {
            if (inputCalcTextBox.Text.Contains("."))
                return;
            else
                inputCalcTextBox.Text += ".";

        }

        private void btnPlus_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "+";
        }

        private void btnMinus_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "-";
        }

        private void btnMulti_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "*";
        }

        private void btnDiv_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "/";
        }

        private void bracket1_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += "(";
        }

        private void bracket2_Click(object sender, EventArgs e)
        {
            inputCalcTextBox.Text += ")";
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
    }
}
