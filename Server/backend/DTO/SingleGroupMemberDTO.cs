using System;

namespace ScalableTeaching.DTO
{
    public class SingleGroupMemberDTO
    {
        public string UserUsername { get; set; }
        public Guid GroupID { get; set; }

        public bool Validate()
        {
            return(GroupID != Guid.Empty || UserUsername is not null || UserUsername.Length != 0);
        }
    }
}