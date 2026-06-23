using Amas.Api.Contracts;
using Amas.Application.ContactRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Amas.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/contact-requests")]
public sealed class ContactRequestsController(IContactRequestService contactRequests) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("contact-public")]
    public async Task<ActionResult<ApiResponse<ContactRequestReceivedDto>>> Create(
        CreateContactRequestPayload payload,
        CancellationToken cancellationToken)
    {
        var result = await contactRequests.CreateAsync(new CreateContactRequest(
            payload.FullName,
            payload.Email,
            payload.Phone,
            payload.RequestType,
            payload.Message,
            payload.SourcePage,
            payload.CaptchaToken,
            payload.Website,
            GetClientIp(),
            Request.Headers.UserAgent.FirstOrDefault()), cancellationToken);

        return result.Succeeded
            ? Created("/api/v1/contact-requests", ApiResponse<ContactRequestReceivedDto>.Success(result.Data!))
            : BadRequest(ApiResponse<ContactRequestReceivedDto>.Failure(result.Error!));
    }

    private string? GetClientIp()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        return forwardedFor?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
            ?? HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}

public sealed record CreateContactRequestPayload(
    string FullName,
    string Email,
    string? Phone,
    string RequestType,
    string Message,
    string? SourcePage,
    string? CaptchaToken,
    string? Website);
