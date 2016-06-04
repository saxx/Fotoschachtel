using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Fotoschachtel.Services
{
    public class HashService
    {
        public string HashEventPassword([NotNull] string @event, [CanBeNull] string password)
        {
            if (string.IsNullOrWhiteSpace(@event))
            {
                throw new ArgumentException("Event must not be empty", nameof(@event));
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                password = "";
            }
            var hash = Hash(password, @event.ToLower());
            hash = hash.Replace(":", "");
            hash = hash.Replace("=", "");
            return hash;
        }


        private string Hash([NotNull] string value, [NotNull]string salt)
        {
            var result = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                value,
                Encoding.Unicode.GetBytes(salt),
                KeyDerivationPrf.HMACSHA1,
                10000,
                256 / 8));
            return result;
        }
    }
}
