using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Net.Http;

namespace BiometriaArduinoConsole {
    public class RequestAccess
    {
        public int id { get; set; }
        public int confidence { get; set; }
    }

    public class ResponseAccess
    {
        public String command { get; set; }

        public String commandParameter { get; set; }
    }
    class Program
    {
        static SerialPort serial = new SerialPort();
        public static HttpClient client = new HttpClient();
        static void Main(string[] args)
        {
            serial.PortName = "COM6";
            serial.BaudRate = 9600;
            serial.Open();
            serial.DataReceived += new SerialDataReceivedEventHandler(read);
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

        //realiza a leitura do arduino (evento)
        private static void read(Object sender, EventArgs e)
        {
            String leitura = serial.ReadLine();
            String[] array = leitura.Split(' ');
            if(array[0].Equals("access"))
            {
                validarAcesso(Convert.ToInt32(array[1]), Convert.ToInt32(array[2]));
            } 
            else
            {
                Console.WriteLine(leitura);
            }
        }

        //Envia o comando para o arduino
        private static void write(String command, String commandParameter)
        {
            if (command != null)
            {
                serial.Write(command + " " + commandParameter);
            }
        }

        //envia o Id cadastrado no arduino para a API
        static async void validarAcesso(int id, int confidence)
        {
            Console.WriteLine("Validando acesso: " + id);
            RequestAccess info = new RequestAccess();
            info.id = id;
            info.confidence = confidence;
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "http://localhost:8080/user/validate/", info);
            response.EnsureSuccessStatusCode();
            ResponseAccess responseAcess = await response.Content.ReadAsAsync<ResponseAccess>();
            Console.WriteLine(responseAcess.command);
            Console.WriteLine(responseAcess.commandParameter);
            write(responseAcess.command, responseAcess.commandParameter);
        }
    }
}
