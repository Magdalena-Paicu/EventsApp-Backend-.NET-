using MessagePack;
using System.ComponentModel.DataAnnotations.Schema;

namespace NessWebApi.Models
{
    public class UserFavoriteEvents
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserFavoriteEventsId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }
    }
}
