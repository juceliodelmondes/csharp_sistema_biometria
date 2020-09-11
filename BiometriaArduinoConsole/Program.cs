using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace BiometriaArduinoConsole {
    class Program
    {
        static SerialPort serial = new SerialPort();
        static void Main(string[] args)
        {
            serial.PortName = "COM6";
            serial.BaudRate = 9600;
            serial.Open();
            serial.DataReceived += new SerialDataReceivedEventHandler(ler);
            while(true)
            {
                String cmd = Console.ReadLine();
                if(cmd.Equals("exit"))
                {
                    Environment.Exit(1);
                } 
                else
                {
                    serial.Write(cmd);
                }
            }
        }

        private static void ler(Object sender, EventArgs e)
        {
            Console.WriteLine(serial.ReadLine());
        }
    }
}
