using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class LoTrinh
    {
       
        public string LoTrinhId { get; set; }

    
        public string TramDi { get; set; }

     
        public string TramToi { get; set; }

    
        public decimal? GiaVeCoDinh { get; set; }

    
        public Tram TramDiNavigation { get; set; }

     
        public Tram TramToiNavigation { get; set; }
    }
}
