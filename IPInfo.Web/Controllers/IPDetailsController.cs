using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace IPInfo.Web.Controllers
{
    [Route("api/ipdetails")]
    [ApiController]
    public class IPDetailsController : ControllerBase
    {
        const string IP_ADDRESS_PATTERN = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";

        IIPInfoProvider _ipInfoProvider;
        IIPInfoBatchUpdateProvider _batchUpdateProvider;

        public IPDetailsController(IIPInfoProvider ipInfoProvider, IIPInfoBatchUpdateProvider batchUpdateProvider)
        {
            _ipInfoProvider = ipInfoProvider ?? throw new ArgumentNullException(nameof(ipInfoProvider));
            _batchUpdateProvider = batchUpdateProvider ?? throw new ArgumentNullException(nameof(batchUpdateProvider));
        }

        [HttpGet("{ipAddress}")]
        [ProducesResponseType(typeof(IPDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDetails([RegularExpression(IP_ADDRESS_PATTERN)]string ipAddress)
        {
            var details = await _ipInfoProvider.GetDetails(ipAddress);

            if(!details.IsSuccess)
                return HandleError(details);

            return Ok(details.Value);
        }

        [HttpPut("update")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDetails([FromBody]string[] ipAddresses)
        {
            var validator = new RegularExpressionAttribute(IP_ADDRESS_PATTERN);

            if(!ipAddresses.All(ip => validator.IsValid(ip)))
                return HandleError(Result.Failure<Guid>(Error.BadRequest()));

            var details = await _batchUpdateProvider.UpdateDetails(ipAddresses);

            if (!details.IsSuccess)
                return HandleError(details);

            return Ok(details.Value);
        }

        [HttpGet("status/{operationId}")]
        [ProducesResponseType(typeof(BatchStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStatus(Guid operationId)
        {
            var details = await _batchUpdateProvider.GetStatus(operationId);

            if (!details.IsSuccess)
                return HandleError(details);

            return Ok(details.Value);
        }

        #region error handling

        protected virtual IActionResult HandleError<T>(Result<T> failure)
        { 
            switch(failure.Error.Code)
            { 
                case Error.EXCEPTION:
                     return Problem(statusCode: StatusCodes.Status500InternalServerError, detail: failure.Error.Message, type: Error.EXCEPTION);

                case Error.NOT_FOUND:
                     return NotFound();

                case Error.BAD_REQUEST:
                    return Problem(statusCode: StatusCodes.Status400BadRequest, detail: failure.Error.Message, type: Error.BAD_REQUEST);

                case Error.SERVICE_UNAVAILABLE:
                    // throw new IPServiceNotAvailableException(failure.Error.Code, failure.Error.Message);
                    return Problem(statusCode: StatusCodes.Status503ServiceUnavailable, detail: failure.Error.Message, type: Error.SERVICE_UNAVAILABLE);

                default:
                    return Problem(statusCode: StatusCodes.Status500InternalServerError, detail: failure.Error.Message, type: failure.Error.Code);
            }
        }

        #endregion
    }
}
