using MongoDB.Driver;
using Production_Electricite.Models;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Production_Electricite.Controllers.utils
{
    public class LoginInfos
    {
        public User getUserFromRequest(User user)
        {
            IMongoCollection<User> _collection = new Connexion().getCollection<User>("User");
            return _collection.AsQueryable().FirstOrDefault(u => u.login == user.login);
        }

        public string encrypt(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return Convert.ToBase64String(hashBytes);
        }

        public bool isGoodPassword(string passwordRequest, string passwordDB)
        {
            byte[] hashBytes = Convert.FromBase64String(passwordDB);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            var pbkdf2 = new Rfc2898DeriveBytes(passwordRequest, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;
            return true;
        }

        public bool userExists(User user)
        {
            return new Connexion().
                getCollection<User>("User").
                AsQueryable().
                FirstOrDefault(u => u.login == user.login) != null;
        }
    }
}
