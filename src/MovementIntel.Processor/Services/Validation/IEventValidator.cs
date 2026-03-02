using MovementIntel.Processor.DTOs;

namespace MovementIntel.Processor.Services.Validation;

public record EventValidationResult(
    bool IsValid,
    string? Error,
    Guid ParsedEventId,
    DateTime ParsedTimestamp);

public interface IEventValidator {
    EventValidationResult Validate(MovementEventRequest request);
}
