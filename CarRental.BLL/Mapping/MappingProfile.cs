using AutoMapper;
using CarRental.Domain.Entities;
using CarRental.BLL.DTOs.Car;
using CarRental.BLL.DTOs.User;
using CarRental.BLL.DTOs.Booking;
using CarRental.BLL.DTOs.Banner;
using CarRental.BLL.DTOs.Chat;
using CarRental.BLL.DTOs.Faq;
using CarRental.BLL.DTOs.Page;
using CarRental.BLL.DTOs.Document;

namespace CarRental.BLL.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ========== USER MAPPINGS ==========
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserProfileDto, User>()
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName));
        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName));
        CreateMap<RegisterDto, User>();

        // ========== CAR MAPPINGS ==========
        CreateMap<Car, CarDto>().ReverseMap();
        CreateMap<CarImage, CarImageDto>().ReverseMap();

        CreateMap<CarCreateDto, Car>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Bookings, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Единый маппинг для CarUpdateDto с новыми полями
        CreateMap<CarUpdateDto, Car>()
            .ForMember(dest => dest.PricePerDay15, opt => opt.MapFrom(src => src.PricePerDay15))
            .ForMember(dest => dest.PricePerDay30, opt => opt.MapFrom(src => src.PricePerDay30))
            .ForMember(dest => dest.Deposit, opt => opt.MapFrom(src => src.Deposit))
            .ForMember(dest => dest.MileageLimitPerDay, opt => opt.MapFrom(src => src.MileageLimitPerDay))
            .ForMember(dest => dest.OverMileagePricePerKm, opt => opt.MapFrom(src => src.OverMileagePricePerKm))
            .ForMember(dest => dest.UnlimitedMileagePrice, opt => opt.MapFrom(src => src.UnlimitedMileagePrice))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // ========== BOOKING MAPPINGS ==========
        CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.Car, opt => opt.MapFrom(src => src.Car))
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));
        CreateMap<BookingDto, Booking>()
            .ForMember(dest => dest.Car, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
        CreateMap<BookingRequestDto, Booking>();

        // ========== BANNER MAPPINGS ==========
        CreateMap<Banner, BannerDto>().ReverseMap();
        CreateMap<BannerCreateDto, Banner>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src =>
                src.StartDate.HasValue ? DateTime.SpecifyKind(src.StartDate.Value, DateTimeKind.Utc) : (DateTime?)null))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src =>
                src.EndDate.HasValue ? DateTime.SpecifyKind(src.EndDate.Value, DateTimeKind.Utc) : (DateTime?)null))
            .ForMember(dest => dest.VideoAutoplay, opt => opt.MapFrom(src => src.VideoAutoplay))
            .ForMember(dest => dest.VideoMuted, opt => opt.MapFrom(src => src.VideoMuted))
            .ForMember(dest => dest.VideoLoop, opt => opt.MapFrom(src => src.VideoLoop))
            .ForMember(dest => dest.VideoControls, opt => opt.MapFrom(src => src.VideoControls))
            .ForMember(dest => dest.ObjectFit, opt => opt.MapFrom(src => src.ObjectFit))
            .ForMember(dest => dest.ObjectPosition, opt => opt.MapFrom(src => src.ObjectPosition));

        CreateMap<BannerUpdateDto, Banner>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src =>
                src.StartDate.HasValue ? DateTime.SpecifyKind(src.StartDate.Value, DateTimeKind.Utc) : (DateTime?)null))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src =>
                src.EndDate.HasValue ? DateTime.SpecifyKind(src.EndDate.Value, DateTimeKind.Utc) : (DateTime?)null))
            .ForMember(dest => dest.VideoAutoplay, opt => opt.MapFrom(src => src.VideoAutoplay))
            .ForMember(dest => dest.VideoMuted, opt => opt.MapFrom(src => src.VideoMuted))
            .ForMember(dest => dest.VideoLoop, opt => opt.MapFrom(src => src.VideoLoop))
            .ForMember(dest => dest.VideoControls, opt => opt.MapFrom(src => src.VideoControls))
            .ForMember(dest => dest.ObjectFit, opt => opt.MapFrom(src => src.ObjectFit))
            .ForMember(dest => dest.ObjectPosition, opt => opt.MapFrom(src => src.ObjectPosition));

        // ========== CHAT MAPPINGS ==========
        CreateMap<Chat, ChatDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.GetDisplayName()))
            .ForMember(dest => dest.ContactInfo, opt => opt.MapFrom(src => src.GetContactInfo()));
        CreateMap<ChatMessage, ChatMessageDto>()
            .ForMember(dest => dest.SenderName, opt => opt.Ignore());
        CreateMap<ChatCreateDto, Chat>();
        CreateMap<ChatMessageCreateDto, ChatMessage>();

        // ========== FAQ ==========
        CreateMap<FaqItem, FaqDto>().ReverseMap();
        CreateMap<FaqCreateDto, FaqItem>();
        CreateMap<FaqUpdateDto, FaqItem>();

        // ========== PAGES ==========
        CreateMap<Page, PageDto>().ReverseMap();
        CreateMap<PageCreateDto, Page>();
        CreateMap<PageUpdateDto, Page>();

        // ========== DOCUMENTS ==========
        CreateMap<Document, DocumentDto>()
            .ForMember(dest => dest.FileUrl, opt => opt.Ignore())
            .ForMember(dest => dest.FileUrl2, opt => opt.Ignore())
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src =>
                src.User != null ? $"{src.User.FirstName} {src.User.LastName} {src.User.MiddleName}" : ""));
    }
}