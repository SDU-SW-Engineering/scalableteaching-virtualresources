using ScalableTeaching.Models;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ScalableTeaching.Helpers
{
    internal class UserFactory
    {
        public static async Task<User> Create(string Username, string Email = null, string GeneralName = null, string Surname = null)
        {
            Username = Username.ToLower();
            (string, string) keys = await GetUserKeys(Username);
            return new User()
            {
                AccountType = User.UserType.User,
                Mail = Email,
                GeneralName = GeneralName,
                Surname = Surname,
                Username = Username.ToLower(),
                UserPrivateKey = keys.Item1,
                UserPublicKey = keys.Item2
            };
        }

        private async static Task<(string, string)> GetUserKeys(string Username)
        {
            //Generate keys
            var userTempDirectory = $"/tmp/{Username}";
            Directory.CreateDirectory(userTempDirectory);

            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = "ssh-keygen";
            p.StartInfo.Arguments = $"-q -b 3072 -C \"{Username}@scalable-teaching-sdu\" -f {userTempDirectory}/id_rsa_{Username} -m PEM -t RSA -N \"\"";
            p.Start();
            await p.WaitForExitAsync();
            var privateKey = await File.ReadAllTextAsync($"{userTempDirectory}/id_rsa_{Username}");
            var publicKey = await File.ReadAllTextAsync($"{userTempDirectory}/id_rsa_{Username}.pub");
            Directory.Delete(userTempDirectory, true);
            return (privateKey, publicKey);
        }
    }
}
