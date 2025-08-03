using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextPassAPI.Services.Interfaces;
using NextPassAPI.Data.Models.Responses;
using NextPassAPI.Data.Enums;
using Microsoft.AspNetCore.RateLimiting;

namespace NextPassAPI.Controllers
{
    [Route("Notifications")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("GeneralPolicy")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
                var response = new ApiResponse<object>(true, "Notifications retrieved successfully", notifications);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while retrieving notifications", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
                var response = new ApiResponse<object>(true, "Unread notifications retrieved successfully", notifications);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while retrieving unread notifications", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("count/unread")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var count = await _notificationService.GetUnreadCountAsync(userId);
                var response = new ApiResponse<object>(true, "Unread count retrieved successfully", new { count });
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while retrieving unread count", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var success = await _notificationService.MarkAsReadAsync(notificationId, userId);
                if (!success)
                {
                    var errorResponse = new ApiResponse<bool>(false, "Failed to mark notification as read", false);
                    return BadRequest(errorResponse);
                }

                var response = new ApiResponse<bool>(true, "Notification marked as read", true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while marking notification as read", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var success = await _notificationService.MarkAllAsReadAsync(userId);
                var response = new ApiResponse<bool>(true, "All notifications marked as read", success);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while marking all notifications as read", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("access-history")]
        public async Task<IActionResult> GetAccessHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var accessHistory = await _notificationService.GetUserAccessHistoryAsync(userId, page, pageSize);
                var response = new ApiResponse<object>(true, "Access history retrieved successfully", accessHistory);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while retrieving access history", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("access-history/{credentialId}")]
        public async Task<IActionResult> GetCredentialAccessHistory(string credentialId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var accessHistory = await _notificationService.GetPasswordAccessHistoryAsync(credentialId, page, pageSize);
                var response = new ApiResponse<object>(true, "Credential access history retrieved successfully", accessHistory);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while retrieving credential access history", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("security-alerts")]
        public async Task<IActionResult> GetSecurityAlerts([FromQuery] AlertSeverity? severity = null)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var alerts = await _notificationService.GetSecurityAlertsAsync(userId, severity);
                var response = new ApiResponse<object>(true, "Security alerts retrieved successfully", alerts);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while retrieving security alerts", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetNotificationStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var to = toDate ?? DateTime.UtcNow;

                var stats = await _notificationService.GetNotificationStatsAsync(userId, from, to);
                var response = new ApiResponse<object>(true, "Notification stats retrieved successfully", stats);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while retrieving notification stats", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("recent-accesses")]
        public async Task<IActionResult> GetRecentAccesses([FromQuery] int count = 10)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var recentAccesses = await _notificationService.GetRecentAccessesAsync(userId, count);
                var response = new ApiResponse<object>(true, "Recent accesses retrieved successfully", recentAccesses);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(false, "An error occurred while retrieving recent accesses", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}
