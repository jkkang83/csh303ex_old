using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace TZAF
{
    class MySocketServerClass : SocketInterface
    {


        private Socket m_Server;
        private List<Socket> m_Client;
        IPEndPoint ip;

        private byte[] szData;

        private MySocketServerClass(int nPort, TypeOfString type) : base(type)
        {
            ip = new IPEndPoint(IPAddress.Any, nPort);
        }

        ~MySocketServerClass()
        {
            EndSocket();
        }

        public static MySocketServerClass CreateServerSocket(int nPort, TypeOfString type)
        {
            MySocketServerClass serverObject = new MySocketServerClass(nPort, type);
            return serverObject;
        }
        public override void StartSocket()
        {
            try
            {
                if (m_Server == null)
                {
                    m_Client = new List<Socket>();
                    m_Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    m_Server.Bind(ip);
                    m_Server.Listen(0);
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectedClient);
                    //클라이언트 접속 이벤트 발생대기
                    m_Server.AcceptAsync(args);
                }

            }
            catch
            { }


        }

        public override void EndSocket()
        {
            //if (m_Client == null)
            //    return;

            foreach (Socket socket in m_Client)
            {
                if (socket.Connected)
                {
                    socket.Disconnect(false);

                }

                socket.Dispose();
            }

            if (m_Server != null)
                m_Server.Dispose();
            m_Server = null;
        }

        public override void SendMessage(string sMessage)
        {
            SendToClients(sMessage);
        }

        private void ConnectedClient(object sender, SocketAsyncEventArgs e)
        {
            Socket client = e.AcceptSocket;

            try
            {
                if (m_Client != null)
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    szData = new byte[4096];
                    args.SetBuffer(szData, 0, 4096);
                    args.UserToken = m_Client;
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveData);
                    client.ReceiveAsync(args);
                    m_Client.Add(client);

                    base.Connected(((IPEndPoint)client.RemoteEndPoint).Address.ToString());

                }
            }
            catch (Exception ee)
            {
                Debug.Write(ee.ToString());
            }
            try
            {
                e.AcceptSocket = null;
                m_Server.AcceptAsync(e);    //다시 클라이언트 접속 요청 대기
            }
            catch (NullReferenceException eNull)
            {
                Debug.Write(eNull.ToString());
            }
        }

        private void ReceiveData(object sender, SocketAsyncEventArgs e)
        {
            //데이터를 수신하거나 클라이언트의 연결이 끊어지면 호출되는 이벤트
            Socket client = (Socket)sender;
            if (client.Connected && e.BytesTransferred > 0)
            {

                byte[] szData = e.Buffer;

                string sReciveData = base.ByteToString(szData);
                //Buffer 비우기
                Array.Clear(szData, 0, szData.Length);

                base.Receive(((IPEndPoint)client.RemoteEndPoint).Address.ToString(), sReciveData);
                e.SetBuffer(szData, 0, 4096);
                client.ReceiveAsync(e);
            }
            else //연결 끊어짐.
            {
                try
                {
                    if (client.Connected)
                        client.Disconnect(false);
                }
                catch (SocketException eSock)
                {
                    Debug.Write("Socket Error, ErrMsg : " + eSock.ToString());
                }
                finally
                {
                    //base.Disconnected(((IPEndPoint)client.RemoteEndPoint).Address.ToString());
                    //Debug.Write(client.RemoteEndPoint.ToString() + "연결이 끊어짐");
                    base.Disconnected(((IPEndPoint)client.RemoteEndPoint).Address.ToString());
                    m_Client.Remove(client);
                    client = null;

                }
            }
        }

        private void SendToClients(string sSendData)
        {
            if (m_Client == null)
            {
                return;
            }
            byte[] data = base.StringToByte(sSendData);

            foreach (Socket sock in m_Client)
            {
                sock.Send(data, data.Length, SocketFlags.None);
            }
        }

        private void SendToClient(string sIP, string sSendData)
        {
            if (m_Client == null)
            {
                return;
            }
            byte[] data = base.StringToByte(sSendData);

            var client = from ip in m_Client
                         where ((IPEndPoint)ip.RemoteEndPoint).Address.ToString() == sIP
                         select ip;

            foreach (Socket sock in client)
            {
                sock.Send(data, data.Length, SocketFlags.None);
            }
        }

        public bool isConnectedIP(string ip)
        {
            bool bIsConnected = false;

            for (int i = 0; i < m_Client.Count; i++)
            {
                if (((IPEndPoint)m_Client[i].RemoteEndPoint).Address.ToString().CompareTo(ip) == 0)
                {
                    bIsConnected = true;
                    break;
                }
            }
            return bIsConnected;
        }
    }
}
