using System;
using System.Collections.Generic;
using System.Text;

namespace Durwella.UrlShortening
{
    public interface IUserRepository
    {
        bool Validate(string userName, string password);
    }
}
