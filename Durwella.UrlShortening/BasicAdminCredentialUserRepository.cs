using System;
using System.Collections.Generic;
using System.Text;

namespace Durwella.UrlShortening
{
    public class BasicAdminCredentialUserRepository: IUserRepository
    {
        private const string UserName = "admin";
        private readonly string _password;
        public BasicAdminCredentialUserRepository(string password)
        {
            _password = password;
        }

        public bool Validate(string userName, string password)
        {
            return userName == UserName && password == _password;
        }
    }
}
