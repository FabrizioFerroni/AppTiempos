using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.Reports
{
    public class QueryRequestDTO
    {
        public string BaseTable { get; set; } = string.Empty;
        public List<SelectedFieldDTO> Fields { get; set; } = new();
        public List<JoinDTO> Joins { get; set; } = new();
        public List<FilterDTO> Filters { get; set; } = new();
        public List<MetricDTO> Metrics { get; set; } = new();
        public List<GroupFieldDTO> GroupBy { get; set; } = new();
    }

    public record SelectedFieldDTO(string Table, string Field, string? Alias);
    public record JoinDTO(string Table, string ParentTable, string Type, string Field, string TargetField);
    public record FilterDTO(string Table, string Field, string Operator, string Value);
    public record MetricDTO(string Table, string Field, string Aggregation, string? Label);
    public record GroupFieldDTO(string Table, string Field);
}
