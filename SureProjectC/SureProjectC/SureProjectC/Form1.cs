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

namespace SureProjectC
{
    public partial class Form1 : Form
    {
        Socket listen_socket;
        Socket client_socket;
        bool isConnected;
        byte[] bytes = new Byte[1024];
        static string data;

        static string input = "";
        static char[] stack = new char[100];   // 스택
        static int point = 0;                  // 스택 포인트
        static string send_mmsg = "";             // 출력 문자열 (postfix) (out put)

        public Form1()
        {
            InitializeComponent();
            isConnected = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            start("192.168.121.140", 8087, 10);   //(url, port, 백로그 보관요청 최대수)
        }

        public void start(string host, int port, int backlog)
        {
            this.listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress address;
            if (host == "0.0.0.0")
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
        private void SendCal()
        {
            try
            {
                if (isConnected == false) return;
                byte[] msg = Encoding.UTF8.GetBytes(send_mmsg + "<eof>");
                int bytesSent = client_socket.Send(msg);
                send_mmsg = "";
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
                    Invoke((MethodInvoker)delegate
                    {
                        listBox1.Items.Add(data);
                        if (data.Contains("C#"))
                        {
                            listBox1.Items.Add(" 후위표기 값 클라이언트에게 전달 ");
                            byte[] mmsg = Encoding.UTF8.GetBytes(" OP : CC## " + "<eof>");
                            int bytesSent = client_socket.Send(mmsg);
                        }
                        //send_mmsg = data;
                        Calculation();
                        SendCal();
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

        private static void Calculation()
        {
            Console.Out.Write("infix 수식을 입력하시오 : ");
            input = Console.In.ReadLine();      // 입력
            Console.Out.WriteLine("입력하신 infix 수식 : " + input + "\n");

            for (int i = 0; i < data.Length; i++)  // 한글자씩 반복하기
            {
                char c = data[i];                  // 한글자씩 가져오기

                if (c >= '0' && c <= '9')           // 피연산자의 경우
                {
                    send_mmsg += c.ToString();
                }
                else if (c == '+' || c == '-')
                {
                    while (!Stack_IsEmpty())
                    {
                        // 기존 스택이 비어있지 않다면
                        char prev_c = Stack_Pop();  // 스택 최상단의 연산자를 가져옴
                        if (prev_c == '*' || prev_c == '/' || prev_c == '+' || prev_c == '-')
                        {
                            // 스택 최상단 연산자가 현재 연산자 보다 상위 연산자라면
                            send_mmsg += prev_c.ToString();
                        }
                        else
                        {
                            Stack_Push(prev_c);
                            break;
                        }
                    }
                    Stack_Push(c);
                }
                else if (c == '*' || c == '/')
                {
                    while (!Stack_IsEmpty())
                    {
                        // 기존 스택이 비어있지 않다면
                        char prev_c = Stack_Pop();  // 스택 최상단의 연산자를 가져옴
                        if (prev_c == '*' || prev_c == '/')
                        {
                            // 스택 최상단 연산자가 현재 연산자 보다 상위 연산자라면
                            send_mmsg += prev_c.ToString();
                        }
                        else
                        {
                            Stack_Push(prev_c);
                            break;
                        }
                    }
                    Stack_Push(c);
                }
                else if (c == '(')
                {
                    Stack_Push(c);
                }
                else if (c == ')')
                {
                    send_mmsg += "#";
                    while (true)
                    {
                        char prev_oper = Stack_Pop();   // 기존 스택의 최상위 연산자를 꺼내온다. 
                        if (prev_oper == '(')
                            break;
                        send_mmsg += prev_oper.ToString(); // 기존 스택의 최상위 연산자를 출력으로 빼낸다. 
                    }
                }
            }

            // stack에 있는 모든 연산자를 순차적으로 꺼내서 output에 넣는다. 
            while (!Stack_IsEmpty())
            {
                send_mmsg += Stack_Pop().ToString();
            }

            // 결과 출력
           // Console.Out.WriteLine();
            Stack_Show();
            //Console.Out.WriteLine("Postfix : " + send_mmsg);

            //Console.Out.Write("아무키나 입력하시오.");
            //Console.In.ReadLine();  // 종료전 일시 정지
        }

        private static void Stack_Show()
        {
            // 스택 내용을 모두 보여준다. 
            for (int i = 0; i < point; i++)
            {
                Console.Out.Write(stack[i]);
            }
            Console.Out.WriteLine();
        }

        private static void Stack_Push(char c)
        {
            // 스택의 최상위에 값을 추가한다. 
            stack[point] = c;
            point++;
            return;
        }

        private static char Stack_Pop()
        {
            // 스택의 최상위에 값을 가져온다. 
            point--;
            char result = stack[point];
            stack[point] = '\0';
            return result;
        }

        private static bool Stack_IsEmpty()
        {
            // 스택이 비었다면 true를 비어있지 않다면 false를 반환한다. 
            send_mmsg += "#";
            return (point == 0 ? true : false);
        }
    }
}
