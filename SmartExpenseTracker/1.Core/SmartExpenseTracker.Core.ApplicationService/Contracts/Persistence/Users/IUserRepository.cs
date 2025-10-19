
using SmartExpenseTracker.Core.Domain.DomainModels.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence.Users
{
    public interface IUserRepository
    {
        // Basic CRUD
        // User Operations
        Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<bool> IsUserNameUniqueAsync(string userName, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ApplicationUser>> GetActiveUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ApplicationUser>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default);
        Task<ApplicationUser> AddAsync(ApplicationUser user, CancellationToken cancellationToken = default);
        void Update(ApplicationUser user);
        void Remove(ApplicationUser user);

        // Role Operations
        Task<ApplicationRole?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ApplicationRole>> GetAllRolesAsync(CancellationToken cancellationToken = default);
        Task AddUserToRoleAsync(ApplicationUser user, ApplicationRole role, CancellationToken cancellationToken = default);
        Task RemoveUserFromRoleAsync(ApplicationUser user, ApplicationRole role, CancellationToken cancellationToken = default);

        // Login History Operations
        Task AddLoginHistoryAsync(UserLoginHistory loginHistory, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserLoginHistory>> GetLoginHistoriesAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserLoginHistory>> GetFailedLoginAttemptsAsync(Guid userId, DateTime fromDate, CancellationToken cancellationToken = default);

        // User Activity Operations
        Task AddUserActivityAsync(UserActivity activity, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserActivity>> GetUserActivitiesAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserActivity>> GetRecentUserActivitiesAsync(Guid userId, int count = 20, CancellationToken cancellationToken = default);

        // Search Operations
        Task<IReadOnlyList<ApplicationUser>> SearchUsersAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        // Statistics Operations
        Task<Dictionary<string, int>> GetUserStatisticsAsync(CancellationToken cancellationToken = default);
    }
}
