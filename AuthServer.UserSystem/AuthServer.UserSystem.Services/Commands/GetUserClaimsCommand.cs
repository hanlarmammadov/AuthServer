using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Models;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.UserSystem.Services.Commands
{
    public class GetUserClaimsCommand : IGetUserClaimsCommand
    {
        private readonly IMongoCollection<User> _userRepo;
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly IMongoCollection<Role> _roleRepo;
        private readonly ILogger _logger;

        public GetUserClaimsCommand(IMongoCollection<User> userRepo,
                                    IMongoCollection<Account> accountRepo,
                                    IMongoCollection<Role> roleRepo,
                                    ILogger logger)
        {
            _userRepo = userRepo;
            _accountRepo = accountRepo;
            _roleRepo = roleRepo;
            _logger = logger;
        }

        protected Claim AccountStatusClaim(Account account)
        {
            Claim claim = null;
            //Check statuses
            if (account.AccountStatus == AccountStatus.Inactive)
                claim = new Claim("AccountStatus", "AccountInactive");
            else if (account.AccountStatus == AccountStatus.Banned)
                claim = new Claim("AccountStatus", "AccountBanned");
            else if (account.EmailStatus == EmailStatus.NotConfirmed)
                claim = new Claim("AccountStatus", "EmailNotConfirmed");
            else if (account.PasswordStatus == PasswordStatus.Expired)
                claim = new Claim("AccountStatus", "PasswordExpired");
            else if (account.PasswordStatus == PasswordStatus.Temp)
                claim = new Claim("AccountStatus", "PasswordIsTemporary");
            else
                claim = new Claim("AccountStatus", "Ok");

            return claim;
        }

        protected List<Claim> UserClaims(User user, List<Role> userRoles)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim("Account", user.AccountId));
            claims.Add(new Claim("FirstName", user.FirstName));
            claims.Add(new Claim("LastName", user.LastName));
            claims.Add(new Claim("Gender", ((Int32)user.Gender).ToString()));

            foreach (var role in userRoles.Where(x => x.Status == RoleStatus.Active))
                claims.Add(new Claim(ClaimTypes.Role, role.Id));

            return claims;
        }

        public async Task<List<Claim>> Execute(UserClaimsRequest request)
        {
            List<Claim> claims = new List<Claim>();
            try
            {
                //Retrieve account and check existence
                Account account = await _accountRepo.Find(x => x.AccountId == request.AccountId).SingleOrDefaultAsync();
                if (account == null)
                    throw new Exception("Account with provided account id was not found");

                claims.Add(new Claim("Username", account.Username));
                claims.Add(new Claim("MainEmail", account.Email));

                Claim accountStatusClaim = AccountStatusClaim(account);
                claims.Add(accountStatusClaim);
                if (accountStatusClaim.Value != "Ok")
                {
                    return claims;
                }

                //Get user
                User user = await _userRepo.Find(x => x.AccountId == request.AccountId).SingleOrDefaultAsync();
                if (account == null)
                    throw new Exception("User with provided account id was not found");

                //Filter definition for user roles
                var rolesFilter = Builders<Role>.Filter.And(Builders<Role>.Filter.Eq(r => r.Status, RoleStatus.Active),
                                                            Builders<Role>.Filter.In(r => r.Consumer, request.ClaimsConsumers),
                                                            Builders<Role>.Filter.In(r => r.Id, user.RoleIds));

                List<Role> userRoles = await _roleRepo.Find(rolesFilter).ToListAsync();

                List<Claim> userClaims = UserClaims(user, userRoles);

                claims.AddRange(userClaims);

                return claims;
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("GetUserClaimsCommand.Execute", "Exception was thrown", new
                {
                    UserClaimsRequest = request,
                    Exception = ex
                });

                throw;
            }
        }
    }
}
