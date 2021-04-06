using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace LAB1_Bai2
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
            Ckeckstatus();
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000; //number in milisecinds 
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            Ckeckstatus();
        }
        public void Ckeckstatus()
        {
            WriteToFile("Service is started at " + DateTime.Now);
            string day = DateTime.Now.DayOfWeek.ToString();
            //kiem tra process
            Process[] pname = Process.GetProcessesByName("notepad");
            //ktra thoi gian hien tai
            if (day == "Monday" || day == "Wednesday" || day == "Friday" || day == "Thursday") //thu2-4-6-5 cho process run
            {
                //Ktra process co dang chay vao dung thu2-4-6
                if (pname.Length > 0)
                {
                    WriteToFile("Process is Running in " + day);
                    //pname[0].Kill();
                    //WriteToFile("Process killed");
                }
                else
                {
                    WriteToFile("Process is not running in " + day);
                    ProcessStartInfo pstart = new ProcessStartInfo();
                    pstart.FileName = @"C:\Windows\System32\notepad.exe";
                    Process process = Process.Start(pstart);
                    WriteToFile("Process started in " + day);
                }
            }
            else // process stop vao nhung ngay khac
            {
                if (pname.Length > 0)
                {
                    WriteToFile("Process is Running in " + day);
                    pname[0].Kill();
                    WriteToFile("Process killed in " + day);
                }
                else
                {
                    WriteToFile("Process is not running in " + day);
                    /*ProcessStartInfo pstart = new ProcessStartInfo();
                    pstart.FileName = @"C:\Windows\System32\notepad.exe";
                    Process process = Process.Start(pstart);
                    WriteToFile("Process started");*/
                }
            }
        }
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