using BookMatrix.Models;

namespace BookMatrix.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetDashboardAsync();
}