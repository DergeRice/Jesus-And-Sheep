# 영어 전환 및 서버 이전 준비 검증 — 2026-09-16

## 완료한 수정

- 영어 UI 문구 103개 검토: 튜토리얼, 확률 보상, 양/체력 오역, 속도 구매 설명, 토스트, 버튼 문구. 이벤트 SO의 영어 설명도 동기화했습니다.
- 저장한 언어가 씬 이동 때 기본 locale로 덮이지 않도록 수정했습니다. 동적 숫자/설명/재사용 UI는 마지막 표시값을 추적하며 닉네임과 입력란은 번역에서 제외합니다.
- 광고 잔여 횟수와 받은 하트 메시지를 포맷 번역으로 연결했습니다. 하트 받기 버튼은 요청 중 중복 클릭을 막고 실패하면 다시 활성화합니다.
- 영어 자동 크기 조절/줄바꿈, 선택지 여백, Grace 제목, 속도 표시의 자동 크기를 조정했습니다.
- 하트 감소에 음수가 전달되어 1개만 차감되던 오류, 0개 변경 시 차감 오류, 실제 목록과 별도 ballCount 필드 간 불일치를 수정했습니다. 마지막 발사 가능한 하트는 유지합니다.
- 이벤트 02의 보상/턴 수 뒤바뀜(15개/6턴), 이벤트 16의 차감 대신 회복 효과, 이벤트 17의 5개 대신 1개 차감을 설명에 맞췄습니다. 특수 하트 확률 보상은 실제 지급되는 3개로 설명을 수정했습니다.
- 이벤트 27의 손상된 YAML을 복구했습니다. 선택지가 비어 있을 때 턴이 잠기는 경우와 건너뛰는 턴 사이의 입력을 보완했습니다.
- 무작위 양 선택 후 삭제한 인덱스를 다시 참조하는 예외, 소환 가능한 행이 가득 찼을 때 무한 탐색하는 문제를 수정했습니다.
- 네트워크 요청에 타임아웃/해제 처리를 추가했습니다. 연결 실패·잘못된 버전 응답은 오프라인으로 처리하고, 다른 JSON 필드의 true가 버전 검사 결과로 오인되지 않게 했습니다.
- 서버 주소는 Resources/Network/server.json에서 덮어쓸 수 있습니다. 빈 값은 기존 씬의 주소를 유지합니다. OCI용 Terraform은 infra/oracle에 있습니다.

## 실행한 검사

Unity 6000.4.5f1 C# 컴파일 및 `ReleaseRegressionChecks.Run`: **131 assertions 통과**.
4개 씬을 로드하고 이벤트 에셋/참조, 언어 전환 시 숫자 보존, 포맷 번역, 하트 비용, 단일 양 무작위 선택, 소환 공간 포화, 서버 응답 파싱을 검사했습니다.
로비 91개 + 게임 19개의 번역 TMP 라벨을 검사했습니다. 로비/게임/설정/가장 긴 선택지 설명을 720×1280 및 720×1560으로 렌더링했고 최종 보고서에 overflow 경고가 없습니다. 설정 팝업은 720×1280에서 렌더링했습니다.

렌더링은 **에디터 씬 미리보기**입니다. 게임 Start/Awake 전체를 실행한 플레이 테스트가 아닙니다. 19.5:9 미리보기에는 기존 ScreenSizeController의 카메라 보정을 적용했습니다. 화면의 닉네임·점수·하트 등은 씬에 저장된 예시값이며 실제 계정 상태가 아닙니다.

기존 서버에 대한 상태 확인:

- GET `/api2/check?nickname=codexavailabilityprobe` → HTTP 200, `Nickname is available.`
- POST `/api2/versionCheck`, version=2.74 → HTTP 200, `{"isCompatible":true}`
- 회원 생성/점수 변경/하트 발송·수령 요청은 실행하지 않았습니다.

Terraform `init -backend=false`, `fmt -check`, `validate` 통과. 계정 인증이 필요한 `plan`/`apply`는 실행하지 않았습니다.

## 남은 검증/이전

- Android/iOS 실기기 전체 플레이, 광고·결제, 저장 후 재실행, 노치 safe area, 장시간 플레이.
- 원본 서버 코드/DB/예약 작업 확인, OCI 계정의 무료 할당량·ARM 호환성 확인, 데이터 이전, 스테이징 검증, DNS 전환.
- 기존 서버 주소를 계속 사용 중입니다. Terraform 파일 생성만으로 트래픽이 이전되지는 않습니다.

## 재현

저장소 루트에서, 이 프로젝트를 다른 Unity 인스턴스가 열지 않은 상태로 실행합니다. 테스트는 씬을 저장하지 않으며 일반 에디터 실행은 거부합니다.

```sh
/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath "$PWD" \
  -executeMethod ReleaseRegressionChecks.Run -quit \
  -logFile /tmp/jesus-sheep-qa.log
terraform -chdir=infra/oracle init -backend=false
terraform -chdir=infra/oracle fmt -check
terraform -chdir=infra/oracle validate
```

결과: `release-checks.txt`, `*-english-1280.png`, `*-english-1560.png`.
