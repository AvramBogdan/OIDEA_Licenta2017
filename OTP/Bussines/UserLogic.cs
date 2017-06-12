using System;
using System.Collections.Generic;
using System.Linq;
using OTP.Domain.Models;
using OTP.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using OTP.BussinesInterfaces.ModelLogic;

namespace Business
{
    public class UserLogic : IUserLogic
    {
        private readonly IRepository<User> _userRepository;

        public UserLogic(IRepository<User> users)
        {
            _userRepository = users;
        }

        
        public ICollection<User> GetUsers()
        {
            return _userRepository.Query().ToList();
        }

        public User GetUser(Guid id)
        {
            return _userRepository.Query(u => u.Id == id).FirstOrDefault();
        }
        public bool IsValidUser(string user, string password)
        {
            return _userRepository.Query(u => u.Password == password).Any(u => u.Email == user || u.UserName == user);
        }

        public void Update(User user)
        {
            var editedUser = _userRepository.Query(u => u.Id == user.Id).FirstOrDefault();
            editedUser.Email = user.Email;
            editedUser.UserName = user.UserName;
            editedUser.CompleteName = user.CompleteName;
            editedUser.RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 120 };
            _userRepository.SaveChanges();
        
        }
    }
}
