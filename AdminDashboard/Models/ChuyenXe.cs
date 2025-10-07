using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public class ChuyenXe
    {
      
     
        public string ChuyenId { get; set; }

      
        public string LoTrinhId { get; set; }

    
        public string XeId { get; set; }

     
        public DateTime NgayDi { get; set; }

     
        public TimeSpan GioDi { get; set; }

   
        public TimeSpan GioDenDuKien { get; set; }

       
        public string TrangThai { get; set; }

      
        public LoTrinh LoTrinh { get; set; }

       
        public Xe Xe { get; set; }

    }
}
