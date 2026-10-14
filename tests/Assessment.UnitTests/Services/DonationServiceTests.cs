using AutoMapper;
using Assessment.Application.DTOs.Donations;
using Assessment.Application.Services;
using Assessment.Domain.Entities;
using Assessment.Domain.Enums;
using Assessment.Domain.Exceptions;
using Assessment.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Assessment.UnitTests.Services;

/// <summary>
/// TASK 6: Unit Testing (10 Points)
/// 
/// This test suite contains starter test setup for DonationService, but is severely missing key tests.
/// Candidates are expected to implement comprehensive unit tests covering:
/// 1. Happy path (Success): Creating a donation successfully updates the donor's total amount and creates an audit log.
/// 2. Not found scenario: Creating a donation for a non-existent DonorId throws NotFoundException.
/// 3. Validation / Business rule failure: Creating a donation for an INACTIVE donor throws BusinessException.
/// 4. Edge case / Invalid state: Approving a donation that is NOT in Pending status throws BusinessException.
/// </summary>
public class DonationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<DonationService>> _loggerMock;
    private readonly DonationService _donationService;

    public DonationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<DonationService>>();

        _donationService = new DonationService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDonationExists_ReturnsMappedDonationDto()
    {
        // Arrange
        int donationId = 1;
        var donation = new Donation
        {
            Id = donationId,
            DonorId = 10,
            Amount = 500m,
            Status = DonationStatus.Approved,
            DonationDate = DateTime.UtcNow
        };
        var expectedDto = new DonationDto
        {
            Id = donationId,
            DonorId = 10,
            Amount = 500m,
            Status = DonationStatus.Approved
        };

        _unitOfWorkMock.Setup(u => u.Donations.GetDonationWithDonorAsync(donationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(donation);

        _mapperMock.Setup(m => m.Map<DonationDto>(donation))
            .Returns(expectedDto);

        // Act
        var result = await _donationService.GetByIdAsync(donationId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(donationId);
        result.Amount.Should().Be(500m);
        _unitOfWorkMock.Verify(u => u.Donations.GetDonationWithDonorAsync(donationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDonationDoesNotExist_ReturnsNull()
    {
        // Arrange
        int nonExistentId = 999;
        _unitOfWorkMock.Setup(u => u.Donations.GetDonationWithDonorAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Donation?)null);

        // Act
        var result = await _donationService.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================================
    // TODO [Task 6]: Implement the missing unit tests below to complete Task 6.
    // =========================================================================

    [Fact]
    public async Task CreateDonationAsync_WithValidData_ShouldSucceedAndCalculateDonorTotal()
    {
        // TODO [Task 6]: Write test verifying valid donation creation:
        // - Mocks donor retrieval returning an active donor
        // - Mocks Donation addition and Donor update
        // - Asserts returned DTO and verifies SaveChangesAsync was called
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreateDonationAsync_WhenDonorNotFound_ShouldThrowNotFoundException()
    {
        // TODO [Task 6]: Write test asserting that when Donor does not exist in repository,
        // calling CreateDonationAsync throws NotFoundException.
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreateDonationAsync_WhenDonorIsInactive_ShouldThrowBusinessException()
    {
        // TODO [Task 6]: Write test asserting that when Donor.IsActive is false,
        // calling CreateDonationAsync throws BusinessException with ErrorCode "INACTIVE_DONOR".
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ApproveDonationAsync_WhenDonationIsNotPending_ShouldThrowBusinessException()
    {
        // TODO [Task 6]: Write test asserting that attempting to approve a donation
        // whose status is already Approved or Cancelled throws BusinessException with ErrorCode "INVALID_STATUS".
        await Task.CompletedTask;
    }
}
