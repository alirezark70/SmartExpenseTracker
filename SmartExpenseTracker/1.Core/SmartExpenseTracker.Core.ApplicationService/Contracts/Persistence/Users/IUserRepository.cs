
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
        Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshTokenHash, CancellationToken cancellationToken = default);

        // Validation
        Task<bool> IsUserNameUniqueAsync(string userName, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

        // Queries
        Task<IReadOnlyList<ApplicationUser>> GetActiveUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ApplicationUser>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default);

        // Login History
        Task AddLoginHistoryAsync(UserLoginHistory loginHistory, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserLoginHistory>> GetLoginHistoriesAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default);

        // User Activity
        Task AddUserActivityAsync(UserActivity activity, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserActivity>> GetUserActivitiesAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    }
}
