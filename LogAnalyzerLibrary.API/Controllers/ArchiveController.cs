using LogAnalyzerLibrary.Application;
using LogAnalyzerLibrary.Application.ArchiveService;
using LogAnalyzerLibrary.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LogAnalyzerLibrary.API.Controllers
{
    [ApiController]
    [Route("api/logsanalyzer/archive/")]
    public class ArchiveController(IArchiveService archiveService) : ControllerBase
    {

        /// <summary>
        /// Archives logs based on the provided period and directory path.
        /// </summary>
        /// <param name="request">The period and directory information to filter logs for archiving.</param>
        /// <returns>A response with a success message or error details.</returns>
        /// <response code="200">Logs successfully archived.</response>
        /// <response code="400">Bad request if directory path is empty or invalid dates are provided.</response>
        /// <response code="500">Internal server error if an exception occurs during archiving.</response>
        [HttpPost]
        public async Task<IActionResult> ArchiveLogs([FromBody] PeriodDTO request)
        {
            try
            {
                if (request.DirectoryPath == null || !request.DirectoryPath.Any())
                {
                    // Return a 400 Bad Request if the directory paths are empty or null
                    return BadRequest("Directory paths cannot be null or empty.");
                }

                if (request.StartDate > request.EndDate)
                {
                    return BadRequest("Start date cannot be later than the end date.");
                }


                List<string> result = await archiveService.ArchiveLogsAsync(request);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while archiving logs.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes log archives based on the provided period.
        /// </summary>
        /// <param name="model">The period to filter logs for deletion.</param>
        /// <returns>A response with a success message or error details.</returns>
        /// <response code="200">Archives successfully deleted.</response>
        /// <response code="400">Bad request if invalid dates are provided.</response>
        /// <response code="404">Not found if the archive file is not found.</response>
        /// <response code="500">Internal server error if an exception occurs during deletion.</response>
        [HttpDelete]
        public async Task<IActionResult> DeleteArchives([FromQuery] PeriodDTO model)
        {
            try
            {
                if (model.StartDate > model.EndDate)
                {
                    return BadRequest("Start date cannot be later than the end date.");
                }

                string result = await archiveService.DeleteArchiveAsync(model);
                return Ok(result);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting archives. {ex.Message}");
            }
        }

    }
}
