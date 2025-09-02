using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Notifications;
using AutoMapper;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationRecipientsController : ControllerBase
    {
        private readonly INotificationRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public NotificationRecipientsController(
            INotificationRepository repo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // GET /api/notificationrecipients/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var rec = await _repo.GetRecipientByIdAsync(id, ct);
            if (rec == null) return NotFound();
            return Ok(_mapper.Map<NotificationRecipientDto>(rec));
        }

        // GET /api/notificationrecipients/user/{userId}?onlyUnread=true
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] bool onlyUnread = false, CancellationToken ct = default)
        {
            var recipients = await _repo.GetRecipientsByUserAsync(userId, onlyUnread, ct);
            var dtos = recipients.Select(r => _mapper.Map<NotificationRecipientDto>(r));
            return Ok(dtos);
        }

        // GET /api/notificationrecipients/pending?maxItems=100
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending([FromQuery] int maxItems = 100, CancellationToken ct = default)
        {
            var pending = await _repo.GetPendingRecipientsAsync(maxItems, ct);
            var dtos = pending.Select(r => _mapper.Map<NotificationRecipientDto>(r));
            return Ok(dtos);
        }

        // POST /api/notificationrecipients/{id}/deliver
        [HttpPost("{id:guid}/deliver")]
        public async Task<IActionResult> MarkDelivered(Guid id, CancellationToken ct)
        {
            var rec = await _repo.GetRecipientByIdAsync(id, ct);
            if (rec == null) return NotFound();

            if (!rec.IsDelivered)
            {
                rec.IsDelivered = true;
                rec.DeliveredAt = DateTime.UtcNow;
                rec.UpdatedAt = DateTime.UtcNow;
                _repo.UpdateRecipient(rec);
                await _uow.SaveChangesAsync(ct);
            }

            return NoContent();
        }

        // POST /api/notificationrecipients/{id}/read
        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
        {
            var rec = await _repo.GetRecipientByIdAsync(id, ct);
            if (rec == null) return NotFound();

            if (!rec.IsRead)
            {
                rec.IsRead = true;
                rec.ReadAt = DateTime.UtcNow;
                rec.UpdatedAt = DateTime.UtcNow;
                _repo.UpdateRecipient(rec);
                await _uow.SaveChangesAsync(ct);
            }

            return NoContent();
        }

        // GET /api/notificationrecipients/user/{userId}/count-unread
        [HttpGet("user/{userId:guid}/count-unread")]
        public async Task<IActionResult> CountUnread(Guid userId, CancellationToken ct)
        {
            var count = await _repo.CountUnreadForUserAsync(userId, ct);
            return Ok(new { userId, unread = count });
        }
    }
}
