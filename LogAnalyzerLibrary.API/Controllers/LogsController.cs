﻿using LogAnalyzerLibrary.Application;
using LogAnalyzerLibrary.Application.DTOs;
using LogAnalyzerLibrary.Integration.CloudinaryIntegration;
using Microsoft.AspNetCore.Mvc;

namespace LogAnalyzerLibrary.API.Controllers
{
    [ApiController]
    [Route("api/logsanalyzer/logs")]
    public class LogsController(ILogsService logsService, ICloudinaryService cloudinaryService) : ControllerBase
    {


        /// <summary>
        /// Counts the total logs within a specified date range and directories.
        /// </summary>
        /// <param name="model">The period and directory list to filter the logs.</param>
        /// <returns>A response containing the total count of logs or a not found message.</returns>
        [HttpGet("total-logs")]
        public async Task<IActionResult> CountLogs([FromQuery] PeriodDirectoryListDTO model)
        {
            try
            {
                // Validate if the start date is earlier than the end date
                if (model.StartDate > model.EndDate)
                {
                    return BadRequest("Start date cannot be later than the end date.");
                }

                //trim model
                model.DirectoryCollection = model.DirectoryCollection
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x));

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

        /// <summary>
        /// Searches for logs by directory paths.
        /// </summary>
        /// <param name="model">The directory details to search for logs.</param>
        /// <returns>A response containing the found log files or a not found message.</returns>
        [HttpGet("search-by-dir")]
        public async Task<IActionResult> SearchLogs([FromQuery] DirectoryDTO model)
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

        /// <summary>
        /// Searches for logs by size range.
        /// </summary>
        /// <param name="model">The size range and directory details to search for logs.</param>
        /// <returns>A response containing the found logs or a not found message.</returns>
        [HttpGet("search-by-size")]
        public async Task<IActionResult> SearchLogsBySize([FromQuery] SizeRangeDTO model)
        {

            try
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
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while searching for logs: {ex.Message}");
            }



        }

        /// <summary>
        /// Deletes logs within a specified date range and directories.
        /// </summary>
        /// <param name="model">The period and directory list to filter the logs to delete.</param>
        /// <returns>A response indicating the success or failure of the deletion.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteLogs([FromQuery] PeriodDirectoryListDTO model)
        {

            try
            {

                // Check if model is null
                if (model == null)
                {
                    return BadRequest("Invalid request data.");
                }

                //trim model
                model.DirectoryCollection = model.DirectoryCollection
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x));

                if (model.StartDate > model.EndDate)
                {
                    return BadRequest("Start date cannot be later than the end date.");
                }


                // Call the service method to delete logs within the specified date range
                var result = await logsService.DeleteLogsAsync(model);

                // Return the appropriate response based on the result
                if (result.Contains("No logs"))
                {
                    return NotFound(result); // Return a 404 if no logs are found
                }

                return Ok(result); // Return a 200 if logs are successfully deleted
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here)
                return StatusCode(500, $"An error occurred while deleting logs. {ex.Message}");
            }
        }

        /// <summary>
        /// Counts unique errors found within specified directories.
        /// </summary>
        /// <param name="model">The directory details to count unique errors.</param>
        /// <returns>A response containing the count of unique errors or a not found message.</returns>
        [HttpGet("unique-errors")]
        public async Task<IActionResult> CountUniqueErrors([FromQuery] DirectoryDTO model)
        {

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

        /// <summary>
        /// Counts duplicated errors found within specified directories.
        /// </summary>
        /// <param name="model">The directory details to count duplicated errors.</param>
        /// <returns>A response containing the count of duplicated errors or a not found message.</returns>
        [HttpGet("duplicate-errors")]
        public async Task<IActionResult> CountDuplicatedErrors([FromQuery] DirectoryDTO model)
        {
            try
            {
                var errorsCount = await logsService.CountDuplicatedErrorsAsync(model);

                if (errorsCount == null || errorsCount.IsEmpty)
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

        /// <summary>
        /// Uploads log files to Cloudinary.
        /// </summary>
        /// <param name="files">The list of log files to be uploaded.</param>
        /// <returns>A response containing the uploaded file URLs.</returns>
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
                return StatusCode(500, $"Error uploading files: {ex.Message}");
            }
        }


    }
}
