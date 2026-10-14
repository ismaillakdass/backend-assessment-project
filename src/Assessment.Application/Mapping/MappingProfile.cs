using AutoMapper;
using Assessment.Application.DTOs.Donations;
using Assessment.Application.DTOs.Donors;
using Assessment.Application.DTOs.Events;
using Assessment.Domain.Entities;

namespace Assessment.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Donor mappings
        CreateMap<Donor, DonorDto>();
        CreateMap<CreateDonorDto, Donor>();
        CreateMap<UpdateDonorDto, Donor>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Donation mappings
        CreateMap<Donation, DonationDto>()
            .ForMember(dest => dest.DonorName, opt => opt.MapFrom(src => src.Donor != null ? $"{src.Donor.FirstName} {src.Donor.LastName}".Trim() : string.Empty))
            .ForMember(dest => dest.DonorEmail, opt => opt.MapFrom(src => src.Donor != null ? src.Donor.Email : string.Empty));
        CreateMap<CreateDonationDto, Donation>();

        // Event mappings
        CreateMap<Event, EventDto>()
            .ForMember(dest => dest.RegisteredVolunteersCount, opt => opt.MapFrom(src => src.EventVolunteers.Count));
        CreateMap<CreateEventDto, Event>();

        // TODO [Task 12]: Volunteer mapping is missing here!
        // Candidates should add Volunteer <-> VolunteerDto mapping profile configuration.
    }
}
