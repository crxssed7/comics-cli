using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using comictracker.Models;
using ConsoleUI;
using CVNetCore;
using CVNetCore.Constants;
using CVNetCore.Models;
using CVNetCore.Utility;
using Newtonsoft.Json;

namespace comictracker
{
    class Program
    {
        public static ComicVine Service = new ComicVine(ApiKey.Key);

        private static int CurrentPage = 0;
        private static int IssuesPage = 0;
        private static int IssuesPageSize = 10;

        private static string JSONLocation = "";

        public static UserJSON UserData = new UserJSON();

        static void Main(string[] args)
        {
            JSONLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\crxssed7\\comictracker\\comics.json";
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\crxssed7\\comictracker");

            if (args.Length > 0)
            {
                Deserialize();

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
                    case "coll":
                        // Show Collection
                        ShowCollection();
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

        private static void WriteToConsole(string text, bool returnLine, ConsoleColor forToColor = ConsoleColor.Gray, ConsoleColor forOgColor = ConsoleColor.Gray, ConsoleColor backToColor = ConsoleColor.Black, ConsoleColor backOgColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = forToColor;
            Console.BackgroundColor = backToColor;

            if (returnLine)
                Console.WriteLine(text);
            else
                Console.Write(text);

            Console.ForegroundColor = forOgColor;
            Console.BackgroundColor = backOgColor;
        }

        private static void Deserialize()
        {
            if (File.Exists(JSONLocation))
            {
                using (StreamReader reader = new StreamReader(JSONLocation))
                {
                    string json = reader.ReadToEnd();

                    UserData = JsonConvert.DeserializeObject<UserJSON>(json);
                }
            }
        }

        private static void Serialize()
        {
            Console.WriteLine("Saving data...");
            try
            {
                using (StreamWriter writer = new StreamWriter(JSONLocation))
                {
                    using (JsonWriter jwriter = new JsonTextWriter(writer))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(jwriter, UserData);
                    }
                }

                Console.WriteLine("Data saved!");
            }
            catch
            {
                Console.WriteLine("Saving data failed.");
            }
        }

        // <------------------------- COMMANDS ------------------------->

        private static void Help()
        {
            WriteToConsole("Hello, I've come to help you!", true, ConsoleColor.Cyan, Console.ForegroundColor);
            Console.WriteLine();
            Console.WriteLine("Command          Arguments          Description");
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine("help             N/A                Shows info on all available commands");
            Console.WriteLine("search           comic-name         Searches for a comic ");
            Console.WriteLine("coll             N/A                Shows all the comics in your collection ");
        }

        private static void Search(string query)
        {
            Console.Clear();
            WriteToConsole("Searching for comics...", true, ConsoleColor.Cyan);
            var volumes = Service.GetVolumesByName(query, CurrentPage);

            List<ConsoleMenuItem> items = new List<ConsoleMenuItem>();

            if (volumes.Count > 0)
            {
                for (int i = 0; i < volumes.Count; i++)
                {
                    if (i == 0 && CurrentPage > 0)
                    {
                        items.Add(new ConsoleMenuItem<string>("... load less!", CallBackShowLessResults, query));
                        items.Add(new ConsoleMenuSeperator());
                    }
                    items.Add(new ConsoleMenuItem<CVVolume>($"{CurrentPage + i + 1} {volumes[i].Name} ({volumes[i].StartYear}): {volumes[i].Id}", CallBack, volumes[i]));
                }

                items.Add(new ConsoleMenuSeperator());

                if (volumes.Count == 10)
                    items.Add(new ConsoleMenuItem<string>("... load more!", CallBackShowMoreResults, query));

                items.Add(new ConsoleMenuItem<string>("... exit", CallBackExitSearch, query));

                var menu = new ConsoleMenu<string>("Your query returned these results", items);
                menu.RunConsoleMenu();
            }
            else
            {
                Console.WriteLine("No results for query... try searching again!");
            }
        }

        private static void ShowCollection()
        {
            if (UserData.Comics.Count > 0)
            {
                Console.Clear();
                List<ConsoleMenuItem> items = new List<ConsoleMenuItem>();
                for (int i = 0; i < UserData.Comics.Count; i++)
                {
                    items.Add(new ConsoleMenuItem<Comic>(UserData.Comics[i].Name, ShowVolumeDetails, UserData.Comics[i]));
                }

                items.Add(new ConsoleMenuSeperator());
                items.Add(new ConsoleMenuItem<string>("Exit", CallBackExitSearch, ""));

                var menu = new ConsoleMenu<string>("Your current collection", items);
                menu.RunConsoleMenu();
            }
        }

        private static void CallBack(CVVolume volume)
        {
            ShowVolumeDetails(volume);
        }

        private static void CallBackShowMoreResults(string query)
        {
            CurrentPage = CurrentPage + 10;
            Search(query);
        }

        private static void CallBackShowLessResults(string query)
        {
            CurrentPage = CurrentPage - 10;
            Search(query);
        }

        private static void CallBackExitSearch(string query)
        {
            Console.Clear();
            // Console.WriteLine("Bye!");
        }

        private static void ShowVolumeDetails(CVVolume volume)
        {
            Console.Clear();
            // Name
            WriteToConsole(volume.Name, false, ConsoleColor.DarkCyan);
            Console.Write($" - id: {volume.Id}");
            Console.WriteLine();
            Console.WriteLine();
            // Start Year
            Console.WriteLine(volume.StartYear);
            Console.WriteLine();
            // Description
            string description = volume.Description == null ? "No description provided." : volume.Description;
            Console.WriteLine(StripHTML(description));
            Console.WriteLine();
            // URL
            Console.WriteLine(volume.SiteDetailUrl);
            ShowVolumeOptions(volume);
        }

        private static void ShowVolumeDetails(Comic volume)
        {
            Console.Clear();
            // Name
            WriteToConsole(volume.Name, false, ConsoleColor.DarkCyan);
            Console.Write($" - id: {volume.Id}");
            Console.WriteLine();
            Console.WriteLine();
            // Start Year
            Console.WriteLine(volume.StartYear);
            Console.WriteLine();
            // Description
            Console.WriteLine(volume.Description);
            Console.WriteLine();
            // URL
            Console.WriteLine(volume.URL);
            ShowVolumeOptions(volume);
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<[a-zA-Z/].*?>", "").Replace("amp;", "");
        }

        private static void ShowVolumeOptions(CVVolume volume)
        {
            if (UserData.Comics.Count > 0)
            {
                var existingComic = UserData.Comics.Where(comicLib => comicLib.Id == volume.Id);
                // Add comic, View Issues, Open in ComicVine, Exit
                List<ConsoleMenuItem> items = new List<ConsoleMenuItem>()
                {
                    new ConsoleMenuItem<CVVolume>("View Issues", ViewIssues, volume),
                    existingComic.Count() == 0 ? new ConsoleMenuItem<CVVolume>("Add Comic", AddComic, volume) : new ConsoleMenuItem<CVVolume>("Remove Comic", RemoveComic, volume),
                    new ConsoleMenuItem<string>("Open in ComicVine", OpeninVine, volume.SiteDetailUrl),
                    new ConsoleMenuSeperator(),
                    new ConsoleMenuItem<string>("Exit", CallBackExitSearch, "")
                };
                var menu = new ConsoleMenu<string>("Options", items);
                menu.RunConsoleMenu();
            }
            else
            {
                // There is nothing in the library add it without checking if its already there
                List<ConsoleMenuItem> items = new List<ConsoleMenuItem>()
                {
                    new ConsoleMenuItem<CVVolume>("View Issues", ViewIssues, volume),
                    new ConsoleMenuItem<CVVolume>("Add Comic", AddComic, volume),
                    new ConsoleMenuItem<string>("Open in ComicVine", OpeninVine, volume.SiteDetailUrl),
                    new ConsoleMenuSeperator(),
                    new ConsoleMenuItem<string>("Exit", CallBackExitSearch, "")
                };
                var menu = new ConsoleMenu<string>("Options", items);
                menu.RunConsoleMenu();
            }
        }

        private static void ShowVolumeOptions(Comic comic)
        {
            List<ConsoleMenuItem> items = new List<ConsoleMenuItem>()
            {
                new ConsoleMenuItem<Comic>("View Issues", ViewIssues, comic),
                new ConsoleMenuItem<Comic>("Remove Comic", RemoveComic, comic),
                new ConsoleMenuItem<string>("Open in ComicVine", OpeninVine, comic.URL),
                new ConsoleMenuSeperator(),
                new ConsoleMenuItem<string>("Exit", CallBackExitSearch, "")
            };
            var menu = new ConsoleMenu<string>("Options", items);
            menu.RunConsoleMenu();
        }

        private static void AddComic(CVVolume volume)
        {
            // Add a comic 
            Console.WriteLine("Adding comic...");

            Comic comic = new Comic()
            {
                Id = volume.Id.Value,
                Name = volume.Name + " (" + volume.StartYear + ")",
                Description = volume.Description == null ? "No description provided." : StripHTML(volume.Description),
                StartYear = volume.StartYear,
                URL = volume.SiteDetailUrl
            };

            var issues = Service.GetIssuesByVolume(volume.Id.Value);

            for (int i = 0; i < issues.Count; i++)
            {
                Models.Issue issue = new Models.Issue()
                {
                    Id = issues[i].Id.Value,
                    Name = issues[i].Name,
                    Description = issues[i].Description,
                    IssueNumber = issues[i].IssueNumber,
                    IssueYear = issues[i].IssueYear,
                    URL = issues[i].SiteDetailUrl,
                    VolumeName = comic.Name
                };
                comic.Issues.Add(issue);
            }

            UserData.Comics.Add(comic);

            Console.WriteLine("Comic added!");

            Serialize();
        }

        private static void RemoveComic(CVVolume volume)
        {
            // Remove the comic
            Console.WriteLine("Removing comic...");
            var comic = UserData.Comics.Where(c => c.Id == volume.Id.Value).First();
            UserData.Comics.Remove(comic);
            Console.WriteLine("Comic Removed!");
            Serialize();
        }

        private static void RemoveComic(Comic comic)
        {
            // Remove the comic
            Console.WriteLine("Removing comic...");
            UserData.Comics.Remove(comic);
            Console.WriteLine("Comic Removed!");
            Serialize();
        }

        private static void ViewIssues(CVVolume volume)
        {
            // View issues
            // Get Issues
            Console.Clear();
            var response = Service.GetIssuesByVolume(volume.Id.Value);
            
            int maximumPages = Convert.ToInt32(Math.Ceiling((double)response.Count / (double)IssuesPageSize));

            var issues = GetPage(response, IssuesPage, IssuesPageSize);
            
            List<ConsoleMenuItem> items = new List<ConsoleMenuItem>();
            
            if (IssuesPage > 0)
            {
                // Add the '... load less' button
                items.Add(new ConsoleMenuItem<CVVolume>("... load less!", CallBackShowLessIssues, volume));
                items.Add(new ConsoleMenuSeperator());
            }
            
            for (int i = 0; i < issues.Count; i++)
            {
                issues[i].Volume = volume;
                ConsoleMenuItem<CVNetCore.Models.Issue> item = new ConsoleMenuItem<CVNetCore.Models.Issue>($"Issue: {issues[i].IssueNumber} - {issues[i].Name}", ShowIssueDetails, issues[i]);
                items.Add(item);
            }

            items.Add(new ConsoleMenuSeperator());

            if (IssuesPage + 1 < maximumPages)
            {
                // Add load more button
                items.Add(new ConsoleMenuItem<CVVolume>("... load more!", CallBackShowMoreIssues, volume));
            }

            items.Add(new ConsoleMenuItem<string>("... exit", CallBackExitSearch, ""));

            var menu = new ConsoleMenu<string>($"{volume.Name} ({volume.StartYear}) Issues - Page {IssuesPage + 1}", items);
            menu.RunConsoleMenu();
        }

        private static void ViewIssues(Comic comic)
        {
            // View issues
            Console.Clear();
            int maximumPages = Convert.ToInt32(Math.Ceiling((double)comic.Issues.Count / (double)IssuesPageSize));

            var issues = GetPage(comic.Issues, IssuesPage, IssuesPageSize);

            List<ConsoleMenuItem> items = new List<ConsoleMenuItem>();

            if (IssuesPage > 0)
            {
                // Add the '... load less' button
                items.Add(new ConsoleMenuItem<Comic>("... load less!", CallBackShowLessIssues, comic));
                items.Add(new ConsoleMenuSeperator());
            }

            for (int i = 0; i < issues.Count; i++)
            {
                string tick = issues[i].Read == true ? "/ " : "";
                ConsoleMenuItem<Models.Issue> item = new ConsoleMenuItem<Models.Issue>($"{tick}Issue: {issues[i].IssueNumber} - {issues[i].Name}", ShowIssueDetails, issues[i]);
                items.Add(item);
            }

            items.Add(new ConsoleMenuSeperator());

            if (IssuesPage + 1 < maximumPages)
            {
                // Add load more button
                items.Add(new ConsoleMenuItem<Comic>("... load more!", CallBackShowMoreIssues, comic));
            }

            items.Add(new ConsoleMenuItem<string>("... exit", CallBackExitSearch, ""));

            var menu = new ConsoleMenu<string>($"{comic.Name} Issues - Page {IssuesPage + 1}", items);
            menu.RunConsoleMenu();
        }

        private static void OpeninVine(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        private static IList<Models.Issue> GetPage(IList<Models.Issue> list, int page, int pageSize)
        {
            return list.Skip(page * pageSize).Take(pageSize).ToList();
        }

        private static IList<CVNetCore.Models.Issue> GetPage(IList<CVNetCore.Models.Issue> list, int page, int pageSize)
        {
            return list.Skip(page * pageSize).Take(pageSize).ToList();
        }

        private static void ShowIssueDetails(CVNetCore.Models.Issue issue)
        {
            Console.Clear();
            WriteToConsole($"{issue.Volume.Name}", false, ConsoleColor.DarkCyan);
            Console.WriteLine();
            Console.WriteLine();
            // Name
            string name = issue.Name == null ? "Untitled" : issue.Name;
            WriteToConsole($"Issue {issue.IssueNumber} - {name}", false, ConsoleColor.DarkBlue);
            Console.Write($" - id: {issue.Id}");
            Console.WriteLine();
            Console.WriteLine();
            // Start Year
            Console.WriteLine(issue.IssueYear);
            Console.WriteLine();
            // Description
            string description = issue.Description == null ? "No description provided." : issue.Description;
            Console.WriteLine(StripHTML(description));
            Console.WriteLine();
            // URL
            Console.WriteLine(issue.SiteDetailUrl);
            ShowIssueOptions(issue);
        }

        private static void ShowIssueDetails(Models.Issue issue)
        {
            Console.Clear();
            WriteToConsole($"{issue.VolumeName}", false, ConsoleColor.DarkCyan);
            Console.WriteLine();
            Console.WriteLine();
            // Name
            string name = issue.Name == null ? "Untitled" : issue.Name;
            WriteToConsole($"Issue {issue.IssueNumber} - {name}", false, ConsoleColor.DarkGreen);
            Console.Write($" - id: {issue.Id}");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(issue.Read == true ? "Read" : "Not read");
            Console.WriteLine();
            // Start Year
            Console.WriteLine(issue.IssueYear);
            Console.WriteLine();
            // Description
            string description = issue.Description == null ? "No description provided." : issue.Description;
            Console.WriteLine(StripHTML(description));
            Console.WriteLine();
            // URL
            Console.WriteLine(issue.URL);
            ShowIssueOptions(issue);
        }

        private static void ShowIssueOptions(Models.Issue issue)
        {
            // Mark as read, Open in Vine
            List<ConsoleMenuItem> items = new List<ConsoleMenuItem>()
            {
                issue.Read == false ? new ConsoleMenuItem<Models.Issue>("Mark as read", MarkIssueRead, issue) : new ConsoleMenuItem<Models.Issue>("Mark as unread", MarkIssueUnRead, issue),
                new ConsoleMenuItem<string>("Open in ComicVine", OpeninVine, issue.URL),
                new ConsoleMenuSeperator(),
                new ConsoleMenuItem<string>("Exit", CallBackExitSearch, "")
            };

            var menu = new ConsoleMenu<string>("Options", items);
            menu.RunConsoleMenu();
        }

        private static void ShowIssueOptions(CVNetCore.Models.Issue issue)
        {
            // Mark as read, Open in Vine
            Console.WriteLine();
            Console.WriteLine("If you want to add this issue as 'read', you must first add the comic to your collection, then open the comic with `comictracker coll`.");
            Console.WriteLine();
            List<ConsoleMenuItem> items = new List<ConsoleMenuItem>();

            items.Add(new ConsoleMenuItem<string>("Open in ComicVine", OpeninVine, issue.SiteDetailUrl));
            items.Add(new ConsoleMenuSeperator());
            items.Add(new ConsoleMenuItem<string>("Exit", CallBackExitSearch, ""));

            var menu = new ConsoleMenu<string>("Options", items);
            menu.RunConsoleMenu();
        }

        private static void CallBackShowLessIssues(CVVolume volume)
        {
            IssuesPage--;
            ViewIssues(volume);
        }

        private static void CallBackShowMoreIssues(CVVolume volume)
        {
            IssuesPage++;
            ViewIssues(volume);
        }

        private static void CallBackShowLessIssues(Comic comic)
        {
            IssuesPage--;
            ViewIssues(comic);
        }

        private static void CallBackShowMoreIssues(Comic comic)
        {
            IssuesPage++;
            ViewIssues(comic);
        }

        private static void MarkIssueRead(Models.Issue issue)
        {
            Console.WriteLine("Marking comic as read...");
            issue.Read = true;
            Console.WriteLine("Comic read!");
            Serialize();
        }

        private static void MarkIssueUnRead(Models.Issue issue)
        {
            Console.WriteLine("Marking comic as unread...");
            issue.Read = false;
            Console.WriteLine("Comic unread!");
            Serialize();
        }
    }
}
