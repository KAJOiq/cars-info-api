using ApiAppPay.Models;
using ApiAppPay.Models.DTOs;
using ApiAppPay.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiAppPay.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class ApplictionController : BaseController
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
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Application not found."));
            }

            ApplicationDTO applicationDTO = new ApplicationDTO
            {
                GivenName=App.GivenName,
                FatherName = App.FatherName,
                GrandfatherName = App.GrandfatherName,
                MotherName = App.MotherName,
                MotherFatherName = App.MotherFatherName,
                UseCase = App.UseCase,
                LicenseNumber = App.LicenseNumber,
                LicenseNumberLatin = App.LicenseNumberLatin,
                Governorate = App.Governorate,
                Usage = App.Usage,
                Passengers = App.Passengers,
                VehicleCategory = App.VehicleCategory,
                Cylinders = App.Cylinders,
                Axis = App.Axis,
                CabinType = App.CabinType,
                LoadWeight = App.LoadWeight,
                DateOfIssue = App.DateOfIssue,
                DateOfExpiry = App.DateOfExpiry,
       
                DlCategory=App.DlCategory,
                IdCurrentState=App.IdCurrentState
            };
            return Ok(CreateSuccessResponse(applicationDTO));
        }
    }
}
