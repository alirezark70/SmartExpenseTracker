using Microsoft.EntityFrameworkCore;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence.Users;
using SmartExpenseTracker.Core.Domain.DomainModels.Identity;
using SmartExpenseTracker.Infra.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly WriteDbContext _context;
        private readonly DbSet<ApplicationUser> _users;
        private readonly DbSet<UserLoginHistory> _loginHistories;
        private readonly DbSet<UserActivity> _userActivities;

        public UserRepository(WriteDbContext context)
        {
            _context = context;
            _users = _context.Set<ApplicationUser>();
            _loginHistories = _context.Set<UserLoginHistory>();
            _userActivities = _context.Set<UserActivity>();
        }

        public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
        }

        public async Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == userName && !u.IsDeleted, cancellationToken);
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
        }

        public async Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
        {
            return await _users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokenHash == refreshTokenHash &&
                    u.RefreshTokenExpiryTime > DateTime.UtcNow &&
                    !u.IsDeleted,
                    cancellationToken);
        }

        public async Task<bool> IsUserNameUniqueAsync(string userName, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var normalizedUserName = userName.ToUpperInvariant();
            var query = _users.Where(u => u.NormalizedUserName == normalizedUserName && !u.IsDeleted);

            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = email.ToUpperInvariant();
            var query = _users.Where(u => u.NormalizedEmail == normalizedEmail && !u.IsDeleted);

            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetActiveUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _users
                .Where(u => u.IsActive && !u.IsDeleted)
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default)
        {
            return await _users.CountAsync(u => u.IsActive && !u.IsDeleted, cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var normalizedRoleName = roleName.ToUpperInvariant();

            return await _users
                .Where(u => !u.IsDeleted && u.IsActive)
                .Where(u => u.UserRoles.Any(ur => ur.Role.NormalizedName == normalizedRoleName))
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync(cancellationToken);
        }

        public async Task AddLoginHistoryAsync(UserLoginHistory loginHistory, CancellationToken cancellationToken = default)
        {
            await _loginHistories.AddAsync(loginHistory, cancellationToken);
        }

        public async Task<IReadOnlyList<UserLoginHistory>> GetLoginHistoriesAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default)
        {
            return await _loginHistories
                .Where(lh => lh.UserId == userId)
                .OrderByDescending(lh => lh.LoginTime)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task AddUserActivityAsync(UserActivity activity, CancellationToken cancellationToken = default)
        {
            await _userActivities.AddAsync(activity, cancellationToken);
        }

        public async Task<IReadOnlyList<UserActivity>> GetUserActivitiesAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            return await _userActivities
                .Where(ua => ua.UserId == userId && ua.OccurredAt >= from && ua.OccurredAt <= to)
                .OrderByDescending(ua => ua.OccurredAt)
                .ToListAsync(cancellationToken);
        }
    }
}

