using System;
using System.Security.Cryptography;
using System.Text;

namespace ScalableTeaching.Helpers
{
    public class SSHKeyHelper
    {

        /// <summary>
        /// Takes a username and RSA object containing a private key and outputs a public key in the OpenSSH public key format
        /// </summary>
        /// <param name="privateKey">RSA object containing the private key that should be added</param>
        /// <param name="username">The username that is appended to the key</param>
        /// <returns>public key in "OpenSSH public key format"</returns>
        public static string GetSSHPublicKey(RSA privateKey, string username)
        {
            var key = Convert.ToBase64String(privateKey.ExportRSAPublicKey());
            var keyBuilder = new StringBuilder();
            keyBuilder.Append("ssh-rsa ");
            keyBuilder.Append(key);
            keyBuilder.Append(' ');
            keyBuilder.Append(username);
            keyBuilder.Append("@scalable-teaching-sdu\n");
            return keyBuilder.ToString();
        }

        /// <summary>
        /// Formats a privatekey in a PEM format
        /// </summary>
        /// <param name="privateKey">RSA object containing a privatekey</param>
        /// <returns>PEM formatatted RSA privatekey</returns>
        public static string ExportKeyAsPEM(RSA privateKey)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append("-----BEGIN RSA PRIVATE KEY-----\n");
            var key = Convert.ToBase64String(privateKey.ExportRSAPrivateKey());
            var count = 0;
            foreach (char c in key)
            {
                if (++count == 64)
                {
                    keyBuilder.Append(c);
                    keyBuilder.Append('\n');
                    count = 0;
                }
                else
                {
                    keyBuilder.Append(c);
                }
            }
            keyBuilder.Append("\n-----END RSA PRIVATE KEY-----\n");
            return keyBuilder.ToString();
        }

        /// <summary>
        /// Parses a RSA PEM formatted RSA private key
        /// </summary>
        /// <param name="PEMPrivateKey">PEM formatted private key</param>
        /// <returns>RSA Object containing the read private key</returns>
        public static RSA ParseKeyFromPem(string PEMPrivateKey)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(PEMPrivateKey.AsSpan());
            return rsa;
        }
    }
}
