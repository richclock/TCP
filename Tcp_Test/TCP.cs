using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace TCP
{
    public class OnTCPConnectionArgs : EventArgs
    {
        public string Msg { get; set; }
        public TcpClient TcpClient { get; set; }
    }
    public class OnUDPConnectionArgs : EventArgs
    {
        public string Msg { get; set; }
    }
    public class Server
    {
        public event EventHandler<OnTCPConnectionArgs> OnTCPConnection;
        public void DoUpdateConnection(OnTCPConnectionArgs OnTCPConnectionArgs)
        {
            EventHandler<OnTCPConnectionArgs> _connection
                = new EventHandler<OnTCPConnectionArgs>(OnTCPConnection);

            if (_connection != null) {
                OnTCPConnectionArgs e = new OnTCPConnectionArgs {
                    Msg = OnTCPConnectionArgs.Msg,
                    TcpClient = OnTCPConnectionArgs.TcpClient
                };

                if (_connection != null)
                    _connection(this, e);//fire event

            }
        }
        public List<Client> ConntectedList { get; set; }
        public TcpClient _tmpTcpClient=null;
        TcpListener _tcpListener = null;
        public string IP { get; set; }
        public int Port { get; set; }
        public Server(string ip, int port)
        {
            this.ConntectedList = new List<Client>();
            this.IP = ip;
            this.Port = port;
        }
        
        public async Task StartAsync()
        {

            //取得本機IP
            IPAddress ip = IPAddress.Parse(this.IP);
            //IPAddress ip = GetLocalIP();

            //建立本機端的IPEndPoint物件
            IPEndPoint ipe = new IPEndPoint(ip, this.Port);

            //建立TcpListener物件
            _tcpListener = new TcpListener(ipe);
            
            
            //開始監聽port
            _tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            _tcpListener.Start(5000);

            await Task.Run(() => {
                TcpClient tmpTcpClient;
                int numberOfClients = 0;
                while (true) {
                    try {
                        //建立與客戶端的連線
                        _tmpTcpClient = _tcpListener.AcceptTcpClient();

                        if (_tmpTcpClient.Connected) {
                            Client client = new Client(_tmpTcpClient);
                            client.OnTCPConnection += client_OnTCPConnection;
                            client.ReceiveDataAsync();
                            ConntectedList.Add(client);
                            //object obj = (object)_tmpTcpClient;
                            //Thread myThread = new Thread(new ParameterizedThreadStart(Communicate));
                            //numberOfClients += 1;
                            //myThread.IsBackground = true;
                            //myThread.Start(obj);
                            //myThread.Name = _tmpTcpClient.Client.RemoteEndPoint.ToString();
                        }
                    }
                    catch (Exception ex) {
                        string aa = ex.ToString();
                    }
                }
            });
            
        }

        void client_OnTCPConnection(object sender, OnTCPConnectionArgs e)
        {
            //throw new NotImplementedException();
            MessageBox.Show(e.Msg);
        } 
        public void Stop()
        {


            try {
                foreach (var connected in ConntectedList) {
                    connected.OnTCPConnection -= client_OnTCPConnection;
                    connected.Close();
                }
                _tcpListener.Stop();
                _tcpListener = null;
                //_tmpTcpClient.Close();
                //_tmpTcpClient = null;
                //await Task.Run(() => {

                //});
            }
            catch (Exception ex) {
                string aa = ex.ToString();
            }
            //GC.Collect();
            
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

        
    }
    public class Client
    {
        public event EventHandler<OnTCPConnectionArgs> OnTCPConnection;
        public void DoUpdateConnection(OnTCPConnectionArgs OnTCPConnectionArgs)
        {
            EventHandler<OnTCPConnectionArgs> _connection
                = new EventHandler<OnTCPConnectionArgs>(OnTCPConnection);

            if (_connection != null) {
                OnTCPConnectionArgs e = new OnTCPConnectionArgs {
                    Msg = OnTCPConnectionArgs.Msg
                };

                if (_connection != null)
                    _connection(this, e);//fire event

            }
        }
        public TcpClient _tcpClient = null;
        public string HostIP { get; set; }
        public int Port { get; set; }
        public bool Connected { get; private set; }
        public Client(string ip, int port)
        {
            _tcpClient = new TcpClient();
            this.HostIP = ip;
            this.Port = port;
            Connect();
        }
        public Client(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            IPEndPoint ipEndPoint=(IPEndPoint)_tcpClient.Client.RemoteEndPoint;
            this.HostIP = ipEndPoint.Address.ToString();
            this.Port = ipEndPoint.Port;
            this.Connected = true;
        }
        public void Connect()
        {
            try {
                string hostIP = this.HostIP;
                IPAddress ipa = IPAddress.Parse(hostIP);
                IPEndPoint ipe = new IPEndPoint(ipa, this.Port);
                if (!_tcpClient.Connected) {
                    _tcpClient.Connect(ipe);
                    this.Connected = true;
                }
            }
            catch (Exception ex) { }
        }
        public void Close()
        {
            try {
                _tcpClient.Close();
                _tcpClient.Dispose();
                //_tcpClient = null;
                this.Connected = false;
            }
            catch (Exception ex) { }
        }
            
        public string SendData(string msg)
        {
            string response = "";
            //Connect();
            try {
                if (_tcpClient.Connected) {
                    SendMsg(msg, _tcpClient);
                }
            }
            catch (Exception ex) {
                _tcpClient.Close();
            }
            //TcpClient.Close();
            return response;
        }
        public async void ReceiveDataAsync()
        {
            string msg = "";

                await Task.Run(() => {
                    msg = ReceiveMsg();
                });

        }

        private void SendMsg(string msg, TcpClient tmpTcpClient)
        {
            try {
                NetworkStream ns = tmpTcpClient.GetStream();
                if (ns.CanWrite) {
                    byte[] msgByte = Encoding.UTF8.GetBytes(msg);
                    ns.Write(msgByte, 0, msgByte.Length);
                }
            }
            catch (Exception ex) {
            }
            
        }
        private string ReceiveMsg()
        {
            string receiveMsg = string.Empty;
            byte[] receiveBytes = new byte[_tcpClient.ReceiveBufferSize];
            int numberOfBytesRead = 0;
            string[] ss = null;
            while (_tcpClient.Connected) {
                try {
                    NetworkStream ns = _tcpClient.GetStream();
                    if (ns.CanRead) {
                        do {
                            numberOfBytesRead = ns.Read(receiveBytes, 0, _tcpClient.ReceiveBufferSize);
                            if (numberOfBytesRead != 0) {
                                receiveMsg = Encoding.UTF8.GetString(receiveBytes, 0, numberOfBytesRead);
                                #region 設定OnUpdateConnectionArgs
                                OnTCPConnectionArgs e = new OnTCPConnectionArgs {
                                    TcpClient = _tcpClient,
                                    Msg = receiveMsg
                                };
                                DoUpdateConnection(e);
                                #endregion
                            }
                            else {
                                Close();
                                this.Connected = false;
                                return "";
                            }
                        }
                        while (ns.DataAvailable);
                    }
                }
                catch (Exception ex) {
                    receiveMsg = "";
                }
            }
            
            return receiveMsg;
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
    }

    public class UDP
    {
        int _port = 0;
        public event EventHandler<OnUDPConnectionArgs> OnUDPConnection;
        public void DoUpdateUDPConnection(OnUDPConnectionArgs OnUDPConnectionArgs)
        {
            EventHandler<OnUDPConnectionArgs> _connection
                = new EventHandler<OnUDPConnectionArgs>(OnUDPConnection);

            if (_connection != null)
            {
                OnUDPConnectionArgs e = new OnUDPConnectionArgs();
                e.Msg = OnUDPConnectionArgs.Msg;

                if (_connection != null)
                    _connection(this, e);//fire event

            }
        }
        public UDP(int port)
        {
            _port = port;
        }
        private void UdpReceiveResult()
        {
            IPEndPoint ipEnd = new IPEndPoint(IPAddress.Any, _port);
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Bind(ipEnd);
            EndPoint ip = (EndPoint)ipEnd; //我真的不知道為何一定要這行才能成功= =，誰能解釋一下
            byte[] getdata = new byte[1024]; //要接收的封包大小
            string receiveMsg = null;
            int recv;
            while (true)
            {
                recv = client.ReceiveFrom(getdata, ref ip); //把接收的封包放進getdata且傳回大小存入recv
                receiveMsg = Encoding.UTF8.GetString(getdata, 0, recv); //把接收的byte資料轉回string型態
                #region 設定OnUpdateConnectionArgs
                OnUDPConnectionArgs e = new OnUDPConnectionArgs
                {
                    Msg = receiveMsg
                };
                DoUpdateUDPConnection(e);
                #endregion
            }


        }
        public async void UdpReceiveResultAsync()
        {
            string msg = "";

            await Task.Run(() => {
                UdpReceiveResult();
            });
        }
    }
}
