using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Booking;
using CarRental.Domain.Interfaces.Repositories;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CarRental.BLL.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICarRepository _carRepository;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;
    private readonly IEmailService _emailService;

    public BookingService(
        IBookingRepository bookingRepository,
        ICarRepository carRepository,
        UserManager<User> userManager,
        IMapper mapper,
        ILogger<BookingService> logger,
        IEmailService emailService)
    {
        _bookingRepository = bookingRepository;
        _carRepository = carRepository;
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<BookingDto> CreateBookingAsync(BookingRequestDto requestDto)
    {
        try
        {
            _logger.LogInformation("Creating booking for user: {UserId}, car: {CarId}", 
                requestDto.UserId, requestDto.CarId);

            // 1. Проверяем минимальный срок аренды (1 день )
            var rentalDays = (requestDto.EndDate - requestDto.StartDate).Days;
            if (rentalDays < 1)
                throw new InvalidOperationException("Минимальный срок аренды - от 1 дня");

            // 2. Проверяем пользователя
            var user = await _userManager.FindByIdAsync(requestDto.UserId.ToString());
            if (user == null)
                throw new InvalidOperationException("Пользователь не найден");

            // 3. Проверяем статус пользователя (должен быть активен)
            if (user.Status != UserStatus.Active)
                throw new InvalidOperationException("Ваш аккаунт не активирован");

            // 4. Проверяем email подтверждение
            if (!user.EmailConfirmed)
                throw new InvalidOperationException("Подтвердите ваш email перед бронированием");

            // 5. Проверяем автомобиль
            var car = await _carRepository.GetByIdAsync(requestDto.CarId);
            if (car == null)
                throw new InvalidOperationException("Автомобиль не найден");

            // 6. Проверяем доступность автомобиля
            var isAvailable = await _carRepository.IsCarAvailableAsync(
                requestDto.CarId, requestDto.StartDate, requestDto.EndDate);
            
            if (!isAvailable)
                throw new InvalidOperationException("Автомобиль недоступен на выбранные даты");

            // 7. Проверяем активные бронирования
            var hasActiveBooking = await _bookingRepository.HasActiveBookingAsync(requestDto.UserId);
            if (hasActiveBooking)
                throw new InvalidOperationException("У вас уже есть активное бронирование");

            // 8. Создаем бронирование
            var booking = new Booking
            {
                CarId = requestDto.CarId,
                UserId = requestDto.UserId,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.EndDate,
                TotalPrice = requestDto.TotalPrice,
                Notes = requestDto.Notes,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking);
            var saved = await _bookingRepository.SaveChangesAsync();

            if (!saved)
                throw new InvalidOperationException("Не удалось сохранить бронирование");

            _logger.LogInformation("Booking created successfully: {BookingId}", booking.Id);
            
            var createdBooking = await _bookingRepository.GetByIdAsync(booking.Id);
            if (createdBooking != null)
            {
                var carForNotify = await _carRepository.GetByIdAsync(createdBooking.CarId);
                if (carForNotify != null && createdBooking.User != null)
                {
                    await SendBookingNotificationsAsync(createdBooking, createdBooking.User, carForNotify, "CREATED");
                }
            }
            return _mapper.Map<BookingDto>(createdBooking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for user {UserId}", requestDto.UserId);
            throw;
        }
    }

    public async Task<bool> CancelBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null) return false;

        if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
            return false;

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        _bookingRepository.Update(booking);
        var result = await _bookingRepository.SaveChangesAsync();

        if (result && booking.UserId.HasValue)  // ← исправлено: проверка перед использованием .Value
        {
            // Отправляем уведомления об отмене
            var user = await _userManager.FindByIdAsync(booking.UserId.Value.ToString()!);
            var car = await _carRepository.GetByIdAsync(booking.CarId);
            
            if (user != null && car != null)
            {
                await SendBookingNotificationsAsync(booking, user, car, "CANCELLED");
                await _emailService.SendBookingCancellationAsync(booking, user);
            }
        }

        return result;
    }

    public async Task<bool> ConfirmBookingAsync(Guid bookingId)
    {
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return false;

            if (booking.Status != BookingStatus.Pending)
                return false;

            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;

            var car = await _carRepository.GetByIdAsync(booking.CarId);
            if (car != null)
            {
                car.IsAvailable = false;
                car.UpdatedAt = DateTime.UtcNow;
                _carRepository.Update(car);
            }

            _bookingRepository.Update(booking);
            var result = await _bookingRepository.SaveChangesAsync();
            
            if (result && booking.UserId.HasValue)  // ← исправлено: проверка перед использованием .Value
            {
                var user = await _userManager.FindByIdAsync(booking.UserId.Value.ToString()!);
                if (user != null && car != null)
                {
                    await SendBookingNotificationsAsync(booking, user, car, "CONFIRMED");
                    await _emailService.SendBookingConfirmationAsync(booking, user);
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
            throw;
        }
    }

    private async Task SendBookingNotificationsAsync(Booking booking, User user, Car car, string eventType)
    {
        try
        {
            // Email менеджеру
            var subject = eventType switch
            {
                "CREATED" => "Новое бронирование",
                "CONFIRMED" => "Бронирование подтверждено",
                "CANCELLED" => "Бронирование отменено",
                _ => "Уведомление о бронировании"
            };

            var message = $@"
                <p><strong>Событие:</strong> {subject}</p>
                <p><strong>ID бронирования:</strong> {booking.Id}</p>
                <p><strong>Пользователь:</strong> {user.FirstName} {user.LastName}</p>
                <p><strong>Email:</strong> {user.Email}</p>
                <p><strong>Телефон:</strong> {user.PhoneNumber}</p>
                <p><strong>Автомобиль:</strong> {car.Brand} {car.Model}</p>
                <p><strong>Период:</strong> {booking.StartDate:dd.MM.yyyy} - {booking.EndDate:dd.MM.yyyy}</p>
                <p><strong>Стоимость:</strong> {booking.TotalPrice}₽</p>
                <p><strong>Время события:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>";

            await _emailService.SendManagerNotificationAsync(subject, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending booking notifications for {BookingId}", booking.Id);
        }
    }

    public async Task<BookingCalculationDto> CalculateBookingPriceAsync(BookingRequestDto requestDto)
    {
        var car = await _carRepository.GetByIdAsync(requestDto.CarId);
        if (car == null)
            throw new InvalidOperationException("Автомобиль не найден");

        var days = (requestDto.EndDate - requestDto.StartDate).Days;
        if (days < 1) days = 1; // Минимум 1 дн по ТЗ

        var totalPrice = car.PricePerDay * days;

        return new BookingCalculationDto
        {
            CarId = requestDto.CarId,
            StartDate = requestDto.StartDate,
            EndDate = requestDto.EndDate,
            Days = days,
            PricePerDay = car.PricePerDay,
            TotalPrice = totalPrice
        };
    }

    public async Task<BookingDto?> GetBookingByIdAsync(Guid id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        return _mapper.Map<BookingDto?>(booking);
    }

    public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(Guid userId)
    {
        var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<bool> HasActiveBookingAsync(Guid userId)
    {
        return await _bookingRepository.HasActiveBookingAsync(userId);
    }

    public async Task<IEnumerable<BookingDto>> GetRecentBookingsAsync(int count = 5)
    {
        var bookings = await _bookingRepository.GetRecentBookingsAsync(count);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<IEnumerable<BookingDto>> GetBookingsByStatusAsync(BookingStatus status)
    {
        var bookings = await _bookingRepository.GetBookingsByStatusAsync(status);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<IEnumerable<BookingDto>> GetBookingsForCarAsync(Guid carId)
    {
        var bookings = await _bookingRepository.GetBookingsForCarAsync(carId);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task CompleteExpiredBookingsAsync()
    {
        var expired = await _bookingRepository.GetExpiredConfirmedBookingsAsync(DateTime.UtcNow);
        foreach (var b in expired)
        {
            b.Status = BookingStatus.Completed;
            b.UpdatedAt = DateTime.UtcNow;
            _bookingRepository.Update(b);

            var car = await _carRepository.GetByIdAsync(b.CarId);
            if (car != null)
            {
                car.IsAvailable = true;
                car.UpdatedAt = DateTime.UtcNow;
                _carRepository.Update(car);
            }
        }
        if (expired.Any())
        {
            await _bookingRepository.SaveChangesAsync();
            _logger.LogInformation("Завершено {Count} просроченных бронирований", expired.Count());
        }
    }

    public async Task UpdateBookingAsync(BookingDto bookingDto)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingDto.Id);
        if (booking == null) throw new KeyNotFoundException($"Бронирование с ID {bookingDto.Id} не найдено");

        _mapper.Map(bookingDto, booking);
        booking.UpdatedAt = DateTime.UtcNow;
        _bookingRepository.Update(booking);
        await _bookingRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<BookingCalendarEventDto>> GetBookingsForCalendarAsync(DateTime? start = null, DateTime? end = null)
    {
        IEnumerable<Booking> bookings;
        if (start.HasValue && end.HasValue)
            bookings = await _bookingRepository.GetBookingsForPeriodAsync(start.Value, end.Value);
        else
            bookings = await _bookingRepository.GetAllBookingsAsync();

        return bookings.Select(b => new BookingCalendarEventDto
        {
            Id = b.Id,
            // !!! ИЗМЕНЕНО: теперь заголовок – информация об автомобиле
            Title = b.Car != null ? $"{b.Car.Brand} {b.Car.Model} ({b.Car.LicensePlate})" : "Автомобиль удалён",
            Start = b.StartDate,
            End = b.EndDate.AddDays(1),
            Color = b.Status == BookingStatus.Confirmed ? "#28a745" :
                    b.Status == BookingStatus.Pending ? "#ffc107" :
                    b.Status == BookingStatus.Active ? "#007bff" : "#6c757d",
            Status = b.Status.ToString(),
            UserId = b.UserId,
            UserName = b.User != null ? $"{b.User.FirstName} {b.User.LastName}" : "Неизвестный",
            CarId = b.CarId,                                 // ← добавить это поле в DTO
            CarInfo = b.Car != null ? $"{b.Car.Brand} {b.Car.Model} ({b.Car.LicensePlate})" : "",
            Notes = b.Notes ?? ""
        });
    }

    public async Task<BookingDto> CreateAdminBookingAsync(AdminBookingDto adminDto)
    {
        // 1. Если клиент не указан, используем системного пользователя
        if (adminDto.UserId == null || adminDto.UserId == Guid.Empty)
        {
            var systemUser = await GetOrCreateSystemUserAsync();
            adminDto.UserId = systemUser.Id;
        }

        // 2. Проверяем автомобиль
        var car = await _carRepository.GetByIdAsync(adminDto.CarId);
        if (car == null)
            throw new InvalidOperationException("Автомобиль не найден");

        // 3. Проверяем, свободен ли автомобиль на выбранные даты
        var isAvailable = await _carRepository.IsCarAvailableAsync(
            adminDto.CarId, adminDto.StartDate, adminDto.EndDate);
        if (!isAvailable)
            throw new InvalidOperationException("Автомобиль занят на выбранные даты");

        // 4. Создаём бронь
        var booking = new Booking
        {
            CarId = adminDto.CarId,
            UserId = adminDto.UserId.Value,        // теперь точно не null
            StartDate = adminDto.StartDate,
            EndDate = adminDto.EndDate,
            TotalPrice = adminDto.TotalPrice ?? 0,
            Notes = adminDto.Notes,
            Status = adminDto.Status,
            CreatedAt = DateTime.UtcNow
        };

        // Если статус "Подтверждено" — помечаем автомобиль как недоступный
        if (adminDto.Status == BookingStatus.Confirmed)
        {
            car.IsAvailable = false;
            car.UpdatedAt = DateTime.UtcNow;
            _carRepository.Update(car);
        }

        await _bookingRepository.AddAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        _logger.LogInformation("Администратор создал бронь {BookingId} для авто {CarId} (клиент {UserId})",
            booking.Id, booking.CarId, booking.UserId);

        return _mapper.Map<BookingDto>(booking);
    }

    private async Task<User> GetOrCreateSystemUserAsync()
    {
        const string systemEmail = "system@carrental.local";
        var systemUser = await _userManager.FindByEmailAsync(systemEmail);
        if (systemUser == null)
        {
            systemUser = new User
            {
                UserName = "System",
                Email = systemEmail,
                EmailConfirmed = true,
                Status = UserStatus.Active,
                FirstName = "Системный",
                LastName = "Пользователь",
                CreatedAt = DateTime.UtcNow
            };
            // Генерируем пароль, соответствующий политике (заглавная, цифра, спецсимвол)
            var rawPassword = $"System_{Guid.NewGuid():N}!A1";
            var result = await _userManager.CreateAsync(systemUser, rawPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Не удалось создать системного пользователя: {errors}");
            }
        }
        return systemUser;
    }
    public async Task<bool> DeleteBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null) return false;

        if (booking.Status == BookingStatus.Confirmed)
        {
            var car = await _carRepository.GetByIdAsync(booking.CarId);
            if (car != null)
            {
                car.IsAvailable = true;
                car.UpdatedAt = DateTime.UtcNow;
                _carRepository.Update(car);
            }
        }

        _bookingRepository.Delete(booking);
        var result = await _bookingRepository.SaveChangesAsync();
        if (result)
        {
            _logger.LogInformation("Бронь {BookingId} удалена администратором", bookingId);
        }
        return result;
    }
}