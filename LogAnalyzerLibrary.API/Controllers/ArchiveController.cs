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
            if (request.DirectoryPaths == null || !request.DirectoryPaths.Any())
            {
                // Return a 400 Bad Request if the directory paths are empty or null
                return BadRequest("Directory paths cannot be null or empty.");
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

        [HttpDelete("{startDate}/{endDate}")]
        public async Task<IActionResult> DeleteArchives([FromRoute] DateTime startDate, [FromRoute] DateTime endDate, [FromBody] PeriodDTO model)
        {
            try
            {

                model.StartDate = startDate;
                model.EndDate = endDate;

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
