using LogAnalyzerLibrary.Application;
using LogAnalyzerLibrary.Application.DTOs;
using LogAnalyzerLibrary.Integration.CloudinaryIntegration;
using Microsoft.AspNetCore.Mvc;

namespace LogAnalyzerLibrary.API.Controllers
{
    [ApiController]
    [Route("api/logsanalyzer/logs")]
    public class LogsController(ILogsService logsService, ICloudinaryService cloudinaryService) : ControllerBase
    {



        [HttpGet("total-logs")]
        public async Task<IActionResult> CountLogs([FromQuery] PeriodDTO model)
        {
            try
            {
                // Validate if the start date is earlier than the end date
                if (model.StartDate > model.EndDate)
                {
                    return BadRequest("Start date cannot be later than the end date.");
                }

                // Call the service method to count logs within the specified date range
                var result = await logsService.CountTotalLogsAsync(model);

                // Return the appropriate response based on the result
                if (result.Contains("No logs"))
                {
                    return NotFound(result); // Return a 404 if no logs are found
                }

                return Ok(result); // Return a 200 if logs are counted
            }
            catch (Exception ex)
            {
                // General exception handling in case something goes wrong in the controller
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("search/{model}")]
        public async Task<IActionResult> SearchLogs([FromRoute] DirectoryDTO model)
        {
            try
            {
                var logFiles = await logsService.SearchLogsAsync(model);

                if (logFiles == null || logFiles.Count == 0)
                {
                    return NotFound("No log files found in the provided directories.");
                }

                return Ok(new { logFiles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while searching for logs: {ex.Message}");
            }
        }

        [HttpGet("search-by-size")]
        public async Task<IActionResult> SearchLogsBySize([FromQuery] SizeRangeDTO model)
        {
            if (model.MinSizeKB <= 0 || model.MaxSizeKB <= 0 || model.MinSizeKB > model.MaxSizeKB)
            {
                return BadRequest("Invalid size range");
            }

            var logs = await logsService.SearchLogsBySizeAsync(model);

            if (logs.Count == 0)
            {
                return NotFound("No logs found in the specified size range.");
            }

            return Ok(logs);
        }

        [HttpDelete("{startDate}/{endDate}")]
        public async Task<IActionResult> DeleteLogs([FromRoute] DateTime startDate, [FromRoute] DateTime endDate, [FromBody] PeriodDTO model)
        {

            try
            {

                // Check if model is null
                if (model == null)
                {
                    return BadRequest("Invalid request data.");
                }

                if (startDate > endDate)
                {
                    return BadRequest("Start date cannot be later than the end date.");
                }

                model.StartDate = startDate;
                model.EndDate = endDate;

                // Call the service method to delete logs within the specified date range
                var result = await logsService.DeleteLogsAsync(model);

                // Return the appropriate response based on the result
                if (result.Contains("No logs"))
                {
                    return NotFound(result); // Return a 404 if no logs are found
                }

                return Ok(result); // Return a 200 if logs are successfully deleted
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here)
                return StatusCode(500, $"An error occurred while deleting logs. {ex.Message}");
            }


        }

        [HttpPost("unique-errors")]
        public async Task<IActionResult> CountUniqueErrors([FromBody] DirectoryDTO model)
        {
            if (model.DirectoryPath == null || !model.DirectoryPath.Any())
            {
                // Return a 400 Bad Request if the directory paths are empty or null
                return BadRequest("Directory paths cannot be null or empty.");
            }

            try
            {
                // Call the method from ILogsHandler to count unique errors in the provided directories
                var uniqueErrors = await logsService.CountUniqueErrorsAsync(model);

                // If no errors found, return a 404 Not Found
                if (uniqueErrors == null || uniqueErrors.Count == 0)
                {
                    return NotFound("No unique errors found in the specified directories.");
                }

                var formatedResponse = uniqueErrors.Select(x => new
                {
                    LogFile = x.Key,
                    UniqueErrorCount = x.Value,
                });

                // Return a 200 OK with the results
                return Ok(formatedResponse);
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error if an exception occurs
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("duplicate-errors")]
        public async Task<IActionResult> CountDuplicatedErrors([FromBody] DirectoryDTO model)
        {
            if (model.DirectoryPath == null || !model.DirectoryPath.Any())
            {
                // Return a 400 Bad Request if the directory paths are empty or null
                return BadRequest("Directory paths cannot be null or empty.");
            }

            try
            {
                var errorsCount = await logsService.CountDuplicatedErrorsAsync(model);
                if (errorsCount == null || errorsCount.Count == 0)
                {
                    return NotFound("No errors found in the specified directories.");
                }

                var formattedResponse = errorsCount.Select(x => new
                {
                    LogFile = x.Key,
                    DuplicatedErrorCount = x.Value
                });

                return Ok(formattedResponse);
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error if an exception occurs
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("upload-logs")]
        public async Task<IActionResult> UploadLogs([FromForm] List<IFormFile> files)
        {

            try
            {
                var uploadedUrls = await cloudinaryService.UploadFilesAsync(files);

                // Return the list of uploaded URLs
                return Ok(uploadedUrls);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error uploading files: {ex.Message}");
            }
        }


    }
}
