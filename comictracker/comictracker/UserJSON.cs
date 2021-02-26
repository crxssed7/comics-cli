using comictracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace comictracker
{
    [Serializable]
    public class UserJSON
    {
        public List<Comic> Comics { get; set; } = new List<Comic>();
    }
}
