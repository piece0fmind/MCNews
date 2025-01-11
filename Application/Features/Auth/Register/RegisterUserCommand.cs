using Application.Domain;
using Application.Features.Auth.Services;
using Application.Infrastructure;
using Application.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Auth.Register
{
    public class RegisterUserCommand : IRequest<ApiResponse>
    {
        public string UserName { get; set; } 
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
       
    }
    public class RegisterUserResponse
    {
        public string Message { get; set; }
    }
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, ApiResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        public RegisterUserHandler(
            AppDbContext dbContext,
            IMediator mediator,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _mediator = mediator;
            _configuration = configuration;
        }
        public async Task<ApiResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var validationErrors = await ValidateRequestAsync(request);
            if (validationErrors.Any())
                return new ApiResponse
                {
                    Success = false,
                    Message = string.Join("; ", validationErrors)
                };

            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var newUser = await SaveUserAsync(request, _dbContext, cancellationToken);
                var defaultRole = await GetDefaultRoleAsync(cancellationToken);

                await AssignRoleToUser(newUser.Id, defaultRole.Id);
                await transaction.CommitAsync(cancellationToken);

                return new ApiResponse
                {
                    Success = true,
                    Message = $"Successfully registered as {newUser.UserName}.",
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Registration failed. Please try again.",
                };
            }
        }
        public async Task AssignRoleToUser(Guid userId, Guid roleId)
        {
            var userRole = new UserRole
            {
                UserId = userId,  
                RoleId = roleId   
            };

            await _dbContext.UserRoles.AddAsync(userRole);
            await _dbContext.SaveChangesAsync();
        }
        private async Task<AppUser> SaveUserAsync(RegisterUserCommand request, AppDbContext _dbContext, CancellationToken cancellationToken)
        {
            var newUser = new AppUser
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.Trim(),
                UserName = request.UserName.Trim(),
                Phone = request.Phone,
                Password = request.Password, // need to hash
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            await _dbContext.Users.AddAsync(newUser, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return newUser;
        }
        private async Task<List<string>> ValidateRequestAsync(RegisterUserCommand request)
        {
            var errors = new List<string>();

            if (request.Password != request.ConfirmPassword)
                errors.Add("Passwords do not match.");

            var userExists = await _dbContext.Users
                .AnyAsync(x => x.IsActive && (x.Email == request.Email || x.UserName == request.UserName));
            if (userExists)
                errors.Add("Email or Username already exists.");

            var roleExists = await _dbContext.Roles.AnyAsync(x => x.Name == ApplicationConstants.DefaultRole);
            if (!roleExists)
                errors.Add("Default role is not configured.");

            return errors;
        }

        private async Task<Role> GetDefaultRoleAsync(CancellationToken cancellationToken)
        {
            var defaultRole = await _dbContext.Roles
                .FirstOrDefaultAsync(x => x.Name == ApplicationConstants.DefaultRole, cancellationToken);

            if (defaultRole == null)
                throw new Exception("Default role is not configured.");

            return defaultRole;
        }

    }

    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("Username is required.");
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("FirstName is required.");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("LastName is required.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
        }
    }

}
