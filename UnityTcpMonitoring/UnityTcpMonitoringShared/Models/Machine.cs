using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UnityTcpMonitoringShared.Models
{
    public partial class Machine : ObservableValidator
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ObservableProperty]
        [StringLength(50)]
        private string _name = string.Empty;

        [Required]
        [ObservableProperty]
        [StringLength(20)]
        private string _status = "정지";

        [ObservableProperty]
        private int _productionCount;

        [ObservableProperty]
        [StringLength(20)]
        private string _lastProductionTime = "";


        public int LineId { get; set; }
        public Line? Line { get; set; }
    }
}