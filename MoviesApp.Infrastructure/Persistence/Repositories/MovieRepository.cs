using Microsoft.EntityFrameworkCore;
using MoviesApp.Domain.Interfaces;
using MoviesApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Infrastructure.Persistence.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly StarWarsDbContext _context;

        public MovieRepository(StarWarsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            return await _context.Movies
                .Where(m => m.DateTo == null)
                .ToListAsync();
        }

        public async Task<Movie?> GetByIdAsync(int id)
        {
            return await _context.Movies
                .Where(m => m.Id == id && m.DateTo == null)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Movie movie)
        {
            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Movie movie)
        {
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Movie>> GetByExternalIdsAsync(IEnumerable<string> externalIds)
        {
            return await _context.Movies
                .Where(m => externalIds.Contains(m.ExternalId))
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Movie> movies)
        {
            await _context.Movies.AddRangeAsync(movies);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
