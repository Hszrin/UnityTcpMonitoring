# 실시간 설비 모니터링 및 TCP 통신 시스템

Unity 기반 설비 시뮬레이터와 WPF 클라이언트를 중앙 TCP 서버로 연결하여  
설비 상태와 생산 데이터를 실시간으로 주고받도록 구현한 프로젝트입니다.

실제 PLC나 생산 설비 대신 Unity를 활용해 가상의 생산설비를 구성하고,  
생산량 및 상태 정보를 서버로 전송하여 WPF 클라이언트에서 확인하고 제어할 수 있도록 구현했습니다.

---

## 프로젝트 개요

### 개발 목적

스마트팩토리 환경에서 사용되는

- 설비 데이터 수집
- 설비 상태 모니터링
- 생산량 동기화
- 원격 설비 제어
- 서버 기반 데이터 관리

구조를 직접 구현해보기 위해 제작한 프로젝트입니다.

실제 산업 설비 대신 Unity를 설비 시뮬레이터로 사용하여  
`설비 → 서버 → 클라이언트` 형태의 데이터 흐름을 구현했습니다.

---

## 시스템 구조

```text
┌────────────────────────────┐
│      Unity Simulator       │
│                            │
│ EquipmentData              │
│ MachineNetworkSender       │
└─────────────┬──────────────┘
              │
              │ TCP / JSON
              ▼
┌────────────────────────────┐
│     Central TCP Server     │
│                            │
│ ClientManager              │
│ UnityPacketHandler         │
│ WPFPacketHandler           │
│ PacketSender               │
└─────────────┬──────────────┘
              │
              │ TCP / JSON
              ▼
┌────────────────────────────┐
│         WPF Client         │
│                            │
│ NetworkService             │
│ PacketHandler              │
│ ViewModel                  │
└────────────────────────────┘

주요 기능
Unity 설비 시뮬레이터
- 생산 설비 동작 시뮬레이션
- 설비별 MachineId 관리
- 설비 가동 / 정지 상태 관리
- 생산량 증가 처리
- 생산 발생 시 서버에 데이터 전송
- 서버 명령을 통한 설비 제어
- 서버와 설비 초기 상태 동기화
TCP Server
- Unity / WPF 클라이언트 연결 관리
- 최초 연결 시 클라이언트 역할 구분
- JSON 기반 패킷 송수신
- Unity 생산 데이터 수신
- 생산량 DB 반영
- WPF 클라이언트에 설비 상태 전달
- WPF의 설비 제어 명령을 Unity로 전달
- Heartbeat 기반 연결 상태 확인
WPF Client
- 설비 목록 조회
- 생산 라인별 설비 관리
- 설비 상태 확인
- 생산량 실시간 갱신
- 설비 가동 / 정지 제어
- 서버와 초기 데이터 동기화
통신 방식
TCP 연결 이후 클라이언트는 최초 메시지를 통해 자신의 역할을 서버에 전달합니다.
Unity

또는
WPF

서버는 해당 정보를 기준으로 클라이언트를 구분하여 관리합니다.
Unity Client
→ 생산 데이터 송신
→ 설비 상태 송신

WPF Client
→ 설비 데이터 조회
→ 설비 제어 요청

생산 데이터 처리 흐름
Unity에서 생산이 발생하면 설비의 생산량을 증가시키고 서버로 데이터를 전송합니다.
Unity
  ↓
ProductionUpdate
  ↓
TCP Server
  ↓
Database Update
  ↓
WPF Client Refresh

예시 패킷:
{
  "Command": "ProductionUpdate",
  "Machine": {
    "Id": 1,
    "Status": "가동",
    "ProductionCount": 120
  }
}

설비 제어 흐름
WPF 클라이언트에서 설비 제어를 요청하면 서버가 해당 명령을 Unity로 전달합니다.
WPF Client
    ↓
ControlMachine
    ↓
TCP Server
    ↓
Unity Simulator
    ↓
설비 상태 변경

이를 통해 WPF 클라이언트에서 Unity 설비를 원격으로 가동 또는 정지할 수 있도록 구현했습니다.
주요 Packet Command
Command	설명
InitMachine	초기 설비 데이터 전달
InitLine	초기 생산 라인 데이터 전달
AddMachine	설비 추가
RemoveMachine	설비 삭제
RefreshMachine	설비 상태 갱신
ControlMachine	설비 가동 / 정지 제어
ProductionUpdate	Unity 생산 데이터 전송
Heartbeat	Unity 연결 상태 확인
ImportExcel	Excel 기반 데이터 처리


비동기 TCP 통신
Unity, Server, WPF 간 통신은 비동기 방식으로 구현했습니다.
StreamReader, StreamWriter를 이용하여 데이터를 송수신하며
패킷은 한 줄 단위 JSON 문자열 형태로 전달합니다.
Client
  ↓
WriteLineAsync()
  ↓
TCP Stream
  ↓
ReadLineAsync()
  ↓
Server

Unity 측에서는 SemaphoreSlim을 이용하여
동시에 여러 패킷이 송신될 때 Writer가 충돌하지 않도록 처리했습니다.
패킷 구조
서버는 기본적으로 다음과 같은 형태의 JSON 패킷을 사용합니다.
{
  "Command": "InitMachine",
  "Data": []
}

WPF에서는 패킷 종류에 따라 DTO를 분리하여 역직렬화하도록 구현했습니다.
예시:
public class MachineListPacket
{
    public string Command { get; set; }
    public List<Machine> Data { get; set; }
}

설비 상태 동기화
Unity 내부 생산량과 서버 DB의 생산량이 서로 달라지는 문제를 방지하기 위해
서버 데이터를 기준으로 초기 상태를 동기화하도록 구현했습니다.
Unity 연결
    ↓
서버에서 InitMachine 전달
    ↓
Unity EquipmentData 초기화
    ↓
이후 생산 발생
    ↓
ProductionUpdate 전송
    ↓
서버 DB 갱신

이를 통해 Unity와 서버의 생산 데이터를 동일하게 유지하도록 구성했습니다.
프로젝트 구조
Unity Simulator
├─ EquipmentData
├─ MachineNetworkSender
├─ MachineStatusPacket
└─ Machine Control

TCP Server
├─ ClientManager
├─ PacketSender
├─ UnityPacketHandler
├─ WPFPacketHandler
└─ Repository

WPF Client
├─ NetworkService
├─ ServerConnection
├─ PacketHandler
├─ PacketSender
├─ View
└─ ViewModel

Troubleshooting
1. UTF-8 BOM으로 인한 JSON 역직렬화 오류
문제
TCP 통신 도중 JSON 역직렬화 과정에서 다음 오류가 발생했습니다.
0xEF is an invalid start of a value

원인
StreamWriter가 UTF-8 BOM을 포함하여 데이터를 전송하면서
JSON 문자열 앞에 BOM 문자가 포함되었습니다.
해결
모든 TCP Writer에서 BOM 없는 UTF-8을 사용하도록 변경했습니다.
new UTF8Encoding(false)

이를 통해 JSON 역직렬화 오류를 해결했습니다.
2. 패킷 구조 불일치
문제
서버는 다음과 같은 Wrapper 구조로 데이터를 전송하고 있었습니다.
{
  "Command": "InitMachine",
  "Data": []
}

하지만 WPF에서 List<Machine>으로 직접 역직렬화를 시도하여 오류가 발생했습니다.
해결
서버 패킷 구조와 동일한 DTO를 정의했습니다.
public class MachineListPacket
{
    public string Command { get; set; }
    public List<Machine> Data { get; set; }
}

패킷 구조와 DTO 구조를 일치시켜 문제를 해결했습니다.
3. TCP 연결 종료 처리
문제
클라이언트 종료 또는 연결 해제 시 다음과 같은 오류가 발생했습니다.
Unable to read data from the transport connection

또는
현재 연결은 원격 호스트에 의해 강제로 끊겼습니다.

해결
- EndOfStream 검사
- 연결 종료 예외 처리
- 비동기 Read Loop 종료
- 연결 해제된 클라이언트 정리
를 적용하여 비정상 연결 종료 상황에서도 서버가 계속 동작하도록 개선했습니다.
4. Unity 생산량과 DB 생산량 불일치
문제
Unity에서는 초기 생산량이 0으로 시작하지만
DB에는 기존 생산량이 저장되어 있어 서로 다른 값을 표시하는 문제가 있었습니다.
해결
Unity 연결 시 서버가 InitMachine 패킷을 전송하도록 변경했습니다.
Unity는 서버에서 받은 설비 정보를 기준으로 초기 데이터를 설정하고,
이후 생산 발생 시 변경된 값을 다시 서버로 전송하도록 구성했습니다.
5. 비동기 패킷 송신 충돌
문제
동시에 여러 패킷을 송신할 경우 동일한 StreamWriter에
여러 작업이 접근할 가능성이 있었습니다.
해결
SemaphoreSlim을 사용하여 송신 작업을 직렬화했습니다.
await _sendLock.WaitAsync();

try
{
    await writer.WriteLineAsync(json);
    await writer.FlushAsync();
}
finally
{
    _sendLock.Release();
}

이를 통해 하나의 Writer에 동시에 접근하지 않도록 처리했습니다.
Tech Stack
Client
- Unity
- WPF
- C#
Server
- .NET
- TCP Socket
- JSON
Database
- Entity Framework Core
- SQLite
Architecture
- MVVM
- Repository Pattern
- Client / Server Architecture
실행 방법
Unity Simulator
Releases에서 Unity Windows Build 파일을 다운로드합니다.
압축 해제 후 실행 파일을 실행합니다.
UnityTcpMonitoring.exe

Unity Windows Build는 exe 단독이 아닌 전체 빌드 파일이 필요합니다.

예시:
UnityTcpMonitoring.exe
UnityTcpMonitoring_Data/
UnityPlayer.dll
MonoBleedingEdge/
UnityCrashHandler64.exe

Server
서버 프로그램을 실행합니다.
기본 TCP Port:
5000

WPF Client
WPF 클라이언트를 실행하면 중앙 서버에 연결됩니다.
Unity Simulator
        ↓
     TCP Server
        ↓
     WPF Client
     
Unity Simulator 조작법

Unity 시뮬레이터에서는 마우스를 이용하여 공장 내부 화면을 이동하고 확대 / 축소할 수 있습니다.

| 조작 | 기능 |
|---|---|
| 마우스 휠 버튼 드래그 | 화면 이동 |
| 마우스 휠 위 / 아래 | 화면 확대 / 축소 |

화면 이동

마우스 휠 버튼을 누른 상태에서 마우스를 움직이면  
공장 내부 화면을 자유롭게 이동할 수 있습니다.

```text
Mouse Wheel Click + Drag
        ↓
