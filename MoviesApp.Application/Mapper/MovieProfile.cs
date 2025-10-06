using AutoMapper;
using MoviesApp.Application.DTOs;
using MoviesApp.Application.DTOs.Movie;
using MoviesApp.Application.DTOs.MovieExternal;
using MoviesApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.Mapper
{
    public class MovieProfile : Profile
    {
        public MovieProfile()
        {
            CreateMap<Movie, Movie>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalId, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.Edited, opt => opt.Ignore())
                .ForMember(dest => dest.DateTo, opt => opt.Ignore());

            CreateMap<MovieRequestDto, Movie>();

            CreateMap<SwapiFilmResult, Movie>()
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.Uid))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Properties.Title))
                .ForMember(dest => dest.Director, opt => opt.MapFrom(src => src.Properties.Director))
                .ForMember(dest => dest.Producer, opt => opt.MapFrom(src => src.Properties.Producer))
                .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => DateTime.Parse(src.Properties.Release_date)))
                .ForMember(dest => dest.Edited, opt => opt.MapFrom(src => DateTime.Parse(src.Properties.Edited)))
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => DateTime.Parse(src.Properties.Created)))
                .ForMember(dest => dest.EpisodeId, opt => opt.MapFrom(src => src.Properties.Episode_id))
                .ForMember(dest => dest.OpeningCrawl, opt => opt.MapFrom(src => src.Properties.Opening_crawl))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Properties.Url));
        }
    }
}
