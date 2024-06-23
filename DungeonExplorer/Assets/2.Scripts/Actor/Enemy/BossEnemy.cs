using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class DelegateDrone
{
    public GameObject gameObject;
    public SpriteRenderer spriteRenderer;
}

/// <summary>
/// Điều khiển và đối phó với các Boss
/// </summary>
[RequireComponent(typeof(BossEnemyDamage))]
public class BossEnemy : Enemy
{
    public GameObject bossHealthBarUI;                      // Giao diện HP
    public GameObject[] invisibleWall;                      // Tường chắn
    public List<string> attackName = new List<string>();    // Danh sách tên tấn công
    public DelegateDrone[] delegateDrones;                  // Máy bay được sử dụng khi Boss tấn công
    public Sprite[] droneSprite;                            // Chia các loại tấn công của máy bay
    public SpriteRenderer eyeSprite;        // Hiệu ứng nhìn

    // Biến đổi đồ vật được sử dụng khi trùm di chuyển trong trận chiến
    public Transform topLeftMovePos;
    public Transform topRightMovePos;
    public Transform centerMovePos;
    public Transform bottomLeftMovePos;
    public Transform bottomRightMovePos;

    public AudioClip battleMusic;           // Âm thanh trong trận
    public AudioClip targetMissileSound;    // Âm thanh khi Boss bắn trúng Player
    public AudioClip radiationBulletSound;  // Âm thanh khi bắn tùm lum

    int _phase = 1;             // Biến chứa giai đoạn của trận chiến hiện tại
    int _currentAttackIndex;    // Chỉ số của cuộc tấn công hiện tại
    float _attackDelay;         // Delay tấn công

    bool _isDetected;       // Người chơi có nhận ra k?
    bool _isAttacking;      // Boss có tấn công hay không
    bool _isBattleStarted;  // Trận đấu vs boss đã bắt đầu chưa

    Transform _playerTransform;
    Transform _currentMovePos;
    List<Transform> movePos = new List<Transform>();

    Coroutine _attackCoroutine = null;
    AudioClip _prevMusic;
    SpriteRenderer _sprite;
    BossEnemyDamage _bossEnemyDamage;

    protected override void Awake()
    {
        // Loại bỏ boss khi nó chết
        if (DeadEnemyManager.IsDeadBoss(keyName))
        {
            isDead = true;
            Destroy(gameObject);
            return;
        }

        base.Awake();

        // Thêm tọa độ để di chuyển vào danh sách cùng 1 lúc
        var movePosList = new List<Transform>() { topRightMovePos, topRightMovePos, centerMovePos, bottomLeftMovePos, bottomRightMovePos };
        movePos.AddRange(movePosList);

        // Đặt tọa độ đầu tiên để di chuyển về trung tâm
        _currentMovePos = centerMovePos;

        // Khởi tạo Sprite
        _sprite = transform.GetComponent<SpriteRenderer>();
        _sprite.color = new Color32(255, 255, 255, 0);

        // Đặt lại độ trễ tấn công
        _attackDelay = enemyData.attackDelay;

        // Thiết lập máu của boss
        _bossEnemyDamage = (BossEnemyDamage)enemyDamage;
        _bossEnemyDamage.IsInvincibled = true;
        _bossEnemyDamage.KnockBack = null;
        _bossEnemyDamage.Died = null;
        _bossEnemyDamage.phaseChangedEvent += OnPhaseChanged;
        _bossEnemyDamage.KnockBack += OnKnockedBack;
        _bossEnemyDamage.Died += OnDied;

        _playerTransform = GameObject.Find("Player").GetComponent<Transform>();
    }

    void Update()
    {
        // Không thực thi nếu chết
        if (isDead) return;

        // Bộ đếm tgian
        deltaTime = Time.deltaTime;

        if (!_isDetected)
        {
            // Nếu người chơi vẫn chưa được tìm thấy, hãy đợi cho đến khi người chơi đến trong một phạm vi nhất định.
            // Bắt đầu khi vào phạm vi
            float distance = (_playerTransform.position - actorTransform.position).sqrMagnitude;
            if (distance < Mathf.Pow(enemyData.detectRange, 2))
            {
                StartCoroutine(BattleStart());
            }
        }
        else if (_isBattleStarted)
        {
            // Các cuộc tấn công được xử lý trong thời gian thực sau khi trận chiến bắt đầu.
            if (!_isAttacking)
            {
                _attackDelay -= deltaTime;

                if (_attackDelay <= 0)
                {
                    StartAttack();
                }
            }
        }
    }

