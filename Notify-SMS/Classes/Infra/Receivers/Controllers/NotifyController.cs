using Classes.Domain;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;

namespace Classes.Infra.Receivers.Controllers
{
    public class NotifyController : ControllerBase
    {
        private readonly INotifyService service;

        public NotifyController(INotifyService offerService)
        {
            this.service = offerService;
        }

        [HttpPost("api/send-notification")]
        public async Task<IActionResult> PostAsync([FromBody] RequestData requestData)
        {
            try
            {
                var response = await service.PostSMSNotificationAsync(requestData);

                if (response.Data is null) return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ServerErrorResponse(ex.Message));
            }
        }

        [HttpGet("api/get-notification/{notifyId}")]
        public async Task<IActionResult> GetByIdAsync(string notifyId)
        {
            try
            {
                var response = await service.GetSMSNotificationByIdAsync(notifyId);

                if (response.Data is null) return NotFound(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ServerErrorResponse(ex.Message));
            }
        }

        [HttpDelete("api/delete-notification/{notifyId}")]
        public async Task<IActionResult> DeleteAsync(string notifyId)
        {
            try
            {
                var response = await service.DeleteSMSNotificationAsync(notifyId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ServerErrorResponse(ex.Message));
            }
        }

        private static ResponseData ServerErrorResponse(string errorMessage)
        {
            return new ResponseData
            {
                Message = MessageErrors.SERVER_ERROR_MESSAGE,
                Errors = new[] { errorMessage }
            };
        }
    }

    internal static class MessageErrors
    {
        public const string SERVER_ERROR_MESSAGE = "Ocurrio un error en el servidor";
    }
}
