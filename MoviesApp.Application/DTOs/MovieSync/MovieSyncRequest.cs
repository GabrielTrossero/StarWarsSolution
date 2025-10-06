using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.DTOs.MovieSync
{
    public class MovieSyncRequest
    {
        public bool NotAdded { get; set; } = false;
        public bool Added { get; set; } = false;
        public bool Deleted { get; set; } = false;
        public bool UpdatedLocal { get; set; } = false;
        public bool UpdatedExternal { get; set; } = false;
    }
}
