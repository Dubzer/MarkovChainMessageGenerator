using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Markov;
using MarkovChainMessageGenerator;
using Newtonsoft.Json;

class Program
{
    private static string _jsonPath = AppDomain.CurrentDomain.BaseDirectory;
    private static MarkovChain<string> _chain = new MarkovChain<string>(1);
    private static int currentGeneration = 0;
    private static readonly HttpClient client = new HttpClient();
    private static bool log = false;
    
    static async Task Main()
    
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            RestoreChain();
        }
        catch 
        {
            Console.WriteLine("CAN'T READ FILE");
        }

        Console.WriteLine("Ready. \n" +
                          "start - Starts generation. Make sure that server is running \n" +
                          "save - Saves progress to file \n" +
                          "gen - Generates message \n" +
                          "exit - Saves progress to file and closes program \n" +
                          "logon - Turns on logs of server \n" +
                          "logoff - Turns off logs of server \n");
        while (true)
        {
            switch (Console.ReadLine())
            {
                case "start":
                    StartGeneration();
                    break;
                case "save":
                    SerializeChain();
                    Console.WriteLine("Saved!");
                    break;
                case "gen":
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine("GENERATED: " + _chain.GenerateSentence(new Random()));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case "exit":
                    SerializeChain();
                    Environment.Exit(0);
                    break;
                case "logon":
                    Console.WriteLine("Turned on!");
                    log = true;
                    break;
                case "logoff":
                    Console.WriteLine("Turned off!");
                    log = false;
                    break;
            }
        }
    }

    static async void StartGeneration()
    {
        await Task.Delay(20);
        string message = "";
        message = await GetAsync("http://127.0.0.1:19566/");

        if(log) 
            Console.WriteLine("GOT: " + message);
        GenerateChain(message);
    }

    static void RestoreChain()
    {
        string itemsJson = FileManipulator.Read(_jsonPath + "items.json");
        string terminalsJson = FileManipulator.Read(_jsonPath + "terminals.json");
        _chain.items = JsonConvert
            .DeserializeObject<List<KeyValuePair<ChainState<string>, Dictionary<string, int>>>>(itemsJson)
            .ToDictionary(x => x.Key, x => x.Value);
        
        _chain.terminals = JsonConvert
            .DeserializeObject<List<KeyValuePair<ChainState<string>, int>>>(terminalsJson)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    private static void GenerateChain(string text)
    {
        Regex rgx0 = new Regex(@"[^\w 0-9 - ']");
        Regex rgx1 = new Regex("[ ]{2,}");

        string finalString = rgx1.Replace( // Removes double spaces
            rgx0.Replace(text, ""), // Removes all junk
            " ");

        _chain.Add(finalString.Split(' ').ToList(), 1);
        currentGeneration++;

        if (currentGeneration == 5000)
        {
            SerializeChain();
        }
        
        StartGeneration();
    }

    private static async Task<string> GetAsync(string uri)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
        await using Stream stream = response.GetResponseStream();
        using StreamReader reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
    
    private static void SerializeChain()
    {
        string itemsJson = JsonConvert.SerializeObject(_chain.items.ToArray());
        string terminalsJson = JsonConvert.SerializeObject(_chain.terminals.ToArray());
        FileManipulator.Write(itemsJson, _jsonPath + "items.json");
        FileManipulator.Write(terminalsJson, _jsonPath + "terminals.json");
        currentGeneration = 0;
    }
}