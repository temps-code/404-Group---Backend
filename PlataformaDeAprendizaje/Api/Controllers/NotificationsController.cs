using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Notifications;
using Application.UseCases.Notifications;
using AutoMapper;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly CreateNotificationHandler _createHandler;
        private readonly INotificationRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public NotificationsController(
            CreateNotificationHandler createHandler,
            INotificationRepository repo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // POST /api/notifications
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto, CancellationToken ct)
        {
            if (dto == null) return BadRequest(new { message = "Request body required." });
            try
            {
                var created = await _createHandler.HandleAsync(dto, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException aex) { return BadRequest(new { message = aex.Message }); }
        }

        // GET /api/notifications/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdIncludeInactiveAsync(id, ct);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<NotificationDto>(entity);
            return Ok(dto);
        }

        // GET /api/notifications -> todos (activos + inactivos)
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var all = await _repo.GetAllAsync(ct);
            return Ok(all.Select(n => _mapper.Map<NotificationDto>(n)));
        }

        // GET /api/notifications/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var list = await _repo.GetAllActiveAsync(ct);
            return Ok(list.Select(n => _mapper.Map<NotificationDto>(n)));
        }

        // GET /api/notifications/inactive
        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactive(CancellationToken ct)
        {
            var list = await _repo.GetAllInactiveAsync(ct);
            return Ok(list.Select(n => _mapper.Map<NotificationDto>(n)));
        }

        // GET /api/notifications/user/{userId}?onlyUnread=true
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetRecipientsByUser(Guid userId, [FromQuery] bool onlyUnread = false, CancellationToken ct = default)
        {
            var recipients = await _repo.GetRecipientsByUserAsync(userId, onlyUnread, ct);
            var dtos = recipients.Select(r => _mapper.Map<NotificationRecipientDto>(r));
            return Ok(dtos);
        }

        // POST /api/notifications/{recipientId}/deliver -> marcar entregado
        [HttpPost("recipient/{recipientId:guid}/deliver")]
        public async Task<IActionResult> MarkDelivered(Guid recipientId, CancellationToken ct)
        {
            // Carga recipient
            var rec = await _repo.GetRecipientsByUserAsync(Guid.Empty, false, ct); // no directo, usamos DB query below
            // mejor usar context via repo; pero aquí hacemos lookup por GetPendingRecipients y find
            var pending = (await _repo.GetPendingRecipientsAsync(1000, ct)).FirstOrDefault(r => r.Id == recipientId);
            if (pending == null) return NotFound();

            pending.IsDelivered = true;
            pending.DeliveredAt = DateTime.UtcNow;
            _repo.UpdateRecipient(pending);
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/notifications/{recipientId}/read -> marcar leido
        [HttpPost("recipient/{recipientId:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid recipientId, CancellationToken ct)
        {
            // buscar recipient
            var all = await _repo.GetRecipientsByUserAsync(Guid.Empty, false, ct); // placeholder, we'll find
            // Instead get recipients for user then find by id.
            var pending = (await _repo.GetPendingRecipientsAsync(5000, ct)).FirstOrDefault(r => r.Id == recipientId);
            if (pending == null) return NotFound();

            pending.IsRead = true;
            pending.ReadAt = DateTime.UtcNow;
            _repo.UpdateRecipient(pending);
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // GET /api/notifications/user/{userId}/count-unread
        [HttpGet("user/{userId:guid}/count-unread")]
        public async Task<IActionResult> CountUnread(Guid userId, CancellationToken ct)
        {
            var count = await _repo.CountUnreadForUserAsync(userId, ct);
            return Ok(new { userId, unread = count });
        }

        // POST /api/notifications/{id}/deactivate
        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.DeactivateAsync(id, ct);
            if (!ok) return NotFound();
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/notifications/{id}/reactivate
        [HttpPost("{id:guid}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
        {
            var ok = await _repo.ReactivateAsync(id, ct);
            if (!ok) return NotFound();
            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
