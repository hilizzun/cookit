using CookIt.Application.Services;
using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly IUserStatisticsService _userStatisticsService;
    private readonly IAchievementService _achievementService;

    public StatisticsController(
        IUserStatisticsService userStatisticsService,
        IAchievementService achievementService)
    {
        _userStatisticsService = userStatisticsService;
        _achievementService = achievementService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserAchievementsDto>> GetMyAchievements()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var stats = await _userStatisticsService.GetUserStatisticsAsync(userId);
        var achievements = _achievementService.CalculateAchievements(stats);
        return Ok(new UserAchievementsDto { Achievements = achievements });
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<UserAchievementsDto>> GetUserAchievements(string userId)
    {
        var stats = await _userStatisticsService.GetUserStatisticsAsync(userId);
        var achievements = _achievementService.CalculateAchievements(stats);
        return Ok(new UserAchievementsDto { Achievements = achievements });
    }
}