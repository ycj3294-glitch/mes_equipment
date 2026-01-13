using System;
using System.Reflection;
using System.Threading.Tasks;

public class MachineSimulator
{
  private readonly ApiService _apiService;
  private readonly Random _random = new Random();

  public MachineSimulator (ApiService apiService)
  {
    _apiService = apiService; // 외부에서 만들어진 객체를 주입 받음
  }

  public async Task RunAsync()
  {
    Console.WriteLine($"설비 [{AppConfig.MachineId}] 가동을 시작합니다.");

    while (true)
    {
      Console.WriteLine("\n[Poller] 작업 지시를 확인 중......");
      var workOrder = await _apiService.PollWorkOrderAsync(); // 서버에 생산 지시가 있는지 확인

      if (workOrder != null) // 새로운 작업이 있음.
      {
        // 작업 수행 코드
        await ProcessWorkOrder(workOrder);
      }
      else
      {
        Console.WriteLine("[-] 현재 할당된 작업이 없습니다.");
      }
      await Task.Delay(AppConfig.PollingIntervalMs); // 5초 지연 이후 반복 수행
    } 

  }

  private async Task ProcessWorkOrder(WorkOrderDto order)
    {
      // 방어 코드: 완료된 작업이면 생산 금지
      if (order.Status == "COMPLETED")
      {
        Console.WriteLine("[Skip] 이미 완료된 작업입니다.");
        return;
      }

      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine($"[Active] 작업 수주: {order.ProductCode} (목표: {order.TargetQty})");
      Console.ResetColor();

      // 생산 공정 시물레이션 (2초)
      await Task.Delay(2000);

      // 95% 확률로 양품(OK), 5% 확률로 불량(NG)
      bool isSuccess = _random.NextDouble() > 0.05;

      var report = new ProductionReportDto
      {
        OrderId = order.Id,
        MachineId = AppConfig.MachineId,
        Result = isSuccess ? "OK" : "NG",
        DefectCode = isSuccess ? null : "ERR-102" // 예: 치수 불량
      };

      string reportStatus = await _apiService.ReportProductionAsync(report);

      if (reportStatus == "OK")
      {
          Console.ForegroundColor = isSuccess ? ConsoleColor.Green : ConsoleColor.Red;
          Console.WriteLine($"[Report] {order.ProductCode} 생산 완료: {report.Result}");
          Console.ResetColor();
      }
      else if (reportStatus == "SHORTAGE")
      {
          // ★ 자재 부족 시 비상 정지 로직
          Console.WriteLine();
          Console.BackgroundColor = ConsoleColor.Red;
          Console.ForegroundColor = ConsoleColor.White;
          Console.WriteLine(" [ALARM] 자재 재고가 부족합니다! 생산을 중단합니다. ");
          Console.WriteLine(" [Action] 자재 보충 후 시뮬레이터를 다시 실행하세요. ");
          Console.ResetColor();

          // 프로그램을 종료하거나 무한 대기 상태로 빠지게 하여 설비 가동을 멈춤
          Environment.Exit(0); 
      }
      else
      {
          // 서버 에러나 네트워크 오류 시
          Console.WriteLine($"[Warn] 보고 실패: {reportStatus}. 잠시 후 재시도합니다.");
      }
    }
  }
