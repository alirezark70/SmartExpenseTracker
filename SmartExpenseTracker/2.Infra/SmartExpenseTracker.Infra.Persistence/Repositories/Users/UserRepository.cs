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
        private readonly DbSet<ApplicationRole> _roles;
        private readonly DbSet<ApplicationUserRole> _userRoles;

        public UserRepository(WriteDbContext context)
        {
            _context = context;
            _users = _context.Set<ApplicationUser>();
            _loginHistories = _context.Set<UserLoginHistory>();
            _userActivities = _context.Set<UserActivity>();
            _roles = _context.Set<ApplicationRole>();
            _userRoles = _context.Set<ApplicationUserRole>();
        }

        // User Operations
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

        public async Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            // First we need to hash the refresh token to compare with stored hash
            var users = await _users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => !u.IsDeleted && u.RefreshTokenExpiryTime > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            // Check each user's refresh token
            foreach (var user in users)
            {
                if (user.ValidateRefreshToken(DateTime.UtcNow, refreshToken))
                    return user;
            }

            return null;
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
                .Where(u => u.UserRoles.Any(ur => ur.Role.NormalizedName == normalizedRoleName && !ur.IsDeleted))
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync(cancellationToken);
        }

        public async Task<ApplicationUser> AddAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            await _users.AddAsync(user, cancellationToken);
            return user;
        }

        public void Update(ApplicationUser user)
        {
            _users.Update(user);
        }

        public void Remove(ApplicationUser user)
        {
            _users.Remove(user);
        }

        // Role Operations
        public async Task<ApplicationRole?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var normalizedName = roleName.ToUpperInvariant();
            return await _roles
                .FirstOrDefaultAsync(r => r.NormalizedName == normalizedName && !r.IsDeleted, cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationRole>> GetAllRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _roles
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task AddUserToRoleAsync(ApplicationUser user, ApplicationRole role, CancellationToken cancellationToken = default)
        {
            var userRole = new ApplicationUserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                User = user,
                Role = role
            };

            await _userRoles.AddAsync(userRole, cancellationToken);
        }

        public async Task RemoveUserFromRoleAsync(ApplicationUser user, ApplicationRole role, CancellationToken cancellationToken = default)
        {
            var userRole = await _userRoles
                .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);

            if (userRole != null)
            {
                _userRoles.Remove(userRole);
            }
        }

        // Login History Operations
        public async Task AddLoginHistoryAsync(UserLoginHistory loginHistory, CancellationToken cancellationToken = default)
        {
            await _loginHistories.AddAsync(loginHistory, cancellationToken);
        }

        public async Task<IReadOnlyList<UserLoginHistory>> GetLoginHistoriesAsync(
            Guid userId,
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            return await _loginHistories
                .Where(lh => lh.UserId == userId && !lh.IsDeleted)
                .OrderByDescending(lh => lh.LoginTime)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<UserLoginHistory>> GetFailedLoginAttemptsAsync(
            Guid userId,
            DateTime fromDate,
            CancellationToken cancellationToken = default)
        {
            return await _loginHistories
                .Where(lh => lh.UserId == userId &&
                            !lh.IsSuccessful &&
                            lh.LoginTime >= fromDate &&
                            !lh.IsDeleted)
                .OrderByDescending(lh => lh.LoginTime)
                .ToListAsync(cancellationToken);
        }

        // User Activity Operations
        public async Task AddUserActivityAsync(UserActivity activity, CancellationToken cancellationToken = default)
        {
            await _userActivities.AddAsync(activity, cancellationToken);
        }

        public async Task<IReadOnlyList<UserActivity>> GetUserActivitiesAsync(
            Guid userId,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            return await _userActivities
                .Where(ua => ua.UserId == userId &&
                            ua.OccurredAt >= from &&
                            ua.OccurredAt <= to &&
                            !ua.IsDeleted)
                .OrderByDescending(ua => ua.OccurredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<UserActivity>> GetRecentUserActivitiesAsync(
            Guid userId,
            int count = 20,
            CancellationToken cancellationToken = default)
        {
            return await _userActivities
                .Where(ua => ua.UserId == userId && !ua.IsDeleted)
                .OrderByDescending(ua => ua.OccurredAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        // Search Operations
        public async Task<IReadOnlyList<ApplicationUser>> SearchUsersAsync(
            string searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var normalizedSearchTerm = searchTerm.ToUpperInvariant();

            return await _users
                .Where(u => !u.IsDeleted &&
                           (u.NormalizedUserName.Contains(normalizedSearchTerm) ||
                            u.NormalizedEmail.Contains(normalizedSearchTerm) ||
                            u.FirstName.Contains(searchTerm) ||
                            u.LastName.Contains(searchTerm)))
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync(cancellationToken);
        }

        // Statistics Operations
        public async Task<Dictionary<string, int>> GetUserStatisticsAsync(CancellationToken cancellationToken = default)
        {
            var stats = new Dictionary<string, int>
            {
                ["TotalUsers"] = await _users.CountAsync(u => !u.IsDeleted, cancellationToken),
                ["ActiveUsers"] = await _users.CountAsync(u => u.IsActive && !u.IsDeleted, cancellationToken),
                ["VerifiedUsers"] = await _users.CountAsync(u => u.EmailVerified && !u.IsDeleted, cancellationToken),
                ["LockedUsers"] = await _users.CountAsync(u => u.LockoutEndDate > DateTime.UtcNow && !u.IsDeleted, cancellationToken)
            };

            return stats;
        }
    }
}

