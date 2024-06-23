using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// lớp để lưu trữ dữ liệu nhóm đối tượng.
/// </summary>
[System.Serializable]
public class PoolObjectData
{
    public string key;          // key
    public GameObject prefab;   // Prefab đối tượng trò chơi
    public int count = 5;       // Số lần tạo ban đầu
}

/// <summary>
/// lớp đơn chứa nhóm đối tượng và xử lý các hàm liên quan đến nhóm đối tượng.
/// Không giống như các lớp trình quản lý đơn lẻ khác, nó chỉ được duy trì trong bối cảnh đặt đối tượng.
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager instance = null;    // lớp đơn

    [SerializeField] List<PoolObjectData> _poolObjectDataList;  // Danh sách dữ liệu đối tượng nhóm để khởi tạo nhóm đối tượng

    Dictionary<GameObject, string> _keyByPoolObject;        // Từ điển tìm key với đầy đủ đối tượng
    Dictionary<string, GameObject> _keyBySampleObject;      // Nếu ngăn xếp của đối tượng nhóm trống, nó sẽ được sao chép từ từ điển tương ứng.
    Dictionary<string, Stack<GameObject>> _poolObjectByKey; // Từ điển tìm các đối tượng nhóm được lưu trữ trên ngăn xếp theo key.

    void Awake()
    {
        instance = this; // lớp đơn

        _keyByPoolObject = new Dictionary<GameObject, string>();
        _keyBySampleObject = new Dictionary<string, GameObject>();
        _poolObjectByKey = new Dictionary<string, Stack<GameObject>>();
        for(int i = 0; i < _poolObjectDataList.Count; i++)
        {
            // Tạo nhóm đối tượng
            CreatePool(_poolObjectDataList[i]);
        }
    }

    /// <summary>
    /// phương pháp để tạo ra một nhóm đối tượng.
    /// </summary>
    /// <param name="data">Dữ liệu trong nhóm đối tượng muốn tạo</param>
    public void CreatePool(PoolObjectData data)
    {
        string parentObjectName = "Pool <" + data.key + ">";
        var parentObject = GameObject.Find(parentObjectName); // Tìm cha mẹ của một đối tượng nhóm

        GameObject newGameObject = null;
        if(_poolObjectByKey.ContainsKey(data.key))
        {
            // Nếu đối tượng trò chơi đã tồn tại, hãy chèn đối tượng mới vào nhóm đối tượng đã tạo trước đó và chấm dứt thực thi.
#if UNITY_EDITOR
            Debug.Log(data.key + "는 겹치는 키가 있어요");
#endif
            for (int i = 0; i < data.count; i++)
            {
                newGameObject = Instantiate(_keyBySampleObject[data.key], parentObject.transform);
                _keyByPoolObject.Add(newGameObject, data.key);
            }

            return;
        }

        // Tạo đối tượng để dễ dàng phân biệt 
        // Đối tượng nhóm được tạo bên dưới đối tượng trò chơi gốc.
        if (parentObject == null)
        {
            parentObject = new GameObject(parentObjectName);
            parentObject.transform.parent = transform;
        }

        // Tạo đối tượng nhóm
        var poolObject = new Stack<GameObject>();
        for (int i = 0; i < data.count; i++)
        {
            newGameObject = Instantiate(data.prefab, parentObject.transform);   // Tạo đối tượng trò chơi bằng prefabs
            newGameObject.SetActive(false); 
            poolObject.Push(newGameObject); // Chèn một đối tượng trò chơi vào ngăn xếp
            _keyByPoolObject.Add(newGameObject, data.key);
        }
        _poolObjectByKey.Add(data.key, poolObject);
        _keyBySampleObject.Add(data.key, data.prefab);

#if UNITY_EDITOR
        Debug.Log("오브젝트 이름: " + data.key + "\n 오브젝트 수량: " + parentObject.transform.childCount);
#endif
    }

    /// <summary>
    /// phương pháp để lấy một đối tượng pool.
    /// </summary>
    /// <param name="key">Khóa của đối tượng bạn muốn nhập</param>
    /// <param name="pos">tọa độ bạn muốn nhận</param>
    /// <param name="scaleX">scaleX bạn muốn nhận</param>
    /// <param name="angle">góc độ bạn muốn có được</param>
    /// <returns>đối tượng đầy đủ</returns>
    public GameObject GetPoolObject(string key, Vector2 pos, float scaleX = 1,float angle = 0)
    {
        if(!_poolObjectByKey.TryGetValue(key, out var poolObject))
        {
            // Nếu khóa không tồn tại trong đối tượng nhóm, null sẽ được trả về (lỗi)
#if UNITY_EDITOR
            Debug.Log(key + "라는 오브젝트는 없어요!");
#endif
            return null;
        }

        GameObject getPoolObject;
        if(poolObject.Count > 0)
        {
            // Xuất hiện nếu số lượng đối tượng trong ngăn xếp là 1 hoặc nhiều hơn.
            getPoolObject = poolObject.Pop();
        }
        else
        {
            // Tạo một đối tượng mới nếu không có đối tượng nào trên ngăn xếp
            #if UNITY_EDITOR
                Debug.Log(key + "의 수가 적어 한 개 추가");
            #endif
            string parentObjectName = "Pool <" + key + ">";
            var parentObject = GameObject.Find(parentObjectName);
            getPoolObject = Instantiate(_keyBySampleObject[key], parentObject.transform);
            _keyByPoolObject.Add(getPoolObject, key);
        }

        // Đặt giá trị với tham số nhận được
        getPoolObject.transform.position = new Vector3(pos.x, pos.y, getPoolObject.transform.position.z);
        getPoolObject.transform.rotation = Quaternion.Euler(0, 0, angle);
        getPoolObject.transform.localScale = new Vector3(scaleX, 1, 1);
        getPoolObject.SetActive(true);  // kích hoạt

        // trả lại toàn bộ đối tượng
        return getPoolObject;
    }

    /// <summary>
    /// phương thức trả về một đối tượng nhóm đã sử dụng trở lại ngăn xếp.
    /// phương thức được thực thi, đối tượng trò chơi được trả về sẽ bị vô hiệu hóa.
    /// </summary>
    /// <param name="returnGameObject">Đối tượng trò chơi mà bạn muốn trả lại.</param>
    public void ReturnPoolObject(GameObject returnGameObject)
    {
        returnGameObject.SetActive(false);
        string key = _keyByPoolObject[returnGameObject];
        _poolObjectByKey[key].Push(returnGameObject);   // chèn vào ngăn xếp
    }
}
