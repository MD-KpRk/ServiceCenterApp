using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Data.Configurations;
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
        private readonly INavigationService _navigationService;

        public AuthenticationService(
            INavigationService navigationService,
            ApplicationDbContext dbcontext, 
            ICurrentUserService currentUserService, 
            IPasswordHasher passwordHasher)
        {
            _navigationService = navigationService;
            _currentUserService = currentUserService;
            _dbContext = dbcontext;
            _passwordHasher = passwordHasher;
        }

        public async Task CreateAdministratorAsync(string firstName, string surName, string? patronymic, string positionName, string pin)
        {

            Role? adminRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.RoleId == ((int)RoleEnum.Administrator)) 
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

                Employee newAdmin = new Employee
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
            List<Employee> allEmployees = await _dbContext.Employees.ToListAsync();
            Employee? loggedInEmployee = null;

            foreach (Employee emp in allEmployees)
            {
                if (_passwordHasher.Verify(pin, emp.PINHash))
                {
                    loggedInEmployee = emp;
                    break;
                }
            }

            if (loggedInEmployee == null)
            {
                MessageBox.Show("Неверный PIN-код"); 
                return false;
            }

            Employee? userToAuthenticate = await _dbContext.Employees
                .Include(e => e.Position)
                .Include(e => e.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(e => e.EmployeeId == loggedInEmployee.EmployeeId);

            if (userToAuthenticate == null)
            {
                MessageBox.Show("Ошибка аутентификации: пользователь не найден после проверки PIN.");
                return false;
            }

            _currentUserService.SetCurrentUser(userToAuthenticate);

            _navigationService.NavigateToRoleMainPage();

            return true;
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }
    }
}