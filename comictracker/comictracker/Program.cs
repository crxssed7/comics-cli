using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleUI;
using CVNetCore;
using CVNetCore.Constants;
using CVNetCore.Models;
using CVNetCore.Utility;

namespace comictracker
{
    class Program
    {
        public static ComicVine Service = new ComicVine(ApiKey.Key);

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "help":
                        // Show help info
                        Help();
                        break;
                    case "search":
                        // Implement search
                        Search(args[1]);
                        break;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine();
                Console.WriteLine("     ░█████╗░░█████╗░███╗░░░███╗██╗░█████╗░  ████████╗██████╗░░█████╗░░█████╗░██╗░░██╗███████╗██████╗░");
                Console.WriteLine("     ██╔══██╗██╔══██╗████╗░████║██║██╔══██╗  ╚══██╔══╝██╔══██╗██╔══██╗██╔══██╗██║░██╔╝██╔════╝██╔══██╗");
                Console.WriteLine("     ██║░░╚═╝██║░░██║██╔████╔██║██║██║░░╚═╝  ░░░██║░░░██████╔╝███████║██║░░╚═╝█████═╝░█████╗░░██████╔╝");
                Console.WriteLine("     ██║░░██╗██║░░██║██║╚██╔╝██║██║██║░░██╗  ░░░██║░░░██╔══██╗██╔══██║██║░░██╗██╔═██╗░██╔══╝░░██╔══██╗");
                Console.WriteLine("     ╚█████╔╝╚█████╔╝██║░╚═╝░██║██║╚█████╔╝  ░░░██║░░░██║░░██║██║░░██║╚█████╔╝██║░╚██╗███████╗██║░░██║");
                Console.WriteLine("     ░╚════╝░░╚════╝░╚═╝░░░░░╚═╝╚═╝░╚════╝░  ░░░╚═╝░░░╚═╝░░╚═╝╚═╝░░╚═╝░╚════╝░╚═╝░░╚═╝╚══════╝╚═╝░░╚═╝");
                Console.WriteLine();
                Console.WriteLine("     █▀▀ █▀█ ▀▄▀ █▀ █▀ █▀▀ █▀▄");
                Console.WriteLine("     █▄▄ █▀▄ █░█ ▄█ ▄█ ██▄ █▄▀");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("     To get started with Comic Tracker, type `comictracker help`");
            }
        }

        private static void WriteToConsole(string text, ConsoleColor forToColor = ConsoleColor.Gray, ConsoleColor forOgColor = ConsoleColor.Gray, ConsoleColor backToColor = ConsoleColor.Black, ConsoleColor backOgColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = forToColor;
            Console.BackgroundColor = backToColor;

            Console.WriteLine(text);

            Console.ForegroundColor = forOgColor;
            Console.BackgroundColor = backOgColor;
        }

        // <------------------------- COMMANDS ------------------------->

        private static void Help()
        {
            WriteToConsole("Hello, I've come to help you!", ConsoleColor.Cyan, Console.ForegroundColor);
            Console.WriteLine();
            Console.WriteLine("Command          Arguments          Description");
            Console.WriteLine("------------------------------------------------------------------------");
            Console.WriteLine("help             N/A                Shows info on all available commands");
            Console.WriteLine("search           comic-name         Searches for a comic ");
        }

        private static void Search(string query)
        {
            WriteToConsole("Searching for comics...", ConsoleColor.Cyan);
            var volumes = Service.GetVolumesByName(query, 0);

            List<ConsoleMenuItem> items = new List<ConsoleMenuItem>();

            for (int i = 0; i < volumes.Count; i++)
            {
                items.Add(new ConsoleMenuItem<CVVolume>($"{i} " + volumes[i].Name, CallBack, volumes[i]));
            }

            var menu = new ConsoleMenu<string>("Your query returned these results", items);
            menu.RunConsoleMenu();
        }

        private static void CallBack(CVVolume volume)
        {
            Console.WriteLine(volume.SiteDetailUrl);
        }
    }
}
