using Amas.Application.Common;
using Amas.Application.Identity;

namespace Amas.Api.Tests;

internal sealed class TestIdentityService : IIdentityService
{
    public Task<Result<IReadOnlyList<AdminUserDto>>> ListUsersAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Result<IReadOnlyList<AdminUserDto>>.Success([]));

    public Task<Result<AdminUserDto>> CreateUserAsync(CreateAdminUserRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(Result<AdminUserDto>.Failure("Not implemented in endpoint tests."));

    public Task<Result<AdminUserDto>> UpdateUserAsync(Guid id, UpdateAdminUserRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(Result<AdminUserDto>.Failure("Not implemented in endpoint tests."));

    public Task<Result<IReadOnlyList<RoleDto>>> ListRolesAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Result<IReadOnlyList<RoleDto>>.Success([]));

    public Task<Result<RoleDto>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(Result<RoleDto>.Failure("Not implemented in endpoint tests."));

    public Task<Result<RoleDto>> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(Result<RoleDto>.Failure("Not implemented in endpoint tests."));

    public Task<Result<IReadOnlyList<PermissionDto>>> ListPermissionsAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Result<IReadOnlyList<PermissionDto>>.Success([]));

    public Task<Result<AuthenticatedUserDto>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken) =>
        Task.FromResult(Result<AuthenticatedUserDto>.Failure("Invalid credentials."));
}
