using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class TcpClientService
{
  private TcpClient _client; // 연결 관련
  private NetworkStream _stream; // 주고 받는 데이터

  // 현재 연결 상태 확인
  public bool IsConnected => _client != null && _client.Connected;

  // L1 장비(서버)에 접속
  public async Task ConnectAsync(String ip, int port)
  {
    _client = new TcpClient();
    await _client.ConnectAsync(ip, port);
    _stream = _client.GetStream();
  }

  // 지정된 바이트 수만큼 데이터를 읽을 때까지 대기
  public async Task<byte[]> ReadPacketAsync(int size)
  {
    if (!IsConnected) return null;

    try
    {
      byte[] buffer = new byte[size];
      int totalRead = 0;

      while(totalRead < size)
      {
        int read = await _stream.ReadAsync(buffer, totalRead, size - totalRead);
        if (read == 0) return null;
        totalRead += read;
      }
      return buffer;
    }
    catch
    {
      return null;
    }
  }

  public void Disconnect()
  {
    _stream?.Close(); // 자원 반납
    _client?.Close(); // 자원 반납
  }
}
