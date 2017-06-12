using System;
using OTP.Domain.Models;
using System.Collections.Generic;

namespace OTP.BussinesInterfaces.ModelLogic
{
    public interface IUserLogic
    {
        User GetUser(Guid id);

        bool IsValidUser(string user, string password);

        ICollection<User> GetUsers();

        void Update(User user);
    }
}