    /// <summary>
    /// Đây là một coroutine để bắt đầu trận chiến với boss.
    /// </summary>
    IEnumerator BattleStart()
    {
        _isDetected = true;

        // Hp của boss đc kích hoạt
        bossHealthBarUI.SetActive(true);

        // Kích hoạt tường chắn
        foreach (var obj in invisibleWall)
        {
            obj.SetActive(true);
        }

        // Phát nhạc 
        _prevMusic = SoundManager.instance.GetCurrentMusic();
        SoundManager.instance.MusicPlay(battleMusic);

        // Đợi đến khi boss ở tọa độ trung tâm rồi đợi 0,5 giây
        while (actorTransform.position != centerMovePos.position)
        {
            actorTransform.position = Vector3.MoveTowards(actorTransform.position, centerMovePos.position, 4f * deltaTime);
            yield return null;
        }
        yield return YieldInstructionCache.WaitForSeconds(0.5f);

        // Chơi ảnh xuất hiện
        _sprite.color = new Color32(255, 255, 255, 255);
        animator.SetTrigger(GetAnimationHash("Reappear"));

        yield return null;
        float time = animator.GetCurrentAnimatorClipInfo(0).Length;
        yield return YieldInstructionCache.WaitForSeconds(time);

        _isBattleStarted = true;
        _bossEnemyDamage.IsInvincibled = false;
    }


    /// <summary>
    /// Phương pháp thực hiện các cuộc tấn công boss
    /// Tăng sát thương của đòn tấn công hiện tại để thực hiện một đòn tấn công khác nhau mỗi lần
    /// </summary>
    void StartAttack()
    {
        _currentAttackIndex++;
        if (_currentAttackIndex == attackName.Count)
        {
            _currentAttackIndex = 0;
        }
        string nextAttack = attackName[_currentAttackIndex];
        if (nextAttack != null)
        {
            _isAttacking = true;
            _attackCoroutine = StartCoroutine(nextAttack);
        }
    }

    /// <summary>
    /// Cách di chuyển đến tọa độ do sếp chỉ định
    /// </summary>
    IEnumerator Move()
    {
        // Trở nên bất khả chiến bại
        _bossEnemyDamage.IsInvincibled = true;

        // Đợi 1 lúc sau đó biến mất
        animator.SetTrigger(GetAnimationHash("Disappear"));
        yield return null;

        // Đợi thời lượng của hoạt ảnh
        float time = animator.GetCurrentAnimatorClipInfo(0).Length;
        yield return YieldInstructionCache.WaitForSeconds(time);

        // Chọn tọa độ ngẫu nhiên trừ vị trí boss hiện tại
        List<Transform> nextMovePos = movePos.ToList();
        nextMovePos.Remove(_currentMovePos);
        int movePosIndex = Random.Range(0, nextMovePos.Count);

        // Di chuyển đến tọa độ mục tiêu
        while (actorTransform.position != nextMovePos[movePosIndex].position)
        {
            actorTransform.position = Vector3.MoveTowards(actorTransform.position, nextMovePos[movePosIndex].position, enemyData.patrolSpeed * deltaTime);
            yield return null;
        }
        _currentMovePos = nextMovePos[movePosIndex];

        // Hoạt ảnh đang chạy xuất hiện lại sau một thời gian chờ đợi ngắn
        yield return YieldInstructionCache.WaitForSeconds(0.05f);
        animator.SetTrigger(GetAnimationHash("Reappear"));
        yield return null;
        time = animator.GetCurrentAnimatorClipInfo(0).Length;
        yield return YieldInstructionCache.WaitForSeconds(time);

        // Loại bỏ khả năng bất khả chiến bại của trùm và chuyển sang trạng thái không tấn công
        _bossEnemyDamage.IsInvincibled = false;
        _isAttacking = false;
        _attackDelay = enemyData.attackDelay;
    }

