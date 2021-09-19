using ScalableTeaching.Models;

namespace ScalableTeaching.DTO
{
    public class EducatorReturnDTO
    {
        public string Username { get; set; }
        public string Surname { get; set; }
        public string GeneralName { get; set; }
        public string Mail { get; set; }

        public static explicit operator EducatorReturnDTO(User user)
        {
            return new()
            {
                Username = user.Username,
                GeneralName = user.GeneralName,
                Surname = user.Surname,
                Mail = user.Mail
            };
        }
    }
}
