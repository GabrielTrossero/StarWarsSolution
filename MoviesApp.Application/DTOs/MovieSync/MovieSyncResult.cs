using MoviesApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.DTOs.MovieSync
{
    public class MovieSyncResult
    {
        public MovieSyncStatus Status { get; set; }
        public Domain.Entities.Movie ExternalMovie { get; set; }
        public Domain.Entities.Movie LocalMovie { get; set; }
    }

    public enum MovieSyncStatus
    {
        NotAdded = 0,
        Added = 1,
        Deleted = 2,
        UpdatedLocal = 3,
        UpdatedExternal = 4
    }
}
