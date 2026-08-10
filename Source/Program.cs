using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace CSH030Ex
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool isNew;

            Mutex mutex = new Mutex(true, "FZ_Test", out isNew);
            //if ( isNew)
            //{

            //    Application.EnableVisualStyles();
            //    Application.SetCompatibleTextRenderingDefault(false);
            //    Application.Run(new F_Main());

            //    mutex.ReleaseMutex();
            //    System.Diagnostics.Process.GetCurrentProcess().Kill();
            //}
            //else
            //{
            //    MessageBox.Show("Still Running Process .....");
            //    Application.Exit();
            //}
            if (isNew)
            {
                /////////////////////////////////////////////
                /////////////////////////////////////////////
                /////////////////////////////////////////////
                //DateTime thisTime = DateTime.Now;
                //DateTime thatTime;

                //int lLastCount = 25;
                //string LicFilePath = "C:\\CSHTest\\RunData\\Basler_10tap_280_742.dcf";
                //if (File.Exists(LicFilePath))
                //{
                //    StreamReader sr = new StreamReader(LicFilePath);
                //    string fullLine = sr.ReadToEnd();
                //    sr.Close();
                //    string[] eachLine = fullLine.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                //    lLastCount = Convert.ToInt32(eachLine[1]);
                //    if (lLastCount == 0)
                //    {
                //        MessageBox.Show("Program License Expired!!. Please Contact Provider\n");
                //        mutex.ReleaseMutex();
                //        System.Diagnostics.Process.GetCurrentProcess().Kill();
                //    }
                //    thatTime = DateTime.ParseExact(eachLine[0], "HHmmss", null); //before
                //                                                                 //   지난번 시점이 이번시점보다 크면 카운트 다운, 지난번시점이 이번시점보다 작으면 그대로 
                //                                                                 //MessageBox.Show(thisTime.Hour.ToString() + " " + thatTime.Hour.ToString())
                //    if (thisTime.Hour < thatTime.Hour || (thisTime.Hour == thatTime.Hour && thisTime.Minute < thatTime.Minute))
                //        lLastCount--;
                //}
                //else
                //{
                //    lLastCount = 25;
                //}
                //StreamWriter wr = new StreamWriter(LicFilePath);
                //wr.WriteLine(thisTime.ToString("HHmmss"));
                //wr.WriteLine(lLastCount.ToString());
                //int itmp = 0;
                //for (int i = 0; i < 53568; i++)
                //{
                //    itmp = (int)(Math.Sin(i + 2 + lLastCount) * 9999999);
                //    wr.Write(itmp.ToString("X2"));
                //}
                //wr.Close();
                /////////////////////////////////////////////
                /////////////////////////////////////////////
                /////////////////////////////////////////////


                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //Splasher.Show();
                Application.Run(new F_Main());
                //Splasher.Close();


                mutex.ReleaseMutex();
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                //}
                //else
                //{
                //    //error message //jyj 170725
                //    MessageBox.Show("Unauthorized License. Unable to excute program.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    Application.Exit();
                //}
            }
            else
            {
                MessageBox.Show("Still Running Process .....");
                Application.Exit();
            }

        }
    }
}
