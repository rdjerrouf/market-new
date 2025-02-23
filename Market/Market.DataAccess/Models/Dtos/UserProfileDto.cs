using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Market.DataAccess.Models.Dtos
{
    public class UserProfileDto
    {
        public int Id { get; set; }

        public string? Email { get; set; }

        public string? DisplayName { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? PhoneNumber { get; set; }

        public string? City { get; set; }

        public string? Province { get; set; }

        public int PostedItemsCount { get; set; }

        public int FavoriteItemsCount { get; set; }
    }

}
