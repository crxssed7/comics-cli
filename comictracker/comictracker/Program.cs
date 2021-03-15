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
        public static ComicVine Service;

        private static int CurrentPage = 0;
        private static int IssuesPage = 0;
        private static int PageSize = 10;
        private static int CollectionPage = 0;
        
        private static string JSONLocation = "";
        private static string Query = "";

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
                        if (args.Length >= 2)
                        {
                            Login();
                            Search(args[1]);
                        }
                        else
                        {
                            WriteToConsole("You need to enter a comic to search for. Try again.", false, ConsoleColor.Red, Console.BackgroundColor);
                        }
                        break;
                    case "coll":
                        // Show Collection
                        ShowCollection("");
                        break;
                    case "s-coll":
                        // Search a collection
                        if (args.Length >= 2)
                        {
                            SearchCollection(args[1]);
                        }
                        else
                        {
                            WriteToConsole("You need to enter a comic to search for. Try again.", false, ConsoleColor.Red, Console.BackgroundColor);
                        }
                        break;
                    case "id":
                        if (args.Length >= 2)
                        {
                            try
                            {
                                Login();
                                int id = Convert.ToInt32(args[1]);
                                Id(id);
                            }
                            catch
                            {
                                WriteToConsole("There was an error retrieving the comic by ID. Please make sure your input is a number.", true, ConsoleColor.Red, Console.BackgroundColor);
                            }
                        }
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

        private static void Login()
        {
            Service = new ComicVine(ApiKey.Key);
        }

        private static void WriteToConsole(string text, bool returnLine, ConsoleColor forToColor = ConsoleColor.Gray, ConsoleColor backToColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = forToColor;
            Console.BackgroundColor = backToColor;

            if (returnLine)
                Console.WriteLine(text);
            else
                Console.Write(text);

            Console.ResetColor();
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
            WriteToConsole("Hello, I've come to help you!", true, ConsoleColor.Cyan, Console.BackgroundColor);
            Console.WriteLine();
            Console.WriteLine("Command          Arguments          Description");
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("help             N/A                Shows info on all available commands ");
            Console.WriteLine("search           comic-name         Searches for a comic ");
            Console.WriteLine("coll             N/A                Shows all the comics in your collection ");
            Console.WriteLine("s-coll           comic-name         Searches your collection ");
            Console.WriteLine("id               comicvine-id       Gets a specific comic based off of the give ComicVine id ");
        }

        private static void Search(string query)
        {
            Console.Clear();
            Query = query;
            WriteToConsole($"Searching for comics (", false, ConsoleColor.Cyan);
            WriteToConsole($"{query}", false, ConsoleColor.Blue);
            WriteToConsole($")...", false, ConsoleColor.Cyan);
            Console.WriteLine();
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

                items.Add(new ConsoleMenuItem<string>("... go to collection", ShowCollection, ""));
                items.Add(new ConsoleMenuItem<string>("... exit", CallBackExitSearch, query));

                var menu = new ConsoleMenu<string>("Your query returned these results", items);
                menu.RunConsoleMenu();
            }
            else
            {
                Console.WriteLine("No results for query... try searching again!");
            }
        }

        private static void ShowCollection(string tmp)
        {
            if (UserData.Comics.Count > 0)
            {
                ShowComicList(UserData.Comics);
            }
        }

        private static void SearchCollection(string query)
        {
            if (UserData.Comics.Count > 0)
            {
                var comics = UserData.Comics.Where(comic => comic.Name.ToLower().Contains(query.ToLower())).ToList();

                ShowComicList(comics);
            }
        }

        private static void ShowComicList(List<Comic> comicsToShow)
        {
            Console.Clear();
            List<ConsoleMenuItem> items = new List<ConsoleMenuItem>();

            int maximumPages = Convert.ToInt32(Math.Ceiling((double)comicsToShow.Count / (double)PageSize));

            if (CollectionPage > 0)
            {
                // Add the '... load less' button
                items.Add(new ConsoleMenuItem<List<Comic>>("... load less!", ShowLessCollection, comicsToShow));
                items.Add(new ConsoleMenuSeperator());
            }

            var comics = PaginateCollection(comicsToShow, CollectionPage, PageSize);
            for (int i = 0; i < comics.Count; i++)
            {
                items.Add(new ConsoleMenuItem<Comic>(comics[i].Name, ShowVolumeDetails, comics[i]));
            }

            items.Add(new ConsoleMenuSeperator());
            if (CollectionPage + 1 < maximumPages)
            {
                // Add load more button
                items.Add(new ConsoleMenuItem<List<Comic>>("... load more!", ShowMoreCollection, comicsToShow));
            }
            items.Add(new ConsoleMenuItem<string>("... sort by name", SortCollByName, ""));
            items.Add(new ConsoleMenuItem<string>("... sort by year", SortCollByYear, ""));
            items.Add(new ConsoleMenuItem<string>("... exit", CallBackExitSearch, ""));

            var menu = new ConsoleMenu<string>("Your current collection", items);
            menu.RunConsoleMenu();
        }

        private static void SortCollByName(string tmp)
        {
            UserData.Comics = UserData.Comics.OrderBy(comic => comic.Name).ToList();
            ShowCollection(tmp);
        }

        private static void SortCollByYear(string tmp)
        {
            UserData.Comics = UserData.Comics.OrderBy(comic => comic.StartYear).ToList();
            ShowCollection(tmp);
        }

        private static void ShowMoreCollection(List<Comic> comics)
        {
            CollectionPage++;
            ShowComicList(comics);
        }

        private static void ShowLessCollection(List<Comic> comics)
        {
            CollectionPage--;
            ShowComicList(comics);
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
            Console.WriteLine();
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
                    new ConsoleMenuItem<string>("... back to results", Search, Query),
                    new ConsoleMenuItem<string>("... go to collecion", ShowCollection, ""),
                    new ConsoleMenuItem<string>("... exit", CallBackExitSearch, "")
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
                    new ConsoleMenuItem<string>("... go to collecion", ShowCollection, ""),
                    new ConsoleMenuItem<string>("... exit", CallBackExitSearch, "")
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
                new ConsoleMenuItem<Comic>("... update issues", UpdateComics, comic),
                new ConsoleMenuItem<string>("... back to collection", ShowCollection, ""),
                new ConsoleMenuItem<string>("... exit", CallBackExitSearch, "")
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
                    VolumeName = comic.Name,
                    VolumeId = comic.Id
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
            
            int maximumPages = Convert.ToInt32(Math.Ceiling((double)response.Count / (double)PageSize));

            var issues = GetPage(response, IssuesPage, PageSize);
            
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
            items.Add(new ConsoleMenuItem<CVVolume>("... back to comic", ShowVolumeDetails, volume));

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
            int maximumPages = Convert.ToInt32(Math.Ceiling((double)comic.Issues.Count / (double)PageSize));

            var issues = GetPage(comic.Issues, IssuesPage, PageSize);

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

            items.Add(new ConsoleMenuItem<Comic>("... back to comic", ShowVolumeDetails, comic));
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

        private static IList<Comic> PaginateCollection(IList<Comic> list, int page, int pageSize)
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
                new ConsoleMenuItem<int>("... back to comic", GetComicByIdAndShow, issue.VolumeId),
                new ConsoleMenuItem<Comic>("... back to issues", ViewIssues, GetComicById(issue.VolumeId)),
                new ConsoleMenuItem<string>("... exit", CallBackExitSearch, "")
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
            items.Add(new ConsoleMenuItem<CVVolume>("... back to comic", ShowVolumeDetails, issue.Volume));
            items.Add(new ConsoleMenuItem<CVVolume>("... back to issues", ViewIssues, issue.Volume));
            items.Add(new ConsoleMenuItem<string>("... exit", CallBackExitSearch, ""));

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
            ShowIssueDetails(issue);
        }

        private static void MarkIssueUnRead(Models.Issue issue)
        {
            Console.WriteLine("Marking comic as unread...");
            issue.Read = false;
            Console.WriteLine("Comic unread!");
            Serialize();
            ShowIssueDetails(issue);
        }

        private static void GetComicByIdAndShow(int id)
        {
            ShowVolumeDetails(GetComicById(id));
        }

        private static Comic GetComicById(int id)
        {
            if (UserData.Comics.Count > 0)
            {
                var comic = UserData.Comics.Where(c => c.Id == id);
                if (comic.Count() > 0)
                {
                    return comic.First();
                }
            }

            return null;
        }

        public static void UpdateComics(Comic comic)
        {
            Login();
            Console.WriteLine("Updating...");
            var issues = Service.GetIssuesByVolume(comic.Id);
            Console.WriteLine(issues.Count);

            if (issues.Count > comic.Issues.Count)
            {
                Console.WriteLine("New issues found...");
                for (int i = comic.Issues.Count; i < issues.Count; i++)
                {
                    Console.WriteLine($"New issue {issues[i].IssueNumber}");
                    Models.Issue issue = new Models.Issue()
                    {
                        Id = issues[i].Id.Value,
                        Name = issues[i].Name,
                        Description = issues[i].Description,
                        IssueNumber = issues[i].IssueNumber,
                        IssueYear = issues[i].IssueYear,
                        URL = issues[i].SiteDetailUrl,
                        VolumeName = comic.Name,
                        VolumeId = comic.Id
                    };
                    comic.Issues.Add(issue);
                }
            }

            ShowVolumeDetails(comic);
        }

        private static void Id(int id)
        {
            try
            {
                var comic = Service.GetVolumeDetails(id);
                ShowVolumeDetails(comic);
            }
            catch (Exception e)
            {
                //WriteToConsole("Comic with that Id was not found.", true, ConsoleColor.Red, Console.BackgroundColor);
                WriteToConsole(e.Message, true, ConsoleColor.Red, Console.BackgroundColor);
            }
        }
    }
}
