using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UnityTcpMonitoringShared.Models
{
    public partial class Line : ObservableValidator
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ObservableProperty]
        [StringLength(50)]
        private string _name = string.Empty;

        // 시간당 목표 생산량
        [ObservableProperty]
        private int _targetProductionRate; 
        
        [Required]
        [ObservableProperty]
        [StringLength(20)]
        private string _status = "정지";
    }
}