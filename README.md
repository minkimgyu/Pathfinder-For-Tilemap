# 🗺️ Pathfinder For Tilemap

A* 알고리즘을 기반으로 개발된 Tilemap 기반 Pathfinder입니다.

<img width="512" height="512" src="https://github.com/user-attachments/assets/1ddae1a9-1e59-4cd7-8f3c-193550d4604d" alt="Main"/>

[사용 방법 섹션으로 이동](#-사용-방법)

# 🛠️ 개발 도구

Unity (C#)

# ✨ 성능
<img width="674" height="688" alt="image" src="https://github.com/user-attachments/assets/f076c585-1b72-4715-9d0a-8c5d7414140f" />
<img width="674" height="685" alt="image" src="https://github.com/user-attachments/assets/54410bb5-12fb-47cc-a02f-580754d96562" />

32 * 42 맵 기준
- TargetWeight가 1인 경우, 탐색 시간 0.64ms
- TargetWeight가 2인 경우, 탐색 시간 0.19ms

## 최적화 요소
1. Heap을 활용한 OpenList 적용
2. 주변 노드 캐싱을 통한 불필요한 탐색 횟수 줄임
3. Weighted Pathfinding을 활용한 최적화

# ✨ 주요 기능

## ✅ Grid 정보 렌더링 (이동 가능 여부, 노드 가중치)

에디터 상에서 각 노드의 상태를 직관적으로 확인할 수 있도록 정보를 시각화했습니다.

<img width="318" height="417" alt="image" src="https://github.com/user-attachments/assets/43836e96-7a48-4da3-b6a1-9a668fd5a683" />
<img width="508" height="507" alt="image" src="https://github.com/user-attachments/assets/00756c87-8d30-455c-8fd4-6481564eacec" />

좌: 노드 이동 가능 여부, 우: 노드 가중치

## ✅ 경로 탐색 정보 렌더링

에디터 상에서 경로 탐색 과정 중 방문한 노드와 목표까지의 경로를 직관적으로 확인할 수 있도록 정보를 시각화했습니다.

<img width="387" height="507" alt="image" src="https://github.com/user-attachments/assets/c98f7f41-dc99-4b1c-b59a-0b51061a1acb" />

노란색 노드: OpenList에 할당된 노드, 빨간색 노드: ClosedList에 할당된 노드

## ✅ 타겟 크기를 기반으로 한 길찾기 기능 제공 (1x1, 3x3 크기 지원)

유닛의 크기를 고려하여 타일 기준으로 1x1, 3x3 크기를 가진 유닛의 길찾기를 지원합니다.

<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/c5614a36-50ca-48a2-95a6-b7c09c6777e3" />
<img width="389" height="511" alt="image" src="https://github.com/user-attachments/assets/1d7a395f-c7ce-4240-8997-11fd5e1d71f0" />

좌: 1x1 탐색의 경우, 우: 3x3 탐색의 경우

## ✅ 가중치를 활용한 경로 탐색 구현

각 타일에 가중치를 부여하여 g  가중치를 조절합니다. 이를 통해 길찾기 알고리즘이 특정 경로를 선호하거나 피하도록 설정할 수 있습니다.
예를 들어, 늪지대 타일에 높은 가중치를 부여하면 유닛이 해당 지역을 우회하는 경로를 탐색하게 됩니다. 이를 통해 전략적인 유닛 이동을 구현할 수 있습니다.

`F = G * TerrainWeight + H`

<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/b3e69f9a-7ebf-459e-ba84-6ee7537e9c50" />
<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/dc00055e-184a-4917-9d88-f3206ebc17ca" />

파란색 노드: 선호 노드, 빨간색 노드: 기피 노드

그리고 h 가중치를 조절하여 목표 지향적으로 탐색할 수 있습니다. 이를 통해 목표 지점까지 탐색하는 노드를 줄일 수 있습니다.

`F = G + H * TargetWeight`

<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/0413e727-5a60-428a-873a-8d94bd647f79" />
<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/5157ec82-b0f7-474f-9b8a-434af0aada63" />

좌: 가중치가 1인 경우, 우: 가중치가 2인 경우

## ✅ Grid Generator를 이용한 Grid 정보 Scriptable Object 저장 기능

Tilemap 데이터를 기반으로 길찾기에 사용될 Grid 정보를 생성하고, 이를 Scriptable Object로 저장하여 관리할 수 있는 기능을 구현했습니다.

<img width="425" height="331" alt="image" src="https://github.com/user-attachments/assets/3ea8534b-5f73-4df5-8665-a5740e067609" />

## ✅ 지형 정보 변경 시 Grid 갱신 기능 구현

게임 플레이 중 벽이 파괴되는 등 타일의 이동 가능 상태가 변경될 경우, Rebuild 기능을 통해 Grid 정보를 실시간으로 업데이트할 수 있습니다.

<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/f9a93eeb-6d33-4a0f-bafe-794732ea4fae" />
<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/3413db97-27a2-4273-a95c-0a03fad4f9fc" />

Grid 갱신 전

<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/4bf03ad4-7bbb-4e02-ae6d-eed4ee934841" />
<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/8c2c4dfa-6d23-459a-a759-2e15eeb08f45" />

Grid 갱신 후

# ✨ 사용 방법

## ✅ 기본 세팅

<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/9d8becc2-15c5-40d5-ba19-eb2934d3c4e0" />

AStarPathGrid, AStarPathFinder 컴포넌트가 필요합니다. 

(AStarPathGridDrawer, AStarPathDrawer는 렌더링 목적으로 필요함)

<img width="388" height="325" alt="image" src="https://github.com/user-attachments/assets/ba66201a-cdf4-4f08-a342-4ce92f1a55b2" />

AStarPathGridGenerator 컴포넌트가 필요합니다. 

(AStarPathGridPreviewer는 렌더링 목적으로 필요함)

<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/573ea857-a08a-4dcd-a0cf-4fc7a0e8d4e1" />
<img width="388" height="512" alt="image" src="https://github.com/user-attachments/assets/e19a4476-2b85-4eab-b230-6df6c9da3566" />

Block 타일과 Floor 타일이 필요합니다. 

Floor 타일 영역에 존재하는 Block 타일이 놓인 위치를 기준으로 경로 탐색 시 노드의 이동 가능 여부를 결정합니다.

## ✅ Grid 생성 및 Scriptable Object 저장 방법
<img width="727" height="595" alt="image" src="https://github.com/user-attachments/assets/7373db95-fbbb-469d-ab45-d6a599c6d759" />
<img width="1094" height="620" alt="image" src="https://github.com/user-attachments/assets/474f713c-c9fb-45b2-9387-b7c2a37dcb28" />

1. 프로젝트 창에서 마우스 우클릭 -> Create - PathNode 클릭
2. 바닥 타일맵과 벽 타일맵을 AStarPathGridGenerator의 Wall Tile, Ground Tile에 드래그 앤 드랍
3. 생성한 노드를 AStarPathGridGenerator에 드래그 앤 드랍
4. ... 클릭 후 GenerateGrid 클릭

## ✅ Grid 정보, 경로 탐색 정보 렌더링 방법

Draw Type를 바꾸면서 렌더링 되는 정보를 바꿀 수 있습니다. 또한 Color 항목의 데이터를 바꾸며 보기 좋은 색으로 커스터마이징 할 수 있습니다.

Size 항목을 조절하여 렌더링 되는 노드 크기를 조절할 수 있습니다.

<img width="760" height="592" alt="image" src="https://github.com/user-attachments/assets/88a49800-d1f2-4e29-8c24-135aa92b8161" />

AStarPathGridPreviewer를 통해서 Scriptable Object에 저장된 Grid 정보 미리 보기 가능 (에디터 모드에서 가능)

<img width="574" height="618" alt="image" src="https://github.com/user-attachments/assets/5394da54-ebdb-4989-860a-71730c3c5aba" />

AStarPathGridDrawer를 통해서 AStarPathGrid의 Grid 정보를 바탕으로 미리 보기 가능 (플레이 모드에서 가능)

**AStarPathFinder의 Trace Path 항목과 AStarPathGridDrawer의 Draw Path 항목을 모두 체크해야 경로를 확인할 수 있습니다.**

**경로 확인이 필요 없는 경우, 최적화를 위해 Trace Path, Draw Path를 모두 꺼주십시오**

## ✅ Grid 갱신 방법

<img width="454" height="330" alt="image" src="https://github.com/user-attachments/assets/c686069f-496e-40b3-96fd-d6447cbced87" />

AStarPathGrid 클래스 내부의 RebuildGrid 함수를 호출하면 됩니다. 

**전체 범위를 갱신하려면 매개 변수를 넣지 않고 호출하고 일부 범위를 갱신하려면 매개 변수로 topLeft, bottomRight 인덱스를 넣어주면 됩니다.**


## ✅ 가중치 조절 방법
<img width="454" height="330" alt="image" src="https://github.com/user-attachments/assets/cf305b02-109e-451c-b07c-b784f6f76175" />

AStarPathGridGenerator 클래스 내부의 SetTerrainPenaltyBias 함수를 수정하면 됩니다. 

**노드의 TerrainWeight는 -1 ~ 1 범위로 값이 지정되고 경로 탐색 과정에서 -1이에 가까울수록 우선하고, 1에 가까울수록 기피하게 됩니다.**

