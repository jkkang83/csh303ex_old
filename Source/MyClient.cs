using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace TZAF
{
    class MySocketClientClass : SocketInterface
    {

        private Socket m_Client;
        private Socket m_cbSock;

        IPEndPoint ip;
        private byte[] recvBuffer;

        System.Timers.Timer Recon = null;
        bool m_bConnected = false;

        private MySocketClientClass(string sIP, int nPort, TypeOfString type) : base(type)
        {
            ip = new IPEndPoint(IPAddress.Parse(sIP), nPort);

            Recon = new System.Timers.Timer();
            Recon.Interval = 5000;
            Recon.Enabled = false;
            Recon.Elapsed += Recon_Elapsed;

        }

        private void Recon_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Recon.Enabled = false;
            if (m_Client != null)
            {
                m_Client.Dispose();
                m_Client = null;
            }

            StartSocket();
        }

        ~MySocketClientClass()
        {
            EndSocket();
        }

        public static MySocketClientClass CreateClientSocket(string sIP, int nPort, TypeOfString type)
        {
            MySocketClientClass clientObject = new MySocketClientClass(sIP, nPort, type);
            return clientObject;
        }

        public override void StartSocket()
        {
            if (m_Client == null)
            {
                m_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                recvBuffer = new byte[4096];
                BeginConnect();
            }
        }

        public override void EndSocket()
        {
            if (m_Client == null)
                return;

            try
            {
                m_bConnected = false;
                m_Client.Disconnect(false);
            }
            catch (SocketException e)
            {
                Debug.Write(e.Message);
            }
            finally
            {

                m_Client.Dispose();
                m_Client = null;
            }
        }

        public override void SendMessage(string sMessage)
        {
            if (m_Client != null)
            {
                BeginSend(sMessage);
            }
        }

        //[Obsolete("서버용 기능")]

        private void BeginConnect()
        {
            try
            {
                m_Client.BeginConnect(ip, new AsyncCallback(ConnectCallBack), m_Client);
            }
            catch (SocketException e)
            {

                Debug.Write(e.Message);
                m_Client.Disconnect(false);
                m_Client.Dispose();
                m_Client = null;
                //    StartSocket();
                Recon.Enabled = true;
            }
        }

        private void ConnectCallBack(IAsyncResult IAR)
        {
            try
            {
                Socket sock = (Socket)IAR.AsyncState;
                IPEndPoint serverIP = (IPEndPoint)sock.RemoteEndPoint;
                Debug.Write("Server IP : " + serverIP.Address.ToString());
                sock.EndConnect(IAR);
                m_cbSock = sock;
                m_cbSock.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveCallBack), m_cbSock);
                m_bConnected = true;

                base.Connected(((IPEndPoint)sock.RemoteEndPoint).Address.ToString());


            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.NotConnected)
                {
                    Debug.Write("Not Connected");
                    m_Client.Dispose();
                    m_Client = null;
                    Recon.Enabled = true;

                }
            }
            catch
            {
                m_bConnected = false;
            }
        }

        private void BeginSend(string sSendData)
        {
            try
            {
                if (m_Client.Connected)
                {


                    byte[] buffer = base.StringToByte(sSendData);
                    m_Client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallBack), sSendData);

                }
                else
                {
                    throw new SocketException((int)SocketError.NotConnected);
                }
            }
            catch (SocketException e)
            {
                Debug.Write("Send Fail ErrMsg : " + e.Message);
            }
        }


        private void SendCallBack(IAsyncResult IAR)
        {
            //  string sMsg = (string)IAR.AsyncState;
            //  m_Client.EndSend(IAR);

            Debug.Write(" 전송완료");
        }

        private void Receive()
        {
            m_cbSock.BeginReceive(this.recvBuffer, 0, recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveCallBack), m_cbSock);
        }
        private void OnReceiveCallBack(IAsyncResult IAR)
        {
            if (!m_bConnected)
            {
                //소켓 해제됨.
                return;
            }
            try
            {
                Socket sock = (Socket)IAR.AsyncState;
                if (!sock.Connected)
                {
                    throw new SocketException((int)SocketError.NotConnected);
                }
                int nReadSize = sock.EndReceive(IAR);
                if (nReadSize != 0)
                {
                    string sData = base.ByteToString(recvBuffer);
                    Array.Clear(recvBuffer, 0, recvBuffer.Length);
                    Debug.Write(sData);
                    base.Receive(((IPEndPoint)sock.RemoteEndPoint).Address.ToString(), sData);

                }
                else
                {
                    base.Disconnected(((IPEndPoint)sock.RemoteEndPoint).Address.ToString());

                    throw new SocketException((int)SocketError.NotConnected);
                }

                Receive();
            }
            catch (SocketException e)
            {


                if (e.SocketErrorCode == SocketError.NotConnected)
                {
                    //BeginConnect();
                    Recon.Enabled = true;
                }
                else if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Recon.Enabled = true;
                }
            }
        }
    }
}
