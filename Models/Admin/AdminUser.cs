

using System.ComponentModel.DataAnnotations;

namespace Models.Admin
{
    public class AdminUser : BaseUser
    {
        public bool IsOwner { get; set; } = false;
    }

}