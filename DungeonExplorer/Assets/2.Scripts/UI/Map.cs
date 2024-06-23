using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <tóm>
/// Đây là lớp quản lý bản đồ và chịu trách nhiệm cho phép người chơi tiết lộ bản đồ.
/// </tóm tắt>
public static class MapManager
{
    // Danh sách tọa độ bản đồ người chơi đã tìm thấy
    static List<Vector2> discoveredMapPositions = new List<Vector2>();

    // Xóa tất cả bản đồ được người chơi tìm thấy.
    public static void ClearDiscoveredMaps()
    {
        discoveredMapPositions.Clear();
    }

    /// <summary>
    /// thêm bản đồ mà người chơi tìm thấy vào danh sách..
    /// </summary>
    /// <param name="mapImage">Hình ảnh ô trên bản đồ được người chơi tìm thấy</param>
    public static void AddDiscoveredMap(Image mapImage)
    {
        discoveredMapPositions.Add(mapImage.rectTransform.anchoredPosition);
    }

    /// <summary>
    /// phương pháp thêm tất cả các bản đồ mà người chơi tìm thấy cùng một lúc bằng cách sử dụng các giá trị tọa độ.
    /// </summary>
    /// <param name="mapPos">Danh sách tọa độ bản đồ</param>
    public static void AddDiscoveredMaps(List<Vector2> mapPositions)
    {
        discoveredMapPositions.AddRange(mapPositions);
    }

    /// <summary>
    /// lấy tọa độ danh sách bản đồ được phát hiện
    /// </summary>
    /// <returns>Tọa độ của danh sách bản đồ được phát hiện</returns>
    public static List<Vector2> GetDiscoveredMaps() => discoveredMapPositions;
}

/// <summary>
/// lớp hiển thị menu cần điều khiển và cách thao tác trong phần mô tả của menu UI.
/// </summary>
[System.Serializable]
public class MenuControlAndKey
{
    public GameInputManager.MenuControl[] menuControl;  // Danh sách điều khiển menu để chỉ ra các phím hoặc nút điều khiển menu
    public string menuKey;                                  // key có tên của menu khi muốn điều khiển, lấy giá trị khóa từ dữ liệu ngôn ngữ
}

/// <summary>
/// phương pháp xử lý các chức năng của giao diện người dùng bản đồ.
/// </summary>
public class Map : MonoBehaviour
{
    const int MapTileSpacing = 8;   // Khoảng cách của các ô bản đồ
    const int MaxZoomLevel = 3;  // mức thu phóng bản đồ tối đa
    const int MinZoomLevel = 1;  // Mức thu phóng bản đồ tối thiểu

    [SerializeField] TextMeshProUGUI _mapTitleText;      // Văn bản tiêu đề bản đồ để hỗ trợ ngôn ngữ
    [SerializeField] TextMeshProUGUI _manualText;   // Văn bản hiển thị hướng dẫn vận hành
    [SerializeField] RectTransform _mapTileContainer;        // Đối tượng trò chơi chứa danh sách các ô bản đồ
    [SerializeField] RectTransform _playerIcon;     // Biểu tượng cho biết vị trí của người chơi
    [SerializeField] List<Image> _mapTiles;         // Danh sách các ô bản đồ được hiển thị trên giao diện người dùng
    [SerializeField] MenuControlAndKey[] _menuControlAndKey;    // Danh sách các lớp hiển thị menu bạn muốn điều khiển và cách thao tác trong phần mô tả của menu UI.

    int _mapZoomLevel = 1;       // Mức thu phóng bản đồ hiện tại

    Vector2 _mapOriginPos = Vector2.zero; // nguồn gốc của bản đồ
    Camera _camera;
    Transform _cameraTransform;

    void Awake()
    {
        transform.GetComponent<PauseScreen>().MapOpend += OnMapOpend;

        _camera = Camera.main;
        _cameraTransform = _camera.GetComponent<Transform>();

        // Vô hiệu hóa tất cả bản đồ
        foreach (var mapTile in _mapTiles)
        {
            mapTile.gameObject.SetActive(false);
        }

        // Lấy danh sách bản đồ mà người chơi đã tìm thấy cho đến nay và kích hoạt các bản đồ đó
        foreach (Vector2 mapPos in MapManager.GetDiscoveredMaps())
        {
            Image mapImage = _mapTiles.Find(x => x.rectTransform.anchoredPosition == mapPos);
            mapImage?.gameObject.SetActive(true);
        }
        _mapOriginPos = _mapTileContainer.anchoredPosition;
    }

    void Update()
    {
        UpdatePlayerIcon();
        DiscoverMap();
        MoveMap();
        ZoomMap();
    }

