using AppTiemposV3.SharedClases.DTOs.Reports;
using NanoidDotNet;
using System.ComponentModel.DataAnnotations;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets.SubAlphabets;

namespace AppTiemposV3.Api.Entities
{
    public class ReportEntity : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [StringLength(36)]
        public string UrlIdentificator { get; set; } = Generate($"{NoLookAlikeSafeLettersLowercase}{NoLookAlikeSafeDigits}", 36);
        [Required]
        public string Description { get; set; } = string.Empty;
        public string? TableBase { get; set; } = string.Empty;
        [Required]
        public string ReportMode { get; set; } = string.Empty;
        [StringLength(10240000)]
        public string? QueryRaw { get; set; } = string.Empty;
        public QueryRequestDTO? QueryRequest { get; set; } = null;
        public ScheduleReportDto Schedule { get; set; } = null!;
        public DateTime LastRun { get; set; } = DateTime.Now;
        public int RunCount { get; set; } = 0;
        public bool IsFavorite { get; set; } = false;
        public bool IsScheduled { get; set; } = false;
        [Required] public required Guid UserId { get; set; }
        public UserEntity User { get; set; } = null!;
    }
}
