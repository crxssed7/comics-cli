using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace comictracker.Models
{
    [Serializable]
    public class Comic
    {
        public string Name { get; set; } = "";

        public int Id { get; set; } = 0;

        public string StartYear { get; set; } = "";

        public string URL { get; set; } = "";

        public string Description { get; set; } = "";

        public List<Issue> Issues { get; set; } = new List<Issue>();
    }
}