화면 이동

확대 / 축소
마우스 휠을 위 또는 아래로 돌려
공장 설비 화면을 확대하거나 축소할 수 있습니다.

Mouse Wheel Scroll
        ↓
Zoom In / Zoom Out


실행 화면
Unity 설비 시뮬레이터
<!-- Unity 실행 화면 이미지 추가 -->

![Unity Simulator](./Images/unity-simulator.png)

WPF 모니터링 화면
<!-- WPF 실행 화면 이미지 추가 -->

![WPF Client](./Images/wpf-client.png)

프로젝트를 통해 경험한 내용
- TCP 기반 Client / Server 통신 구조
- 비동기 네트워크 프로그래밍
- JSON 패킷 설계
- Unity와 WPF 간 데이터 통신
- 클라이언트 연결 관리
- 설비 상태 및 생산량 동기화
- Entity Framework Core를 이용한 데이터 관리
- 네트워크 예외 및 연결 종료 처리
- 패킷 송수신 과정에서 발생하는 인코딩 문제 해결
Repository 구성
본 Repository에서는 포트폴리오 확인을 위해
- 프로젝트 설명
- 실행 화면
- 핵심 코드
- Windows 실행 Build
를 제공합니다.
전체 Unity 프로젝트 소스는 포함하지 않으며,
통신 구조를 확인할 수 있는 주요 코드만 일부 포함합니다.
Download
Windows 실행 파일은 GitHub Releases에서 다운로드할 수 있습니다.
압축을 해제한 뒤 .exe 파일을 실행해주세요.