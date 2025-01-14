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

        [HttpPost]
        public async Task<IActionResult> ArchiveLogs([FromBody] PeriodDTO request)
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


            try
            {
                List<string> result = await archiveService.ArchiveLogsAsync(request);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while archiving logs.", error = ex.Message });
            }
        }

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
                // Log the exception (not shown here)
                return StatusCode(500, $"An error occurred while deleting archives. {ex.Message}");
            }
        }

    }
}
