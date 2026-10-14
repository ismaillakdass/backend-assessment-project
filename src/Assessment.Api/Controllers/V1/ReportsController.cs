using Asp.Versioning;
using Assessment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Assessment.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reports")]
public class ReportsController : ControllerBase
{
    private readonly IDonorReportService _donorReportService;

    public ReportsController(IDonorReportService donorReportService)
    {
        _donorReportService = donorReportService;
    }

    /// <summary>
    /// TASK 2: EF Core Performance Problem (20 Points)
    /// This endpoint calls DonorReportService, which suffers from severe performance bottlenecks:
    /// - Full entity tracking
    /// - Early ToListAsync materialization
    /// - In-memory filtering instead of SQL WHERE
    /// - N+1 database queries
    /// Candidates should optimize the underlying query.
    /// </summary>
    [HttpGet("donor-summary")]
    public async Task<IActionResult> GetDonorSummary(
        [FromQuery] decimal? minTotalAmount,
        [FromQuery] int? minDonationsCount,
        CancellationToken cancellationToken)
    {
        var report = await _donorReportService.GetDonorSummaryReportAsync(
            minTotalAmount,
            minDonationsCount,
            cancellationToken);

        return Ok(report);
    }
}
