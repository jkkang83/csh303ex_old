using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace TZAF
{
    public class Network
    {
        public event EventHandler<string> LogEvened = null;
        public event EventHandler<byte []> ReciveEvened = null;


        public TcpClient Tcp { get; set; }
        private NetworkStream SendStream { get; set; }
        private NetworkStream ReciveStream { get; set; }
        public Network()
        {

        }
        public void Connect(string ip, int port) //For Client
        {
            try
            {
                Tcp = new TcpClient(ip, port);
                if (LogEvened != null) LogEvened.Invoke(this, "Connet Complelete");
                Task.Factory.StartNew(() => ReciveData());
            }
            catch
            {
                if (LogEvened != null) LogEvened.Invoke(this, "Can't Find Server");
            }
        }
        public void DisConnect()
        {
            Tcp.Close();
        }
        public void StartServer(int port) //For Server
        {
            try
            {
                Tcp = new TcpClient();
                Task.Factory.StartNew(() => Listen(port));
            }
            catch
            {
                if (LogEvened != null) LogEvened.Invoke(this, "Server Start Failed");
            }
        }
        private void ReciveData()
        {
            try
            {
                ReciveStream = Tcp.GetStream();
                int totalLenth = 0;
                List<byte[]> byteList = new List<byte[]>();
                int lenth;
                do
                {
                    byte[] bytes = new byte[1024];
                    lenth = ReciveStream.Read(bytes, 0, bytes.Length);
                    if (lenth == 0) break;
                    totalLenth += lenth;
                    byteList.Add(bytes);
                    if (!ReciveStream.DataAvailable)
                    {
                        byte[] buff = new byte[totalLenth];
                        for (int i = 0; i < byteList.Count; i++)
                        {
                            int size;
                            if (i == byteList.Count - 1) size = lenth;
                            else size = byteList[i].Length;
                            Array.Copy(byteList[i], 0, buff, 1024 * i, size);
                        }
                        if(ReciveEvened != null) 
                            ReciveEvened.Invoke(this, buff);
                        
                        totalLenth = 0;
                        byteList.Clear();
                    }
                }
                while (true);
            }
            catch
            {
                ReciveStream.Close();
                if (LogEvened != null) LogEvened.Invoke(this, "Network Forced Disconnected");
            }
        }
        private void Listen(int port)
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();

            while (true)
            {
                if (LogEvened != null) 
                    LogEvened.Invoke(this, "Server Listen Start");
                
                Tcp = tcpListener.AcceptTcpClient();

                IPEndPoint ipEndPoint = (IPEndPoint)Tcp.Client.RemoteEndPoint;
                if (LogEvened != null) 
                    LogEvened.Invoke(this, "Client IP : " + ipEndPoint.Address.ToString());

                ReciveData();

                if (LogEvened != null) 
                    LogEvened.Invoke(this, "Client Disconnected.");

                Tcp.Close();
            }
        }
        public void SendData(byte[] buff)
        {
            try
            {
                if (Tcp == null)
                {
                    if (LogEvened != null) LogEvened.Invoke(this, "Network Not Connected.");
                    return;
                }
                SendStream = Tcp.GetStream();
                SendStream.Write(buff, 0, buff.Length);
                if (LogEvened != null) LogEvened.Invoke(this, "Send Data Complete");
            }
            catch
            {
                if (LogEvened != null) LogEvened.Invoke(this, "Network Not Connected.");
            }
        }
    }
}

