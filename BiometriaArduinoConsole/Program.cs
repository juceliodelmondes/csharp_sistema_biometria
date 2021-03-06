﻿using System;
using System.IO.Ports;
using System.Net.Http;
using System.Speech.Synthesis;

namespace BiometriaArduinoConsole {
    public class ValidationRequest
    {
        public int id { get; set; }
        public int confidence { get; set; }
    }

    public class CommandResponse
    {
        public String command { get; set; }

        public String commandParameter { get; set; }

        public String textSpeech { get; set; }
    }

    public class StatusBiometry
    {
        public int idBiometry { get; set; }
        
        public String status { get; set; }
    }

    class Program
    {
        static SerialPort serial = new SerialPort();
        public static HttpClient client = new HttpClient();
        private static SpeechSynthesizer sp = new SpeechSynthesizer();
        static void Main(string[] args)
        {
            serial.PortName = "COM6";
            serial.BaudRate = 9600;
            serial.Open();
            serial.DataReceived += new SerialDataReceivedEventHandler(read);
            sp = new SpeechSynthesizer();
            while (true)
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
            else if(array[0].Equals("?")) //procurar comandos
            {
                procurarComandos();
            }
            else if(array[0].Equals("Waiting") || array[0].Equals("Removef") || array[0].Equals("Waiting2") || 
                array[0].Equals("Recorded") || array[0].Equals("Error")) {
                atualizarStatus(array[0], Convert.ToInt32(array[1]));
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
            try
            {
                Console.WriteLine("Validando acesso: " + id);
                ValidationRequest info = new ValidationRequest();
                info.id = id;
                info.confidence = confidence;
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    "http://192.168.1.100:8080/user/validate/", info);
                response.EnsureSuccessStatusCode();
                CommandResponse responseAcess = await response.Content.ReadAsAsync<CommandResponse>();
                write(responseAcess.command, responseAcess.commandParameter);
                falar(responseAcess.textSpeech);
            }
            catch (HttpRequestException er)
            {

            }
            catch (Exception er)
            {

            }
        }

        static async void procurarComandos()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("http://localhost:8080/command/getCommand");
                response.EnsureSuccessStatusCode();
                CommandResponse responseAcess = await response.Content.ReadAsAsync<CommandResponse>();
                write(responseAcess.command, responseAcess.commandParameter);
            }
            catch(HttpRequestException er)
            {
                
            }
            catch(Exception er)
            {

            }
        }

        static async void atualizarStatus(String information, int idBiometry)
        {
            try
            {
                Console.WriteLine("Enviando status: " + information + " "+idBiometry);
                StatusBiometry status = new StatusBiometry();
                status.idBiometry = idBiometry;
                status.status = information;
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    "http://192.168.1.100:8080/biometry/setRegisterStatus/", status);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException er)
            {

            }
            catch (Exception er)
            {

            }
        }

        private static void falar(string text)
        {
            sp.SpeakAsyncCancelAll();
            sp.SpeakAsync(text);
        }
    }
}