    /// <summary>
    /// hiển thị vị trí hiện tại của người chơi trên bản đồ theo thời gian thực.
    /// </summary>
    void UpdatePlayerIcon()
    {
        float cameraHalfSizeY = _camera.orthographicSize;
        float cameraHalfSizeX = cameraHalfSizeY * _camera.aspect;

        float xPos = (_cameraTransform.position.x + cameraHalfSizeX) / (cameraHalfSizeX * 2);
        float yPos = (_cameraTransform.position.y + cameraHalfSizeY) / (cameraHalfSizeY * 2);

        int xPosIndex = Mathf.FloorToInt(xPos) * MapTileSpacing;
        int yPosIndex = Mathf.FloorToInt(yPos) * MapTileSpacing;

        _playerIcon.anchoredPosition = new Vector2(xPosIndex, yPosIndex);
    }

    /// <summary>
    /// thêm bản đồ mà người chơi hiện đang ở vào trình quản lý bản đồ dưới dạng bản đồ được tìm thấy.
    /// </summary>
    void DiscoverMap()
    {
        // Tìm và truy xuất ô bản đồ ở cùng vị trí với biểu tượng của người chơi trong danh sách ô bản đồ.
        // Nếu bản đồ chưa được kích hoạt, hãy kích hoạt bản đồ rồi thêm bản đồ vào danh sách người quản lý bản đồ.
        Image mapImage = _mapTiles.Find(x => x.rectTransform.anchoredPosition == _playerIcon.anchoredPosition);
        if (!mapImage.gameObject.activeSelf)
        {
            mapImage.gameObject.SetActive(true);
            MapManager.AddDiscoveredMap(mapImage);
        }
    }

    /// <summary>
    /// Lớp này di chuyển bản đồ lên, xuống, trái và phải bằng cách sử dụng thông tin đầu vào của người chơi.
    /// </summary>
    void MoveMap()
    {
        float yMove = GameInputManager.MenuInput(GameInputManager.MenuControl.Up) ? 1 :
                      GameInputManager.MenuInput(GameInputManager.MenuControl.Down) ? -1 :
                      0;
        float xMove = GameInputManager.MenuInput(GameInputManager.MenuControl.Right) ? 1 :
                      GameInputManager.MenuInput(GameInputManager.MenuControl.Left) ? -1 :
                      0;

        // Di chuyển bản đồ dựa trên đầu vào
        _mapTileContainer.Translate(new Vector2(xMove, yMove) * 10f * Time.unscaledDeltaTime);

        // Giới hạn phạm vi di chuyển trên bản đồ
        _mapTileContainer.anchoredPosition = new Vector2(Mathf.Clamp(_mapTileContainer.anchoredPosition.x, -50f, 20f), 
                                                         Mathf.Clamp(_mapTileContainer.anchoredPosition.y, -20f, 20f));
    }

    /// <summary>
    /// phương pháp để phóng to kích thước của bản đồ. Nếu kích thước bản đồ vượt quá kích thước tối đa, nó sẽ trở về kích thước tối thiểu.
    /// </summary>
    void ZoomMap()
    {
        if (GameInputManager.MenuInputDown(GameInputManager.MenuControl.Select))
        {
            if (_mapZoomLevel < MaxZoomLevel)
            {
                _mapTileContainer.localScale += Vector3.one;
                _mapZoomLevel++;
            }
            else
            {
                _mapTileContainer.localScale = Vector3.one;
                _mapZoomLevel = MinZoomLevel;
            }
        }
    }

    /// <summary>
    /// Phương thức khởi tạo được gọi khi bản đồ được thực thi
    /// </summary>
    public void OnMapOpend()
    {
        _mapTitleText.text = LanguageManager.GetText("Map");
        SetManualText();

        _mapZoomLevel = 1;
        _mapTileContainer.localScale = Vector3.one;
        
        _mapTileContainer.anchoredPosition = _mapOriginPos - _playerIcon.anchoredPosition;
    }

    /// <summary>
    /// Một phương pháp đặt văn bản mô tả cách kiểm soát bản đồ.
    /// </summary>
    void SetManualText()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < _menuControlAndKey.Length; i++)
        {

            for (int j = 0; j < _menuControlAndKey[i].menuControl.Length; j++)
            {
                // Thêm phím hoặc nút điều khiển menu vào chuỗi rồi thêm nó vào trình tạo chuỗi.
                string menuControlToText;
                if (GameInputManager.usingController)
                {
                    menuControlToText = GameInputManager.MenuControlToButtonText(_menuControlAndKey[i].menuControl[j]);
                }
                else
                {
                    menuControlToText = GameInputManager.MenuControlToKeyText(_menuControlAndKey[i].menuControl[j]);
                }
                sb.AppendFormat("[ <color=#ffaa5e>{0}</color> ] ", menuControlToText);
            }
            // Sử dụng phím để lấy tên của điều khiển menu bạn muốn điều khiển.
            string keyToName = LanguageManager.GetText(_menuControlAndKey[i].menuKey);
            sb.Append(keyToName);

            // Thêm khoảng trắng nếu tôi không phải là chỉ mục cuối cùng
            if (i < _menuControlAndKey.Length - 1)
            {
                sb.Append("  ");
            }
        }
        _manualText.text = sb.ToString();
    }
}