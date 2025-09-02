using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Notifications;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.UseCases.Notifications
{
    public class CreateNotificationHandler
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateNotificationHandler(
            INotificationRepository notificationRepo,
            IUserRepository userRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _notificationRepo = notificationRepo;
            _userRepo = userRepo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<NotificationDto> HandleAsync(CreateNotificationDto dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Title)) throw new ArgumentException("Title is required.");
            if (string.IsNullOrWhiteSpace(dto.Message)) throw new ArgumentException("Message is required.");

            // Crear entidad Notification
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Message = dto.Message,
                Payload = dto.Payload,
                Channel = string.IsNullOrWhiteSpace(dto.Channel) ? "ui" : dto.Channel,
                SenderId = dto.SenderId,
                CourseId = dto.CourseId,
                ContextType = dto.ContextType,
                ContextId = dto.ContextId,
                SentAt = DateTime.UtcNow,
                ExpiresAt = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Evitar duplicados por contexto si quieres (opcional) - ejemplo simple:
            if (!string.IsNullOrEmpty(notification.ContextType) && !string.IsNullOrEmpty(notification.ContextId))
            {
                // Si ya existe una notificación con el mismo context y title reciente, podrías evitar duplicar
                // (comentado por defecto). Descomenta si lo deseas.
                // var exists = await _notificationRepo.GetAllAsync(ct);
            }

            // Guardar notificación (no persiste aún si tu AddAsync no guarda internamente)
            await _notificationRepo.AddAsync(notification, ct);

            // Determinar recipients
            var recipientsToAdd = new List<NotificationRecipient>();

            if (dto.TargetAll)
            {
                // traer todos los usuarios activos (puede ser costoso en DB grandes)
                var users = await _userRepo.GetAllActiveAsync(ct);
                foreach (var u in users)
                {
                    recipientsToAdd.Add(new NotificationRecipient
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notification.Id,
                        UserId = u.Id,
                        IsDelivered = false,
                        DeliveredAt = null,
                        IsRead = false,
                        ReadAt = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            else if (dto.RecipientIds != null && dto.RecipientIds.Any())
            {
                // Validar que los usuarios existan y estén activos
                var ids = dto.RecipientIds.Distinct().ToList();
                var users = await _userRepo.GetAllActiveAsync(ct);
                var usersSet = users.Select(u => u.Id).ToHashSet();

                foreach (var id in ids)
                {
                    if (!usersSet.Contains(id)) continue; // ignorar ids inválidos
                    recipientsToAdd.Add(new NotificationRecipient
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notification.Id,
                        UserId = id,
                        IsDelivered = false,
                        DeliveredAt = null,
                        IsRead = false,
                        ReadAt = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Añadir recipients al repo (no guarda hasta SaveChanges)
            foreach (var r in recipientsToAdd)
            {
                await _notificationRepo.AddRecipientAsync(r, ct);
            }

            // Persistir todo en una sola transacción (IUnitOfWork.SaveChangesAsync)
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<NotificationDto>(notification);
        }
    }
}