    /// <summary>
    /// Thực hiện của máy bay tấn công player
    /// </summary>
    IEnumerator DroneAttack()
    {
        // Chọn số lượng tấn công
        int repeatCount = Random.Range(2, 4);

        for (int i = 0; i < repeatCount; i++)
        {
            // Cài đặt phase hiện tại
            int currentPhase = _phase;
            // Thực hiện tấn công bằng máy bay như giai đoạn hiện tại
            Coroutine[] droneAttackCoroutine = new Coroutine[currentPhase];

            // Đợi 0,5 giây sau khi cài đặt máy bay
            yield return StartCoroutine(DroneSetup(currentPhase));
            yield return YieldInstructionCache.WaitForSeconds(0.5f);

            // Thay đổi
            for (int j = 0; j < currentPhase; j++)
            {
                if (delegateDrones[j].spriteRenderer.sprite == droneSprite[0])
                {
                    droneAttackCoroutine[j] = StartCoroutine(TargetMissile(delegateDrones[j].gameObject.transform));
                }
                else if (delegateDrones[j].spriteRenderer.sprite == droneSprite[1])
                {
                    droneAttackCoroutine[j] = StartCoroutine(RadiationBulletFast(delegateDrones[j].gameObject.transform));
                }
                else
                {
                    droneAttackCoroutine[j] = StartCoroutine(RadiationBulletSlow(delegateDrones[j].gameObject.transform));
                }
            }

            // Dừng cuộc tấn công bằng máy bay sau khi chờ 2 giây
            yield return YieldInstructionCache.WaitForSeconds(2f);
            for (int j = 0; j < droneAttackCoroutine.Length; j++)
            {
                StopCoroutine(droneAttackCoroutine[j]);
            }

            // Sau khi đợi 0,5 giây, máy bay không người lái sẽ quay trở lại vị trí của tên trùm.
            yield return YieldInstructionCache.WaitForSeconds(0.5f);
            yield return StartCoroutine(DroneReturn(currentPhase));

            // Di chuyển vị trí của máy bay ra khỏi màn hình sau khi chờ 1 khung hình.
            yield return null;
            for (int j = 0; j < delegateDrones.Length; j++)
            {
                delegateDrones[j].gameObject.transform.position = new Vector3(-1000, -1000, 0);
            }
        }

        // Kết thúc tấn công
        _attackDelay = enemyData.attackDelay;
        _isAttacking = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="phase"></param>
    /// <returns></returns>
    IEnumerator DroneSetup(int phase)
    {
        // Tọa độ X tối thiểu và tối đa nơi máy bay sẽ đậu
        const float minX = 5;
        const float maxX = 12;

        List<Transform> dronsTransform = new List<Transform>();
        List<Vector3> movePos = new List<Vector3>();

        // Cài đặt nhiều máy bay không người lái như giai đoạn hiện tại
        for (int i = 0; i < phase; i++)
        {
            // Sau khi di chuyển máy bay đến tọa độ của boss, chọn máy bay nào sẽ tấn công bằng cách sử dụng sprite.
            Vector3 pos = actorTransform.position;
            pos.z += 1;
            delegateDrones[i].gameObject.transform.position = pos;
            delegateDrones[i].spriteRenderer.sprite = droneSprite[Random.Range(0, droneSprite.Length)];
            dronsTransform.Add(delegateDrones[i].gameObject.transform);

            // Chọn ngẫu nhiên tọa độ của máy bay
            Vector3 randomPos;
            if (_currentMovePos == topLeftMovePos || _currentMovePos == bottomLeftMovePos)
            {
                randomPos.x = Random.Range(actorTransform.position.x + minX, actorTransform.position.x + maxX);
            }
            else if (_currentMovePos == topRightMovePos || _currentMovePos == bottomRightMovePos)
            {
                randomPos.x = Random.Range(actorTransform.position.x - minX, actorTransform.position.x - maxX);
            }
            else
            {
                randomPos.x = Random.Range(actorTransform.position.x - maxX,actorTransform.position.x + maxX);
                if (randomPos.x < actorTransform.position.x && randomPos.x > actorTransform.position.x - minX)
                {
                    randomPos.x = actorTransform.position.x - minX;
                }
                else if (randomPos.x >= actorTransform.position.x && randomPos.x < actorTransform.position.x + minX)
                {
                    randomPos.x = actorTransform.position.x + minX;
                }
            }
            randomPos.y = Random.Range(actorTransform.position.y - 1.5f,
                                       actorTransform.position.y + 4);
            // Điều chỉnh máy bay để nó không được đặt quá gần boss.
            if (randomPos.y < actorTransform.position.y && randomPos.y > actorTransform.position.y - 2)
            {
                randomPos.y = actorTransform.position.y - 2;
            }
            else if (randomPos.y >= actorTransform.position.y && randomPos.y < actorTransform.position.y + 2)
            {
                randomPos.y = actorTransform.position.y + 2;
            }
            randomPos.z = actorTransform.position.z + 1;

            // Điều chỉnh máy bay thứ hai để nó không ở vị trí tương tự với máy bay đầu tiên.
            if (i > 0)
            {
                if(randomPos.x < movePos[0].x && randomPos.x > movePos[0].x - 3)
                {
                    randomPos.x = movePos[0].x - 3;
                }
                else if (randomPos.x >= movePos[0].x && randomPos.x < movePos[0].x + 3)
                {
                    randomPos.x = movePos[0].x + 3;
                }

                if (randomPos.y < movePos[0].y && randomPos.y > movePos[0].y - 3)
                {
                    randomPos.y = movePos[0].y - 3;
                }
                else if (randomPos.y >= movePos[0].y && randomPos.y < movePos[0].y + 3)
                {
                    randomPos.y = movePos[0].y + 3;
                }
            }

            movePos.Add(randomPos);
        }

        // Đợi cho đến khi tất cả máy bay đến đích
        bool[] droneArrival = new bool[_phase];
        while(true)
        {
            for(int i = 0; i < dronsTransform.Count; i++)
            {
                if(dronsTransform[i].position != movePos[i])
                {
                    dronsTransform[i].position = Vector3.MoveTowards(dronsTransform[i].position, 
                                                            movePos[i], 
                                                            19.0f * deltaTime);
                }
                else droneArrival[i] = true;
            }

            if(droneArrival.Length == 1)
            {
                if (droneArrival[0] == true) yield break;
            }
            else
            {
                if (droneArrival[0] == true || droneArrival[1] == true) 
                {
                    yield break;
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Một coroutine thực hiện một cuộc tấn công trong đó máy bay bắn đạn về phía player.
    /// </summary>
    /// <param name="droneTransform">máy bay sắp tấn công</param>
    IEnumerator TargetMissile(Transform droneTransform)
    {
        while (!isDead)
        {
            // Chọn hướng tấn công dựa trên vị trí của người chơi
            Vector2 firePos = droneTransform.position;
            float x = firePos.x - _playerTransform.position.x;
            float y = firePos.y - _playerTransform.position.y;
            float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

            ObjectPoolManager.instance.GetPoolObject("TargetMissile", droneTransform.position, 1, angle);
            SoundManager.instance.SoundEffectPlay(targetMissileSound);

            // Tấn công cứ sau 0,375 giây
            yield return YieldInstructionCache.WaitForSeconds(0.375f);
        }
    }

    /// <summary>
    /// máy bay tấn công bằng cách phân tán nhanh đạn ra mọi hướng
    /// </summary>
    /// <param name="droneTransform">máy bay không người lái sắp tấn công</param>
    IEnumerator RadiationBulletFast(Transform droneTransform)
    {
        // Hướng tấn công đầu tiên được xác định bởi vị trí của người chơi.
        Vector2 firePos = droneTransform.position;
        float x = firePos.x - _playerTransform.position.x;
        float y = firePos.y - _playerTransform.position.y;
        float currentAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        float time = 0;
        const float Delay = 0.175f;
        while (!isDead)
        {
            // Bắn 4 viên đạn về 4 hướng
            for (int i = 0; i < 4; i++)
            {
                float bulletAngle = currentAngle + (90 * i);
                ObjectPoolManager.instance.GetPoolObject("RadiationBulletFast", droneTransform.position, 1, bulletAngle);
            }
            // phát lại âm thanh
            SoundManager.instance.SoundEffectPlay(radiationBulletSound);

            // Bắt đầu từ 0,6 giây, góc phóng quay 10 độ.
            if (time >= 0.6f)
            {
                currentAngle += 10f;
            }
            else time += Delay;

            // Tấn công cứ sau 0,175 giây
            yield return YieldInstructionCache.WaitForSeconds(Delay);
        }
    }

    /// <summary>
    /// máy bay tấn công bằng cách rải đạn từ từ ra mọi hướng
    /// </summary>
    /// <param name="droneTransform">공격하려는 드론</param>
    IEnumerator RadiationBulletSlow(Transform droneTransform)
    {
        // Hướng tấn công đầu tiên được xác định bởi vị trí của người chơi.
        Vector2 firePos = droneTransform.position;
        float x = firePos.x - _playerTransform.position.x;
        float y = firePos.y - _playerTransform.position.y;
        float currentAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        while (!isDead)
        {
            // Bắn 4 viên đạn về 4 hướng
            for (int i = 0; i < 4; i++)
            {
                float bulletAngle = currentAngle + (90 * i);
                ObjectPoolManager.instance.GetPoolObject("RadiationBulletSlow", droneTransform.position, 1, bulletAngle);
            }
            // phát lại âm thanh
            SoundManager.instance.SoundEffectPlay(radiationBulletSound);
            // Góc phóng xoay 30 độ với mỗi đòn tấn công
            currentAngle += 30f;
            // Tấn công cứ sau 0,3 giây
            yield return YieldInstructionCache.WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// Coroutine lấy máy bay tấn công
    /// </summary>
    IEnumerator DroneReturn(int phase)
    {
        List<Transform> dronsTransform = new List<Transform>();
        for (int i = 0; i < phase; i++)
        {
            dronsTransform.Add(delegateDrones[i].gameObject.transform);
        }

        // Chọn tọa độ muốn quay về làm vị trí boss
        Vector3 returnTarget = actorTransform.position;
        returnTarget.z = dronsTransform[0].position.z;

        // Đợi cho đến khi tất cả máy bay đến đích
        bool[] droneArrival = new bool[phase];
        while (true)
        {
            for (int i = 0; i < dronsTransform.Count; i++)
            {
                if (dronsTransform[i].position != returnTarget)
                {
                    dronsTransform[i].position = Vector3.MoveTowards(dronsTransform[i].position, returnTarget, 24f * deltaTime);
                }
                else
                {
                    droneArrival[i] = true;
                }
            }

            if (droneArrival.Length == 1)
            {
                if (droneArrival[0] == true) yield break;
            }
            else
            {
                if (droneArrival[0] == true || droneArrival[1] == true)
                {
                    yield break;
                }
            }

            yield return null;
        } 
    }

    /// <summary>
    /// Đây là một coroutine xử lý cái chết của boss.
    /// </summary>
    protected override IEnumerator OnDied()
    {
        isDead = true;

        // Hủy bỏ coroutine tấn công và thay đổi nó thành trạng thái Không hoạt động.
        StopCoroutine(_attackCoroutine);
        animator.SetTrigger(GetAnimationHash("Idle"));

        // Lắc màn hình và chạy thời gian đạn
        ScreenEffect.instance.ShakeEffectStart(0.15f, 0.5f);
        ScreenEffect.instance.BulletTimeStart(0f, 1.0f);

        // Di chuyển tất cả các máy bay ra khỏi màn hình
        foreach (var delegateDrone in delegateDrones)
        {
            delegateDrone.gameObject.transform.position = new Vector2(-1000, -1000);
        }

        // Đợi 1 khung hình
        yield return null;

        // Thả 2 vật phẩm hồi phục
        Vector2 position = new Vector2(transform.position.x, transform.position.y + 2);
        for (int i = 0; i < 2; i++)
        {
            ObjectPoolManager.instance.GetPoolObject("HealingPieceDefinitelyDrop", position);
        }

        // Thêm vào danh sách boss đã chết
        DeadEnemyManager.AddDeadBoss(keyName);

        // Vô hiệu hóa thanh sức khỏe
        bossHealthBarUI.SetActive(false);

        // Dừng nhạc boss và chuyển về nhạc đang phát trước đó
        SoundManager.instance.MusicPlay(_prevMusic);

        // Dần dần làm cho đôi mắt của boss trở nên vô hình.
        while (eyeSprite.color.a > 0)
        {
            eyeSprite.color = new Color(255, 255, 255, eyeSprite.color.a - 0.1f);
            yield return YieldInstructionCache.WaitForSeconds(0.1f);
        }

        // chờ 1 giây
        yield return YieldInstructionCache.WaitForSeconds(1.0f);

        // Boss từ từ đi xuống và biến mất từ ​​​​phía dưới màn hình
        float fallingDuration = 3.0f;
        float fallSpeed = 2.0f;
        while (fallingDuration > 0)
        {
            actorTransform.Translate(fallSpeed * Vector2.down * deltaTime);
            fallSpeed += deltaTime * 7;
            fallingDuration -= deltaTime;
            yield return null;
        }

        // Vô hiệu hóa các bức tường
        foreach (var obj in invisibleWall)
        {
            obj.SetActive(false);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Phương thức này được gọi khi giai đoạn của boss thay đổi..
    /// </summary>
    void OnPhaseChanged()
    {
        // Chuyển sang giai đoạn tiếp theo
        _phase++;

        // Vật phẩm hồi máu rơi ra tùy thuộc vào HP của người chơi
        float playerDamage = GameObject.Find("Player").GetComponent<PlayerDamage>().GetHealthPercent();
        int healingPieceCount = playerDamage <= 0.5f ? 2 :
                                playerDamage < 1f ? 1 : 0;
        for (int i = 0; i < healingPieceCount; i++)
        {
            Vector2 position = new Vector2(transform.position.x, transform.position.y + 2);
            ObjectPoolManager.instance.GetPoolObject("HealingPieceDefinitelyDrop", position);
        }
    }
}