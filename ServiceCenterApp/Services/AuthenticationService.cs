using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.ViewModels;
using System.Diagnostics;
using System.Windows;

namespace ServiceCenterApp.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;

        public AuthenticationService(
            ApplicationDbContext dbcontext, 
            ICurrentUserService currentUserService, 
            IPasswordHasher passwordHasher)
        {
            _currentUserService = currentUserService;
            _dbContext = dbcontext;
            _passwordHasher = passwordHasher;
        }

        public async Task CreateAdministratorAsync(string firstName, string surName, string? patronymic, string positionName, string pin)
        {

            Role? adminRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.RoleId == 1) 
                ?? throw new InvalidOperationException("Роль администратора не найдена в базе данных");

            if (await _dbContext.Employees.AnyAsync())
            {
                throw new InvalidOperationException("Пользователи уже существуют");
            }

            using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                Position? position = await _dbContext.Positions.FirstOrDefaultAsync(p => p.PositionName == positionName);
                if (position == null)
                {
                    position = new Position { PositionName = positionName };
                    _dbContext.Positions.Add(position);
                }

                var newAdmin = new Employee
                {
                    FirstName = firstName,
                    SurName = surName,
                    Patronymic = patronymic,
                    PINHash = _passwordHasher.Hash(pin), 
                    RoleId = adminRole.RoleId,
                    Role = adminRole,
                    PositionId = position.PositionId,
                    Position = position
                };

                _dbContext.Employees.Add(newAdmin);

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Если произошла ошибка, откатываем все изменения
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> LoginAsync(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                return false;
            }

            List<Employee> allEmployees = await _dbContext.Employees
                .Include(e => e.Role)
                .Include(e => e.Position)
                .ToListAsync();

            foreach (Employee employee in allEmployees)
            {
                if (_passwordHasher.Verify(pin, employee.PINHash))
                {
                    _currentUserService.SetCurrentUser(employee);
                    MessageBox.Show($"Вход выполнен: {employee.FirstName} {employee.SurName}");

                    return true;
                }
            }



#warning Custom ErrorBox


            return false;
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }
    }
}