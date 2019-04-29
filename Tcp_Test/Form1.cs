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

namespace Tcp_Test
{
    public partial class Form1 : Form
    {
        string _localIP = "";
        TCP.Client _client = null;
        TCP.Server _server = null;
        public Form1()
        {
            InitializeComponent();

            _localIP = GetLocalIP().ToString();
            _server = new TCP.Server(_localIP, 321);
            _server.OnTCPConnection += _server_OnTCPConnection;

        }

        void _server_OnTCPConnection(object sender, TCP.OnTCPConnectionArgs e)
        {
            //throw new NotImplementedException();
            MessageBox.Show(e.Msg);
        }

        void _client_OnTCPConnection(object sender, TCP.OnTCPConnectionArgs e)
        {
            //throw new NotImplementedException();
            MessageBox.Show(e.Msg);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            
            
            textBox1.Text = _localIP;
            await _server.StartAsync();
        }


        private IPAddress GetLocalIP()
        {
            List<IPAddress> ipList = new List<IPAddress>();
            IPAddress ip = null;
            string strHostName = Dns.GetHostName();
            // 取得本機的IpHostEntry類別實體，用這個會提示已過時
            //IPHostEntry iphostentry = Dns.GetHostByName(strHostName);

            // 取得本機的IpHostEntry類別實體，MSDN建議新的用法
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

            // 取得所有 IP 位址
            foreach (IPAddress ipaddress in iphostentry.AddressList) {
                // 只取得IP V4的Address
                if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    ipList.Add(ipaddress);
                }
            }
            if (ipList.Count > 1) {
                foreach (IPAddress ipaddress in ipList) {
                    if (!ipaddress.ToString().Contains("192.168")) {
                        ip = ipaddress;
                    }
                }
            }
            else {
                ip = ipList[0];
            }

            return ip;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            _client.SendData(textBox2.Text);
        }

        private void _client_OnTCPConnection1(object sender, TCP.OnTCPConnectionArgs e)
        {
            //throw new NotImplementedException();
            MessageBox.Show(e.Msg);
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                button1_Click(sender, e);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _client = new TCP.Client(textBox1.Text, 321);
            _client.Connect();
            _client.ReceiveDataAsync();
            _client.OnTCPConnection += _client_OnTCPConnection1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //_server.SendMsg(textBox3.Text, _server.ConntectedList[0]);
            _server.ConntectedList[0].SendData(textBox3.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try {
                if (_server.ConntectedList[0] != null && _server.ConntectedList.Count > 0) {
                    _server.ConntectedList[0].Close();
                }
            }
            catch (Exception ex) { }
            
            
        }
    }
}
