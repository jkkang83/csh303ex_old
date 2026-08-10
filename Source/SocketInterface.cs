using System.Text;

namespace CSH030Ex
{
    abstract class SocketInterface
    {

        public enum TypeOfString
        {
            UNICODE,
            UTF8
        }


        public delegate void ReceiveMessage(string IPAddress, string sMessage);
        /// <summary>
        /// void Receive(string ip, string msg);
        /// </summary>
        public event ReceiveMessage ReceiveEvent;

        public delegate void ConnectedStateMessage(string IPAddress);
        /// <summary>
        /// TCP/IP 연결 성공시 이벤트
        /// </summary>
        public event ConnectedStateMessage ConnectedEvent;
        /// <summary>
        /// TCP/IP 연결 해제시 이벤트
        /// </summary>
        public event ConnectedStateMessage DisconnectedEvent;
        TypeOfString stringType;

        abstract public void StartSocket();
        abstract public void EndSocket();
        abstract public void SendMessage(string sMessage);


        protected SocketInterface(TypeOfString type)
        {
            stringType = type;
        }
        ~SocketInterface()
        {
        }
        protected void Receive(string IPAddress, string sMessage)
        {
            if (ReceiveEvent != null)
            {
                ReceiveEvent(IPAddress, sMessage);
            }
        }

        protected byte[] StringToByte(string sString)
        {
            byte[] data = null;
            switch (stringType)
            {
                case TypeOfString.UNICODE:
                    data = Encoding.Unicode.GetBytes(sString);
                    break;
                case TypeOfString.UTF8:
                    data = Encoding.UTF8.GetBytes(sString);//new UTF8Encoding().GetBytes(sString);
                    break;
                default:
                    break;
            }
            return data;
        }

        protected void Connected(string sIP)
        {
            if (ConnectedEvent != null)
            {
                ConnectedEvent(sIP);
            }

        }
        protected void Disconnected(string sIP)
        {
            if (DisconnectedEvent != null)
            {
                DisconnectedEvent(sIP);
            }
        }

        protected string ByteToString(byte[] byteString)
        {
            string sString = null;
            switch (stringType)
            {
                case TypeOfString.UNICODE:
                    sString = Encoding.Unicode.GetString(byteString);
                    break;
                case TypeOfString.UTF8:
                    sString = new UTF8Encoding().GetString(byteString);//Encoding.UTF8.GetString(byteString);//new UTF8Encoding().GetString(byteString);
                    break;
                default:
                    break;
            }
            return sString;
        }
    }
}
