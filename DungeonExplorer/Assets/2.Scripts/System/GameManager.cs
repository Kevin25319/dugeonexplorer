using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 플레이에 관련된 것들을 관리하는 싱글톤 클래스입니다. 
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance = null; // 싱글톤 클래스 인스턴스

    // 플레이어가 사망했을 경우 호출하는 델리게이트
    public delegate void PlayerDieEventHandler();
    public event PlayerDieEventHandler PlayerDieEvent = null;

    /// <summary>
    /// 게임 상태를 나열한 열거형입니다.
    /// </summary>
    public enum GameState
    {
        Title,
        Play,
        MenuOpen
    }
    public GameState currentGameState = GameState.Play;     // 현재 게임 상태

    [HideInInspector] public bool firstStart = true;        // 게임 데이터를 생성한 후 처음 시작한 것인지 체크
    [HideInInspector] public Vector2 playerStartPos;        // 플레이어의 게임 시작 위치
    [HideInInspector] public float playerStartlocalScaleX;  // 플레이어가 게임이 시작되고 나서 바라보는 방향
    
    [HideInInspector] public int playerCurrentHealth;       // 플레이어의 현재 체력
    [HideInInspector] public int playerCurrentDrivingForce; // 플레이어의 현재 원동력

    [HideInInspector] public string resurrectionScene;      // 플레이어가 사망했을 경우 부활할 씬
    [HideInInspector] public Vector2 playerResurrectionPos; // 플레이어가 사망했을 경우 부활할 좌표

    int gameDataNum = 1; // 현재 플레이 중인 게임 데이터의 번호

    int _prevPlayTime;  // 이전 게임 플레이 시간
    float _startTime;   // 게임을 실행한 시간(플레이 시간을 계산할 때 사용)

    bool _isStarted = true; // 게임이 현재 시작된 상태인지 체크

    void Awake()
    {
        // 싱글톤 클래스로 설정
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        Cursor.visible = false; // 마우스 커서가 보이지 않게 설정

        EnemyDataManager.Init();
        SceneManager.sceneLoaded += OnSceneLoaded;

        OptionsData.Init(); // 옵션 데이터 초기화

#if UNITY_EDITOR
        // 유니티 에디터에서 게임 플레이 씬을 실행했을 경우 1번 세이브 데이터를 불러옴
        if (currentGameState == GameState.Play)
        {
            GameLoad(1, false);
        }
#endif
    }

    /// <summary>
    /// 게임이 현재 시작된 상태인지 확인하는 메소드입니다.
    /// </summary>
    /// <returns>게임이 현재 시작됐는지 여부</returns>
    public bool IsStarted()
    {
        if (_isStarted)
        {
            // 게임이 시작된 상태라면 게임 시작 시간을 현재 시간으로 맞추고 게임이 시작된 상태를 false로 변경
            // 이후 true 반환
            _startTime = (float)DateTime.Now.TimeOfDay.TotalSeconds;
            _isStarted = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 현재 게임 상태를 설정하는 메소드입니다.
    /// 메뉴를 오픈하면 게임의 시간이 멈추며, 게임 플레이로 변경하면 시간 정지가 취소됩니다.
    /// </summary>
    /// <param name="newGameState">변경하려는 게임 상태</param>
    public void SetGameState(GameState newGameState)
    {
        switch(newGameState)
        {
            case GameState.Play:
                ScreenEffect.instance.TimeStopCancle();
                break;
            case GameState.MenuOpen:
                ScreenEffect.instance.TimeStopStart();
                break;
            case GameState.Title:
                break;
        }

        currentGameState = newGameState;
    }

    /// <summary>
    /// 게임의 플레이 시간(int)을 시각적인 텍스트로 가져오기 위한 메소드입니다.
    /// </summary>
    /// <param name="time">플레이 시간(int)</param>
    /// <returns>플레이 시간을 텍스트로 변환한 string 객체</returns>
    public string IntToTimeText(int time)
    {
        int sec = (time % 60);
        int min = ((time / 60) % 60);
        int hour = (time / 3600);

        string secToStr = sec < 10 ? "0" + sec.ToString() : sec.ToString();
        string minToStr = min < 10 ? "0" + min.ToString() : min.ToString();
        string hourToStr = hour.ToString();

        return string.Format("{0}:{1}:{2}", hourToStr, minToStr, secToStr);
    }

    /// <summary>
    /// 플레이어의 사망을 처리하는 메소드입니다.
    /// </summary>
    public void HandlePlayerDeath()
    {
        playerStartPos = playerResurrectionPos;
        DeadEnemyManager.ClearDeadEnemies();
        PlayerDieEvent?.Invoke();
    }

    /// <summary>
    /// 게임 진행을 저장하는 메소드입니다.
    /// </summary>
    public void GameSave()
    {
        string sceneName = resurrectionScene; // 플레이어 캐릭터가 부활하는 씬(체크 포인트)을 게임 시작 위치로 설정
        List<string> seenTutorials = TutorialManager.GetSeenTutorialsToString();
        List<string> deadBosses = DeadEnemyManager.GetDeadBosses();
        List<Vector2> foundMaps = MapManager.GetDiscoveredMaps();
        int playTime = _prevPlayTime + Mathf.CeilToInt((float)DateTime.Now.TimeOfDay.TotalSeconds - _startTime);    // 게임 플레이 타임 계산

        var saveData = new GameSaveData
        (
            playTime,
            sceneName,
            playerResurrectionPos,
            PlayerLearnedSkills.hasLearnedClimbingWall,
            PlayerLearnedSkills.hasLearnedDoubleJump,
            foundMaps,
            deadBosses,
            seenTutorials
        );
        SaveSystem.GameSave(saveData, gameDataNum);
    }

    /// <summary>
    /// tải dữ liệu chơi trò chơi đã lưu.
    /// </summary>
    /// <param name="gameDataNum">Số dữ liệu trò chơi mà b muốn tải</param>
    /// <param name="sceneLoaded">Có tải cảnh hay không</param>
    public void GameLoad(int gameDataNum, bool sceneLoaded = true)
    {
        var saveData = SaveSystem.GameLoad(gameDataNum);
        this.gameDataNum = gameDataNum;

        if(saveData == null)
        {
            // Nếu dữ liệu lưu bạn muốn tải không tồn tại, điều đó được xác định rằng trò chơi đã được bắt đầu lần đầu tiên và bản đồ đầu tiên được tải và trả lại
            SceneTransition.instance.LoadScene("OldMachineRoom_A");
            return;
        }
        _prevPlayTime = saveData.playTime;  // Đặt thời gian chơi lưu dữ liệu về thời gian chơi trò chơi trước đó

        resurrectionScene = saveData.sceneName;     // Đặt cảnh người chơi hồi sinh khi chết thành cảnh bạn muốn tải từ bản lưu
        playerResurrectionPos = saveData.playerPos; // Đặt tọa độ hồi sinh người chơi

        // Nhận bản đồ được khám phá
        MapManager.AddDiscoveredMaps(saveData.foundMaps);
        // Nhập các kỹ năng đã học
        PlayerLearnedSkills.hasLearnedClimbingWall = saveData.hasLearnedClimbingWall;
        PlayerLearnedSkills.hasLearnedDoubleJump = saveData.hasLearnedDoubleJump;

        // Thêm danh sách trùm chết
        for (int i = 0; i < saveData.deadBosses.Count; i++)
        {
            DeadEnemyManager.AddDeadBoss(saveData.deadBosses[i]);
        }

        // Thêm danh sách các hướng dẫn đã xem
        for (int i = 0; i < saveData.seenTutorials.Count; i++)
        {
            TutorialManager.AddSeenTutorial(saveData.seenTutorials[i]);
        }

        firstStart = false; // xác định rằng đây không phải là lần đầu tiên trò chơi được bắt đầu sau khi tạo dữ liệu trò chơi

        if (sceneLoaded)
        {
            // tải cảnh khi sceneLoaded được đặt thành True
            playerStartPos = saveData.playerPos;
            SceneTransition.instance.LoadScene(saveData.sceneName);
        }
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Nếu trạng thái trò chơi hiện tại là màn hình tiêu đề, dữ liệu sẽ được khởi tạo.
        if (currentGameState == GameState.Title)
        {
            gameDataNum = 1;

            firstStart = true;

            playerStartPos = Vector2.zero;
            playerStartlocalScaleX = 0;

            playerCurrentHealth = 0;
            playerCurrentDrivingForce = 0;

            resurrectionScene = string.Empty;
            playerResurrectionPos = Vector2.zero;

            _prevPlayTime = 0;
            _startTime = 0;

            _isStarted = true;
        }
        // Đặt lại sự kiện người chơi chết khi cảnh thay đổi.
        PlayerDieEvent = null;
    }

    void OnApplicationQuit()
    {
        // Tự động lưu trò chơi khi cửa sổ trò chơi đóng lại
        if (instance.currentGameState != GameState.Title)
        {
            instance.GameSave();
        }
        OptionsData.OptionsSave();
    }
}
