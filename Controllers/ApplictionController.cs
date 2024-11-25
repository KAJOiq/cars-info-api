using ApiAppPay.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiAppPay.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class ApplictionController : ControllerBase
    {
        private readonly AppService _AppService;

        public ApplictionController(AppService AppService)
        {
            _AppService = AppService;
        }

        [HttpGet("{appid}")]
        [Authorize]
        public async Task<IActionResult> GetAppById(long appid)
        {
            var App = await _AppService.GetAppByIdAsync(appid);

            if (App == null)
            {
                var errorResponse = new ApiResponse<object>(false, null, new List<string> { "Appliction not found" });
                return NotFound(errorResponse);
            }

            var successResponse = new ApiResponse<object>(true, App);
            return Ok(successResponse);
        }
    }
}
