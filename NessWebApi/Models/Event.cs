using MessagePack;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace NessWebApi.Models
{
    public class Event
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }

        public string Location { get; set; }

        public string Author { get;set; }

        public string ImageUrl { get; set; }

        public DateTime StartDateTime { get; set;}

        public DateTime EndDateTime { get; set; }

        public float DurationHours { get; set; }

        public string Address { get; set; }

        public string eventLink { get; set; }

        public string ticketLink { get; set; }

        public string createdBy { get; set; }

        public bool isPetFriendly { get; set; }

        public bool isKidFriendly { get; set; }

        public bool isFree { get; set; }  
        public bool withTicket { get; set; }    

        public bool isDraft { get; set; }

        public bool isFavorite { get; set; }
    }
}
