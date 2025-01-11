using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared
{
    public class JwtSettings
    {
        /// <summary>
        /// The secret key used for signing the JWT token. Ensure this is a strong and secure key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The issuer of the JWT token, typically your application or API name.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// The audience for the JWT token, representing the intended recipients.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// The token expiration time in minutes.
        /// </summary>
        public int ExpiresInMinutes { get; set; }
    }

}
