using System.Text.Json.Serialization;
// 생산 결과를 보고하는 DTO (backend 서버와 통신)
  public class ProductionReportDto
  {
      [JsonPropertyName("orderId")]
      public long OrderId { get; set; }
      [JsonPropertyName("machineId")]
      public string? MachineId { get; set; }
      [JsonPropertyName("result")]
      public string? Result { get; set; } 
      [JsonPropertyName("defectCode")]
      public string? DefectCode { get; set; }
  }