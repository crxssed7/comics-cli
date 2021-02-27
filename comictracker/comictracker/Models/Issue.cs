using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace comictracker.Models
{
    public class Issue
    {
        public float? IssueNumber { get; set; }

        public string Name { get; set; } = "";

        public int IssueYear { get; set; } = 0;

        public int Id { get; set; } = 0;

        public string Description { get; set; } = "";

        public string URL { get; set; } = "";

        public bool Read { get; set; } = false;

        public string VolumeName { get; set; } = "";
    }
}
