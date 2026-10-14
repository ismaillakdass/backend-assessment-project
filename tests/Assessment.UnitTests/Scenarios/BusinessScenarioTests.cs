using AutoMapper;
using Assessment.Application.DTOs.Donors;
using Assessment.Application.DTOs.Events;
using Assessment.Application.Services;
using Assessment.Domain.Entities;
using Assessment.Domain.Exceptions;
using Assessment.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Assessment.UnitTests.Scenarios;

public class BusinessScenarioTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;

    public BusinessScenarioTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task AssignVolunteerAsync_WhenEventCapacityIsFull_ShouldThrowBusinessException()
    {
        // Arrange
        int eventId = 1;
        int newVolunteerId = 99;

        var fullEvent = new Event
        {
            Id = eventId,
            Name = "LÖSEV Umut Koşusu",
            Capacity = 2,
            EventVolunteers = new List<EventVolunteer>
            {
                new() { EventId = eventId, VolunteerId = 10 },
                new() { EventId = eventId, VolunteerId = 11 }
            }
        };

        var activeVolunteer = new Volunteer
        {
            Id = newVolunteerId,
            FirstName = "Ali",
            LastName = "Kaya",
            IsActive = true
        };

        _unitOfWorkMock.Setup(u => u.Events.GetEventWithVolunteersAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fullEvent);

        _unitOfWorkMock.Setup(u => u.Volunteers.GetByIdAsync(newVolunteerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeVolunteer);

        var loggerMock = new Mock<ILogger<EventService>>();
        var eventService = new EventService(_unitOfWorkMock.Object, _mapperMock.Object, loggerMock.Object);

        // Act
        var act = () => eventService.AssignVolunteerAsync(eventId, newVolunteerId);

        // Assert
        var exception = await act.Should().ThrowAsync<BusinessException>();
        exception.Which.ErrorCode.Should().Be("EVENT_FULL");
    }

    [Fact]
    public async Task AssignVolunteerAsync_WhenVolunteerIsInactive_ShouldThrowBusinessException()
    {
        // Arrange
        int eventId = 1;
        int inactiveVolunteerId = 75;

        var availableEvent = new Event
        {
            Id = eventId,
            Name = "Gönüllü Kampı",
            Capacity = 10,
            EventVolunteers = new List<EventVolunteer>()
        };

        var inactiveVolunteer = new Volunteer
        {
            Id = inactiveVolunteerId,
            FirstName = "Kemal",
            LastName = "Demir",
            IsActive = false // INACTIVE
        };

        _unitOfWorkMock.Setup(u => u.Events.GetEventWithVolunteersAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availableEvent);

        _unitOfWorkMock.Setup(u => u.Volunteers.GetByIdAsync(inactiveVolunteerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveVolunteer);

        var loggerMock = new Mock<ILogger<EventService>>();
        var eventService = new EventService(_unitOfWorkMock.Object, _mapperMock.Object, loggerMock.Object);

        // Act
        var act = () => eventService.AssignVolunteerAsync(eventId, inactiveVolunteerId);

        // Assert
        var exception = await act.Should().ThrowAsync<BusinessException>();
        exception.Which.ErrorCode.Should().Be("INACTIVE_VOLUNTEER");
    }

    [Fact]
    public async Task CreateDonorAsync_WhenEmailAlreadyExists_ShouldThrowBusinessException()
    {
        // Arrange
        var dto = new CreateDonorDto
        {
            FirstName = "Ahmet",
            LastName = "Yılmaz",
            Email = "duplicate@bagisci.org",
            Phone = "+90 532 111 22 33"
        };

        _unitOfWorkMock.Setup(u => u.Donors.EmailExistsAsync(dto.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<DonorService>>();
        var donorService = new DonorService(_unitOfWorkMock.Object, _mapperMock.Object, loggerMock.Object);

        // Act
        var act = () => donorService.CreateAsync(dto);

        // Assert
        var exception = await act.Should().ThrowAsync<BusinessException>();
        exception.Which.ErrorCode.Should().Be("DUPLICATE_EMAIL");
    }
}
