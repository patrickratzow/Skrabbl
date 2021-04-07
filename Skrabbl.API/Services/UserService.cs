﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrabbl.DataAccess;
using Skrabbl.Model;

namespace Skrabbl.API.Services
{
    public class UserService : IUserService
    {
        IUserRepository _userRepository;

        public UserService(IUserRepository userRepo)
        {
            _userRepository = userRepo;
        }




        public async Task<User> CreateUser(string _userName, string _password, string _email)
        {
            User current = new User();
            current.Username = _userName;

            byte[] salt = new CryptographyService().CreateSalt();
            current.Password = new CryptographyService().GenerateHash(_password, salt);
            current.Email = _email;
            current.Salt = Convert.ToBase64String(salt); 

            current.Id = await _userRepository.AddUser(current);

            return current;

        }

        public async Task<User> GetUser(string _username, string _password)
        {
            User user = await _userRepository.GetUserByUsername(_username);
            CryptographyService cryptographyService = new CryptographyService();

            byte[] salt = Convert.FromBase64String(user.Salt);
            bool equal = cryptographyService.AreEqual(_password, user.Password, salt);
            if (equal)
            {
                return user;
            }
            return null;
        }
    }
}