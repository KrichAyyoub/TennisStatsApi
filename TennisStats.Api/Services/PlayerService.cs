using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TennisStats.Api.DTOs;
using TennisStats.Api.Exceptions;
using TennisStats.Api.Models;
using TennisStats.Api.Repositories;

namespace TennisStats.Api.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _repository;

        public PlayerService(IPlayerRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PlayerResponseDto>> GetPlayersAsync()
        {
            var players = await _repository.GetAllAsync();

            // Sort by points descending
            return players
                .OrderByDescending(p => p.Data.Points)
                .Select(MapToDto);
        }

        public async Task<PlayerResponseDto> GetPlayerByIdAsync(int id)
        {
            var player = await _repository.GetByIdAsync(id);
            if (player == null)
            {
                throw new PlayerNotFoundException(id);
            }
            return MapToDto(player);
        }

        public async Task<PlayerResponseDto> CreatePlayerAsync(CreatePlayerRequestDto request)
        {
            if (request == null)
            {
                throw new ValidationException("Request body cannot be null.");
            }

            var player = new Player
            {
                Firstname = request.Firstname,
                Lastname = request.Lastname,
                Shortname = request.Shortname,
                Sex = request.Sex.Trim().ToUpperInvariant(),
                Picture = request.Picture,
                Country = new CountryInfo
                {
                    Code = request.CountryCode.Trim().ToUpperInvariant(),
                    Picture = request.CountryPicture
                },
                Data = new PlayerData
                {
                    Rank = request.Rank,
                    Points = request.Points,
                    Weight = request.Weight,
                    Height = request.Height,
                    Age = request.Age,
                    Last = request.Last ?? new List<int>()
                }
            };

            var createdPlayer = await _repository.AddAsync(player);
            return MapToDto(createdPlayer);
        }



        public async Task<StatsResponseDto> GetStatsAsync()
        {
            var players = (await _repository.GetAllAsync()).ToList();

            if (players.Count == 0)
            {
                return new StatsResponseDto
                {
                    CountryWithBestWinRatio = new CountryWinRatioDto { CountryCode = string.Empty, WinRatio = 0.0 },
                    AverageBMI = 0.0,
                    MedianHeight = 0.0
                };
            }

            // 1. Country with best win ratio
            // Group players by country code (using upper invariant to ensure consistency)
            var countryGroups = players
                .Where(p => p.Country != null && !string.IsNullOrWhiteSpace(p.Country.Code))
                .GroupBy(p => p.Country.Code.Trim().ToUpperInvariant());

            var bestCountry = countryGroups
                .Select(g =>
                {
                    var allMatches = g.SelectMany(p => p.Data?.Last ?? Enumerable.Empty<int>()).ToList();
                    double totalMatches = allMatches.Count;
                    double winRatio = 0.0;
                    if (totalMatches > 0)
                    {
                        double wins = allMatches.Count(m => m == 1);
                        winRatio = wins / totalMatches;
                    }
                    return new CountryWinRatioDto
                    {
                        CountryCode = g.Key,
                        WinRatio = Math.Round(winRatio, 2)
                    };
                })
                .OrderByDescending(c => c.WinRatio)
                .ThenBy(c => c.CountryCode, StringComparer.Ordinal) // Tie-breaker: alphabetical order
                .FirstOrDefault() ?? new CountryWinRatioDto { CountryCode = string.Empty, WinRatio = 0.0 };

            // 2. Average BMI
            // Formule: weight (kg) / height (m)^2
            // Weight in grams -> kg = weight / 1000.0
            // Height in cm -> m = height / 100.0
            var bmis = players
                .Where(p => p.Data != null && p.Data.Height > 0)
                .Select(p =>
                {
                    double weightKg = p.Data.Weight / 1000.0;
                    double heightM = p.Data.Height / 100.0;
                    return weightKg / (heightM * heightM);
                })
                .ToList();

            double averageBMI = bmis.Count > 0 ? Math.Round(bmis.Average(), 2) : 0.0;

            // 3. Median Height
            var heights = players
                .Where(p => p.Data != null && p.Data.Height > 0)
                .Select(p => (double)p.Data.Height)
                .OrderBy(h => h)
                .ToList();

            double medianHeight = 0.0;
            if (heights.Count > 0)
            {
                int count = heights.Count;
                if (count % 2 == 1)
                {
                    medianHeight = heights[count / 2];
                }
                else
                {
                    medianHeight = (heights[(count / 2) - 1] + heights[count / 2]) / 2.0;
                }
            }

            return new StatsResponseDto
            {
                CountryWithBestWinRatio = bestCountry,
                AverageBMI = averageBMI,
                MedianHeight = medianHeight
            };
        }

        private static PlayerResponseDto MapToDto(Player player)
        {
            return new PlayerResponseDto
            {
                Id = player.Id,
                Firstname = player.Firstname,
                Lastname = player.Lastname,
                Shortname = player.Shortname,
                Sex = player.Sex,
                Picture = player.Picture,
                Country = player.Country != null ? new CountryDto
                {
                    Code = player.Country.Code,
                    Picture = player.Country.Picture
                } : new CountryDto(),
                Data = player.Data != null ? new PlayerDataDto
                {
                    Rank = player.Data.Rank,
                    Points = player.Data.Points,
                    Weight = player.Data.Weight,
                    Height = player.Data.Height,
                    Age = player.Data.Age,
                    Last = player.Data.Last?.ToList() ?? new List<int>()
                } : new PlayerDataDto()
            };
        }
    }
}
