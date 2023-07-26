using MessagePack;
using System.ComponentModel.DataAnnotations.Schema;

namespace NessWebApi.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string Location { get; set; }

        public string Author { get;set; }

        public string ImageUrl { get; set; }

        public DateTime StartDateTime { get; set;}

        public DateTime EndDateTime { get; set; }

        public int DurationHours { get; set; }

        public string Address { get; set; }

        public string eventLink { get; set; }

        public string ticketLink { get; set; }

        public string createdBy { get; set; }

        public string isPetFriendly { get; set; }

        public string isKidFriendly { get; set; }

        public string isFree { get; set; }  

        public string isDraft { get; set; }

        public string isFavorite { get; set; }


    }
}
