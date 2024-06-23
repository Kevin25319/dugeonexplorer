using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// xử lý các chức năng của Player
/// </summary>
[RequireComponent(typeof(ActorController), typeof(PlayerDamage))]

public class Player : Actor
{
    // liên quan đến chuyển động
    [SerializeField] float _moveSpeed = 16.5f;                  // Speed
    [SerializeField] float _groundMoveAcceleration = 9.865f;    // gia tốc mặt đất
    [SerializeField] float _groundMoveDeceleration = 19f;       // giảm tốc mặt đất
    [SerializeField] float _airMoveAcceleration = 5.5f;    // gia tốc trong không khí
    [SerializeField] float _airMoveDeceleration = 8f;      // giảm tốc không khí
    [SerializeField] float _runDustEffectDelay = 0.2f;    // Độ trễ trong việc tạo hiệu ứng bụi khi chạy

    // liên quan đến nhảy
    [Space(10)]
    [SerializeField] float _jumpForce = 24f;            // sức mạnh nhảy
    [SerializeField] float _jumpHeight = 5f;            // Nhảy cao
    [SerializeField] float _doubleJumpHeight = 2.0f;    // Độ cao nhảy đôi ( Double Jump )
    [SerializeField] float _maxCoyoteTime = 0.06f;      // thời gian sói tối đa
    [SerializeField] float _maxJumpBuffer = 0.25f;      // Bộ đệm đầu vào nhảy tối đa

    // liên quan đến né
    [Space(10)]
    [SerializeField] float _slidingForce = 24f;          // Mức độ trượt khi né
    [SerializeField] float _dodgeDuration = 0.25f;       // Thời gian né tránh
    [SerializeField] float _dodgeInvinsibleTime = 0.15f; // Thời gian bất khả chiến bại trong khi né tránh
    [SerializeField] float _dodgeCooldown = 0.15f;       // Thời gian cho đến lần trốn tránh tiếp theo có thể thực hiện được sau lần trốn tránh
    [SerializeField] float _dodgeDustEffectDelay = 0.1f; // Độ trễ tạo hiệu ứng bụi trong khi né tránh

    // liên quan đến đá vào tường
    [Space(10)]
    [SerializeField] float _wallSlidingSpeed = 8.5f; // tốc độ trượt trên tường
    [SerializeField] float _wallJumpHeight = 2f;   // Độ cao nhảy tường
    [SerializeField] float _wallJumpXForce = 8f;     // Bay dọc theo trục X bao nhiêu khi nhảy tường  
    [SerializeField] float _wallSlideDustEffectDelay = 0.15f; // Trì hoãn tạo hiệu ứng bụi khi trượt khỏi tường

    // liên quan đến tấn công
    [Space(10)]
    [SerializeField] int _power = 4;                            // dame
    [SerializeField] float _basicAttackKnockBackForce = 8.0f;   // Số lần đánh bật đòn đánh cơ bản
    [SerializeField] float _fireBallKnockBackForce = 7.5f;      // Số lượng ném lại quả cầu lửa

    // Hiệu ứng
    [Space(10)]
    [SerializeField] Image _healingEffect;  // Hiệu ứng khi sức chịu đựng được phục hồi

    // Âm thanh
    [Space(10)]
    [SerializeField] AudioClip _swingSound;         // Âm thanh phát ra khi vung vũ khí
    [SerializeField] AudioClip _fireBallSound;      // âm thanh quả cầu lửa
    [SerializeField] AudioClip _attackHitSound;     // âm thanh tấn công
    [SerializeField] AudioClip _stepSound;          // âm thanh đi bộ
    [SerializeField] AudioClip _jumpSound;          // âm thanh nhảy

    float _coyoteTime;      // Một biến float cho biết khoảng thời gian có thể nhảy sau khi rời khỏi mép của nền tảng.
    float _maxJumpHeight;   // Một biến float để giới hạn khoảng cách bạn có thể nhảy từ độ cao mà bạn hiện đang nhảy.

    float _moveX;   // Giá trị chuyển động trục X của người chơi

    float _nextWallSlideDustEffectTime = 0; // Mất bao lâu để tạo ra hiệu ứng bụi tiếp theo khi trượt khỏi tường?
    float _nextRunDustEffectTime = 0;       // Mất bao lâu để tạo ra hiệu ứng bụi tiếp theo trong khi chạy?


    bool _canMove = true;        // Kiểm tra xem bạn có thể di chuyển không
    bool _canWallSliding = true; // Hãy chắc chắn rằng bạn có thể trượt khỏi tường
    bool _hasDoubleJumped;       // Kiểm tra xem bạn đã thực hiện bước nhảy đôi chưa
    bool _canDodge = true;       // Kiểm tra xem có thể tránh được không

    // biến đầu vào

    // đầu vào trái và phải
    int _xAxis;             // biến int chứa giá trị được chuyển đổi từ đầu vào bên trái và bên phải sang trục x
    bool _leftMoveInput;    // Có nhập chuyển động sang trái hay không
    bool _rightMoveInput;   // Có nhập chuyển động bên phải hay không
    float _timeHeldBackInput;       // Khi trượt trên tường, nếu tiếp tục đưa vào theo hướng ngược lại với tường trong một khoảng thời gian nhất định, một biến float sẽ được sử dụng để thoát khỏi tường.

    // Đầu vào: nhảy
    bool _jumpInput;        // Có nhập nút nhảy hay không
    bool _jumpDownInput;    // Có nhập nút nhảy hay không (chỉ đúng tại thời điểm nhập)
    float _jumpBuffer;      // Biến float để kiểm tra thời gian tiếp tục nhập nút nhảy.
    // đầu vào: tấn công
    bool _attackInput;      // Có nhấn nút tấn công hay không
    bool _specialInput;     // Có nhập nút tấn công đặc biệt hay không
    // đầu vào: né
    bool _dodgeInput;       // Có nhập nút né tránh hay không

    // biến số đưa ra
    bool _isJumped;             // Kiểm tra xem bạn có đang nhảy không
    bool _isFalling;            // Kiểm tra xem bạn có đang rơi không
    bool _isAttacking;          // Kiểm tra xem bạn có đang bị tấn công không
    bool _isDodging;            // Kiểm tra xem bạn có đang tránh không
    bool _isWallSliding;        // Kiểm tra xem bạn có đang trượt khỏi tường không
    bool _isResting;            // Kiểm tra xem bạn có REST không
    bool isBeingKnockedBack;    // Kiểm tra xem bạn có bị knockback không

    // Cấu trúc phản hồi
    KnockBack _basicAttackKnockBack = new KnockBack();
    KnockBack _fireBallKnockBack = new KnockBack();

    Transform _backCliffChecked;            // Đối tượng kiểm tra vách đá phía sau
    Coroutine _knockedBackCoroutine = null; // Coroutine phản công
    PlayerAttack _attack;                   // Một lớp chứa các cuộc tấn công của người chơi
    PlayerDrivingForce _drivingForce;       // Các lớp quản lý và xử lý động lực của người chơi
    PlayerDamage _damage;                   // Một lớp chứa các chức năng xử lý thiệt hại của người chơi

    #region MonoBehaviour

    protected override void Awake()
    {
        base.Awake();

        // Tìm và lưu transform của đối tượng con "BackCliffChecked".
        _backCliffChecked = transform.Find("BackCliffChecked").GetComponent<Transform>();

        // Lấy và lưu tham chiếu tới các thành phần được gắn vào cùng một GameObject.
        _damage = GetComponent<PlayerDamage>();
        _drivingForce = GetComponent<PlayerDrivingForce>();
        _attack = GetComponent<PlayerAttack>();

        // Khởi tạo lực đẩy lùi cho đòn tấn công cơ bản và đòn tấn công cầu lửa.
        _basicAttackKnockBack.force = _basicAttackKnockBackForce;
        _fireBallKnockBack.force = _fireBallKnockBackForce;

        // Đăng ký các sự kiện từ thành phần PlayerDamage.
        _damage.KnockBack += OnKnockedBack;
        _damage.Died += OnDied;
    }

    void Start()
    {
        if (!GameManager.instance.IsStarted())
        {
            // Đặt vị trí và tỷ lệ ban đầu của người chơi nếu trò chơi chưa bắt đầu.
            actorTransform.position = GameManager.instance.playerStartPos;
            actorTransform.localScale = new Vector3(GameManager.instance.playerStartlocalScaleX, 1, 1);

            // Khởi tạo các giá trị HP và lực hiện tại của người chơi từ GameManager.
            _damage.CurrentHealth = GameManager.instance.playerCurrentHealth;
            _drivingForce.CurrentDrivingForce = GameManager.instance.playerCurrentDrivingForce;

            // Lưu trạng thái trò chơi.
            GameManager.instance.GameSave();
        }
        else
        {
            // Cập nhật các giá trị HP và lực hiện tại vào GameManager.
            GameManager.instance.playerCurrentHealth = _damage.CurrentHealth;
            GameManager.instance.playerCurrentDrivingForce = _drivingForce.CurrentDrivingForce;

            if (GameManager.instance.firstStart)
            {
                // Đặt vị trí hồi sinh của người chơi và cảnh hồi sinh nếu đây là lần khởi động đầu tiên.
                GameManager.instance.playerResurrectionPos = actorTransform.position;
                GameManager.instance.resurrectionScene = SceneManager.GetActiveScene().name;
                GameManager.instance.firstStart = false;
            }
            else
            {
                // Nếu cảnh hiện tại là cảnh hồi sinh, đặt lại vị trí của người chơi.
                if (GameManager.instance.resurrectionScene == SceneManager.GetActiveScene().name)
                {
                    actorTransform.position = GameManager.instance.playerStartPos;
                }
            }
        }
    }

    void Update()
    {
        // Kiểm tra nếu người chơi đã chết hoặc đang nghỉ ngơi, không làm gì nếu đúng.
        if (isDead || _isResting) return;

        // Cập nhật deltaTime với giá trị thời gian trôi qua kể từ khung hình trước đó.
        deltaTime = Time.deltaTime;

        // Xử lý các đầu vào của người chơi.
        HandleInput();

        // Xử lý các hành động di chuyển, nhảy, trượt tường, rơi và hạ cánh, tấn công và né tránh.
        HandleMove();
        HandleJump();
        HandleWallSlide();
        HandleFallingAndLanding();
        HandleAttack();
        HandleDodge();

        // Cập nhật các trạng thái hoạt hình của người chơi.
        AnimationUpdate();
    }

    #endregion


    #region Input

    
    void HandleInput()
    {
        // Kiểm tra nếu trạng thái hiện tại của trò chơi không phải là Play thì không làm gì.
        if (GameManager.instance.currentGameState != GameManager.GameState.Play) return;

        // Lấy input di chuyển trái và phải từ GameInputManager.
        _leftMoveInput = GameInputManager.PlayerInput(GameInputManager.PlayerActions.MoveLeft);
        _rightMoveInput = GameInputManager.PlayerInput(GameInputManager.PlayerActions.MoveRight);
        _xAxis = _leftMoveInput ? -1 : _rightMoveInput ? 1 : 0;

        // Lấy input nhảy từ GameInputManager.
        _jumpInput = GameInputManager.PlayerInput(GameInputManager.PlayerActions.Jump);
        _jumpDownInput = GameInputManager.PlayerInputDown(GameInputManager.PlayerActions.Jump);

        // Lấy đầu vào tấn công, tấn công đặc biệt và né tránh từ GameInputManager.
        _attackInput = GameInputManager.PlayerInputDown(GameInputManager.PlayerActions.Attack);
        _specialInput = GameInputManager.PlayerInputDown(GameInputManager.PlayerActions.SpecialAttack);
        _dodgeInput = GameInputManager.PlayerInputDown(GameInputManager.PlayerActions.Dodge);
    }

    #endregion

    #region Move

    
    void HandleMove()
    {
        // Kiểm tra nếu không thể di chuyển thì không làm gì và thoát khỏi phương thức.
        if (!_canMove) return;

        // Kiểm tra nếu có đầu vào di chuyển (trục x khác 0).
        if (_xAxis != 0)
        {
            // Tăng tốc độ di chuyển theo thời gian dựa trên trạng thái của nhân vật (đang đứng trên mặt đất hay ở trên không).
            _moveX += (controller.IsGrounded ? _groundMoveAcceleration : _airMoveAcceleration) * deltaTime;

            // Nếu hướng hiện tại của nhân vật không khớp với hướng di chuyển, lật nhân vật.
            if (actorTransform.localScale.x != _xAxis)
            {
                // Chỉ lật nhân vật nếu không đang tấn công.
                if (!_isAttacking)
                {
                    Flip(); // Lật hướng nhân vật.
                    _moveX = 0; // Đặt lại tốc độ di chuyển.
                    if (controller.IsGrounded)
                    {
                        _nextRunDustEffectTime = 0; // Đặt lại thời gian hiệu ứng chạy.
                    }
                }
            }

            // Nếu nhân vật đang đứng trên mặt đất.
            if (controller.IsGrounded)
            {
               _nextRunDustEffectTime -= deltaTime; // Giảm thời gian đợi cho hiệu ứng bụi chạy theo thời gian.
                // Nếu đã hết thời gian đợi, tạo hiệu ứng bụi chạy.
                if (_nextRunDustEffectTime <= 0)
               {
                    // Tạo hiệu ứng bụi chạy tại vị trí của nhân vật.
                    ObjectPoolManager.instance.GetPoolObject("RunDust", actorTransform.position, actorTransform.localScale.x);
                    _nextRunDustEffectTime = _runDustEffectDelay;

                    // Phát âm thanh bước chân.
                    SoundManager.instance.SoundEffectPlay(_stepSound);

                    // Bắt đầu rung gamepad.
                    GamepadVibrationManager.instance.GamepadRumbleStart(0.02f, 0.017f);
               }
            }
        }
        else
        {
            // Giảm tốc độ di chuyển theo thời gian nếu không có đầu vào di chuyển.
            _moveX -= (controller.IsGrounded ? _groundMoveDeceleration : _airMoveDeceleration) * deltaTime;
            _nextRunDustEffectTime = 0; // Đặt lại thời gian đợi cho hiệu ứng bụi chạy.
        }
        // Giới hạn giá trị của _moveX trong khoảng từ 0 đến 1.
        _moveX = Mathf.Clamp(_moveX, 0f, 1f);

        // Cập nhật tốc độ di chuyển của nhân vật.
        controller.VelocityX = _xAxis * _moveSpeed * _moveX;    
    }

    #endregion

    #region Jump

    
    void HandleJump()
    {
       
        if(controller.IsGrounded || controller.IsWalled)
        {
            _hasDoubleJumped = false;
        }

        
        if (!_isJumped)
        {
            
            if(JumpInputBuffer() && CoyoteTime())
            {
                
                if (!_isAttacking && !_isDodging && !isBeingKnockedBack)
                {
                    StartJump();
                }
            }
            
            else if(PlayerLearnedSkills.hasLearnedDoubleJump)
            {
                if(!_hasDoubleJumped && !controller.IsGrounded && !_isWallSliding)
                {
                    if(_jumpDownInput)
                    {
                        StartDoubleJump();
                    }
                }
            }
        }
        else
        {
            ContinueJumping();
        }
    }

    
    void StartJump()
    {
        _isJumped = true;   

        
        _coyoteTime = _maxCoyoteTime;
        _jumpBuffer = _maxJumpBuffer;

        
        _maxJumpHeight = actorTransform.position.y + _jumpHeight;

        
        ObjectPoolManager.instance.GetPoolObject("JumpDust", actorTransform.position);
        GamepadVibrationManager.instance.GamepadRumbleStart(0.5f, 0.05f);
        SoundManager.instance.SoundEffectPlay(_stepSound);
        SoundManager.instance.SoundEffectPlay(_jumpSound);
    }

    void StartDoubleJump()
    {
        
        _isJumped = true;
        _hasDoubleJumped = true;
        
        _maxJumpHeight = actorTransform.position.y + _doubleJumpHeight;
       
        animator.SetTrigger(GetAnimationHash("DoubleJump"));
        
        ObjectPoolManager.instance.GetPoolObject("JumpDust", actorTransform.position);
        GamepadVibrationManager.instance.GamepadRumbleStart(0.5f, 0.05f);
        SoundManager.instance.SoundEffectPlay(_jumpSound);
    }

    
    void ContinueJumping()
    {
        
        controller.VelocityY = _jumpForce;
        
        if (_maxJumpHeight <= actorTransform.position.y)
        {
            _isJumped = false;
            controller.VelocityY = _jumpForce * 0.75f;
        }
       
        else if (!_jumpInput || controller.IsRoofed)
        {
            _isJumped = false;
            controller.VelocityY = _jumpForce * 0.5f;
        }
    }

    
    bool JumpInputBuffer()
    {
        if (_jumpInput)
        {
            
            if (_jumpBuffer < _maxJumpBuffer)
            {
                _jumpBuffer += deltaTime;
                return true;
            }
        }
        else
        {
            
            _jumpBuffer = 0;
        }
        return false;
    }

    
    bool CoyoteTime()
    {
        if(controller.IsGrounded)
        {
            
            _coyoteTime = 0;
            return true;
        }
        else if(_coyoteTime < _maxCoyoteTime)
        {
            
            _coyoteTime += deltaTime;
            return true;
        }
        
        return false;
    }

    
    void HandleFallingAndLanding()
    {
        if (controller.VelocityY < -5f)
        {
           
            _isFalling = true;
        }
        else if (controller.IsGrounded && _isFalling)
        {
            
            _isFalling = false;
            
            SoundManager.instance.SoundEffectPlay(_stepSound);
            ObjectPoolManager.instance.GetPoolObject("JumpDust", actorTransform.position);
            GamepadVibrationManager.instance.GamepadRumbleStart(0.25f, 0.05f);
        }
    }

    #endregion

    #region ClimbingWall

  
    void HandleWallSlide()
    {
        // Kiểm tra nếu người chơi chưa học kỹ năng leo tường, không thể leo tường,
        // hoặc đang tấn công thì không làm gì và thoát khỏi phương thức.
        if (!PlayerLearnedSkills.hasLearnedClimbingWall || !_canWallSliding || _isAttacking) return;

        // Kiểm tra nếu không đang leo tường.
        if (!_isWallSliding)
        {
            // Kiểm tra nếu nhân vật đang ở trạng thái đụng tường, không đứng trên mặt đất, và đang rơi xuống.
            if (controller.IsWalled && !controller.IsGrounded && controller.VelocityY < 0)
            {
                // Xác định hướng tường (trái hoặc phải).
                int wallDirection = controller.IsLeftWalled ? -1 : 1;

                // Kiểm tra nếu hướng di chuyển của người chơi cùng hướng với tường.
                if (_xAxis == wallDirection)
                {
                    StartWallSliding(); // Bắt đầu leo tường.
                }
            }
        }
        else
        {
            
            ContinueWallSliding(); // Tiếp tục leo tường.
        }
    }

    
    void StartWallSliding()
    {
        _isWallSliding = true; // Đặt trạng thái leo tường là đúng.
        _isDodging = _canMove = false; // Không cho phép né tránh và di chuyển.
        controller.SlideCancle(); // Hủy bỏ mọi chuyển động trượt trước đó.
    }

    
    void ContinueWallSliding()
    {
        int wallDirection = controller.IsLeftWalled ? -1 : 1;
        float dustEffectXPos = actorTransform.position.x + (wallDirection == 1 ? 0.625f : -0.625f);
        Vector2 dustEffectPos = new Vector2(dustEffectXPos, actorTransform.position.y + 1.25f);

        // Giới hạn vận tốc rơi của nhân vật khi leo tường.
        controller.VelocityY = Mathf.Clamp(controller.VelocityY, -_wallSlidingSpeed, float.MaxValue);

        // Kiểm tra nếu người chơi đang giữ phím di chuyển ngược lại hướng tường.
        bool backInput = (wallDirection == 1 && _leftMoveInput) || 
                         (wallDirection == -1 && _rightMoveInput);
        if (backInput)
        {
            _timeHeldBackInput += deltaTime; // Tăng thời gian giữ phím ngược lại.
            if (_timeHeldBackInput >= 0.2f)
            {
                WallSlidingCancle(); // Hủy bỏ trạng thái leo tường nếu giữ phím ngược lại quá lâu.
            }
        }
        else
        {
           
            _timeHeldBackInput = 0; // Đặt lại thời gian giữ phím ngược lại.
        }

        // Kiểm tra nếu người chơi nhấn phím nhảy.
        if (_jumpDownInput)
        {
            
            _isJumped =true; 
            _coyoteTime = _maxCoyoteTime; // Đặt lại thời gian cho phép nhảy sau khi rơi.
            _maxJumpHeight = actorTransform.position.y + _wallJumpHeight; // Đặt chiều cao nhảy tối đa.


            Flip(); // Lật hướng nhân vật.
            controller.SlideMove(_wallJumpXForce, -wallDirection, 30f); // Thực hiện nhảy tường.

            _moveX = 1.0f; // Đặt tốc độ di chuyển.
            
            // Tạo hiệu ứng bụi khi nhảy tường
            ObjectPoolManager.instance.GetPoolObject("WallJumpDust", dustEffectPos, -actorTransform.localScale.x);
            
            GamepadVibrationManager.instance.GamepadRumbleStart(0.5f, 0.05f); // Bắt đầu rung gamepad.
            SoundManager.instance.SoundEffectPlay(_stepSound); // Phát âm thanh bước chân.


            WallSlidingCancle(); // Hủy bỏ trạng thái leo tường.
        }

        // Kiểm tra nếu không còn ở trạng thái đụng tường, hoặc nhân vật đã đứng trên mặt đất, hoặc bị đánh bật (kb).
        if (!controller.IsWalled || controller.IsGrounded || isBeingKnockedBack)
        {
            WallSlidingCancle(); // Hủy bỏ trạng thái leo tường.
        }

        // Giảm thời gian đợi cho hiệu ứng bụi trượt tường theo thời gian.
        _nextWallSlideDustEffectTime -= deltaTime;
        if (_nextWallSlideDustEffectTime <= 0)
        {
            // Đặt lại thời gian đợi cho hiệu ứng bụi trượt tường.
            _nextWallSlideDustEffectTime = _wallSlideDustEffectDelay;

            // Tạo hiệu ứng bụi trượt tường.
            ObjectPoolManager.instance.GetPoolObject("WallSlideDust", dustEffectPos,actorTransform.localScale.x);

            GamepadVibrationManager.instance.GamepadRumbleStart(0.1f, 0.033f); // Bắt đầu rung gamepad.
            SoundManager.instance.SoundEffectPlay(_swingSound); // Phát âm thanh trượt tường.
        }
    }

   
    void WallSlidingCancle()
    {
        _isWallSliding = false; // ngừng trạng thái leo tường
        _canMove = true; // chp phép di chuyển lại bth 
        _timeHeldBackInput = 0; // đặt lại tgian giữ phím về 0
        _nextWallSlideDustEffectTime = _wallSlideDustEffectDelay; // đặt tgian hiệu ứng bụi trượt về mặc định
    }

    #endregion

    #region Attack

    
    void HandleAttack()
    {
        
        if(_isAttacking || _isWallSliding || _isDodging || isBeingKnockedBack) return;

        if(_attackInput)
        {
            
            StartCoroutine(BasicAttack());
        }
        else if(_specialInput)
        {
            if (_drivingForce.TryConsumeDrivingForce(1))
            {
                StartCoroutine(FireBall());
            }
        }
    }

    
    IEnumerator BasicAttack()
    {
        
        _isAttacking = true;

        bool isHit = false;
        bool isNextAttacked = false;

        
        animator.SetTrigger(GetAnimationHash("BasicAttack"));
        animator.ResetTrigger(GetAnimationHash("AnimationEnd"));

        
        _basicAttackKnockBack.direction = actorTransform.localScale.x;

        
        if (controller.IsGrounded)
        {
            controller.SlideMove(_moveSpeed, actorTransform.localScale.x, 65f);
        }

       
        yield return null;
        
        
        SoundManager.instance.SoundEffectPlay(_swingSound);

        
        while(!IsAnimationEnded())
        {
            
            if(isBeingKnockedBack) break;

            if(!isHit)
            {
               
                if (_attack.IsAttacking(_power, _basicAttackKnockBack))
                {
                    
                    for (int i = 0; i < _attack.HitCount; i++)
                    {
                        _drivingForce.IncreaseDrivingForce();
                    }
                    
                    SoundManager.instance.SoundEffectPlay(_attackHitSound);
                    GamepadVibrationManager.instance.GamepadRumbleStart(0.5f, 0.05f);

                    
                    controller.SlideMove(11.5f, -actorTransform.localScale.x);

                    
                    if (!controller.IsGrounded)
                    {
                        controller.VelocityY = 15;
                    }

                    
                    isHit = true;
                }
            }

            
            if(!Physics2D.Raycast(_backCliffChecked.position, Vector2.down, 1.0f, LayerMask.GetMask("Ground")))
            {
                controller.SlideCancle();
            }
            
            
            _canMove = !controller.IsGrounded;

          
            if (IsAnimatorNormalizedTimeInBetween(0.4f, 0.87f))
            {
                if (_attackInput)
                {
                    isNextAttacked = true;
                }
            }
            else if(IsAnimatorNormalizedTimeInBetween(0.87f, 1.0f))
            {
                if(isNextAttacked)
                {
                    
                    if(_xAxis == actorTransform.localScale.x)
                    {
                        controller.SlideMove(_moveSpeed, actorTransform.localScale.x, 65f);
                    }
                   
                    animator.SetTrigger(GetAnimationHash("NextAttack"));
                    
                    SoundManager.instance.SoundEffectPlay(_swingSound);

                   
                    isHit = isNextAttacked = false;
                }
            }

            yield return null;
        }

        
        AttackEnd();
    }

    
    IEnumerator FireBall()
    {
        
        _isAttacking = true;

        
        bool hasSpawnedFireBall = false;

        
        animator.SetTrigger(GetAnimationHash("FireBall"));
        animator.ResetTrigger(GetAnimationHash("AnimationEnd"));
        _fireBallKnockBack.direction = actorTransform.localScale.x;

        
        yield return null;

       
        SoundManager.instance.SoundEffectPlay(_fireBallSound);

       
        while(!IsAnimationEnded())
        {
            
            if(isBeingKnockedBack) break;

            
            if(controller.IsGrounded)
            {
                _moveX = 0;
                _canMove = false;
            }

            
            if(IsAnimatorNormalizedTimeInBetween(0.28f, 0.4f))
            {
                // Chỉ tạo một quả cầu lửa nếu nó chưa được tạo
                if (!hasSpawnedFireBall)
                {
                    float scaleX = actorTransform.localScale.x;
                    float addPosX = actorTransform.position.x + (0.66f * scaleX);
                    float addPosY = actorTransform.position.y + 1.12f;
                    Vector2 addPos = new Vector2(addPosX, addPosY);
                    float angle = scaleX == 1 ? 180 : 0;

                    ObjectPoolManager.instance.GetPoolObject("FireBall", addPos, scaleX, angle);

                    hasSpawnedFireBall = true;  // Đặt quả cầu lửa về trạng thái được tạo
                }
            }

            yield return null;
        }

        // Thực thi phương thức AttackEnd khi cuộc tấn công kết thúc
        AttackEnd();
    }

    
    void AttackEnd()
    {
        animator.SetTrigger(GetAnimationHash("AnimationEnd"));
        _isAttacking = false;
        _canMove = true;
    }

    #endregion

    #region Dodge

    /// <summary>
    /// xử lý việc né tránh của nhân vật người chơi.
    /// </summary>
    void HandleDodge()
    {
        // Không thực hiện trong các trạng thái sau (tấn công, trượt khỏi tường, né hoặc bị đẩy lùi)
        if (_isAttacking || _isWallSliding || _isDodging || isBeingKnockedBack) return;

        // Khi thực hiện né tránh, nếu có thể né tránh và tiếp xúc với mặt đất thì thực hiện trượt.
        if (_dodgeInput && _canDodge && controller.IsGrounded)
        {
            StartCoroutine(Dodging());
        }
    }

    /// <summary>
    /// Coroutine xử lý chức năng né (slide) của nhân vật người chơi.
    /// </summary>
    /// <returns></returns>
    IEnumerator Dodging()
    {
        // Thiết lập thời gian bất khả chiến bại và thời gian tạo hiệu ứng
        float nextDodgeDustEffectTime = 0;
        float dodgeInvinsibleTime = _dodgeInvinsibleTime;
        float dodgeDuration = _dodgeDuration;

        // Đặt ở trạng thái trốn tránh
        _damage.IsDodged = _isDodging = true;
        // Đặt ở trạng thái không thể trốn tránh và di chuyển
        _canDodge = _canMove = false;

        // Thực thi hoạt ảnh tránh, đặt lại trình kích hoạt kết thúc hoạt ảnh để tránh lỗi
        animator.SetTrigger(GetAnimationHash("Sliding"));
        animator.ResetTrigger(GetAnimationHash("AnimationEnd"));

        // Đợi 1 khung hình
        yield return null;

        // Thực hiện chuyển động trượt
        controller.SlideMove(_slidingForce, actorTransform.localScale.x);

        // Thực hiện né tránh cho đến khi thời gian né tránh kết thúc.
        while (dodgeDuration > 0)
        {
            // Hủy né tránh nếu bị đẩy lùi
            if (isBeingKnockedBack) break;

            // Liên tục tạo ra hiệu ứng bụi và phát ra âm thanh khi né tránh
            if (nextDodgeDustEffectTime <= 0)
            {
                ObjectPoolManager.instance.GetPoolObject("RunDust",
                                                        actorTransform.position,
                                                        actorTransform.localScale.x);
                nextDodgeDustEffectTime = _dodgeDustEffectDelay;
                SoundManager.instance.SoundEffectPlay(_jumpSound);
            }
            else
            {
                nextDodgeDustEffectTime -= deltaTime;
            }

            // Nếu thời gian bất khả chiến bại kết thúc trong khi trốn tránh, trạng thái bất khả chiến bại sẽ bị hủy bỏ.
            if (dodgeInvinsibleTime <= 0)
            {
                _damage.IsDodged = false;
            }
            else
            {
                dodgeInvinsibleTime -= deltaTime;
            }

            // Giảm thời gian trốn tránh trong thời gian thực
            dodgeDuration -= deltaTime;
            yield return null;
        }

        // Thực hiện phương pháp thoát tránh
        DodgeEnd();
    }

    /// <summary>
    /// thực thi chức năng cần thiết khi việc né (slide) kết thúc.
    /// </summary>
    void DodgeEnd()
    {
        // dừng chuyển động trượt
        controller.SlideCancle();
        // Kết thúc hoạt ảnh
        animator.SetTrigger(GetAnimationHash("AnimationEnd"));

        // Kết thúc trạng thái trốn tránh và đặt nó ở trạng thái có thể di chuyển
        _isDodging = false;
        _canMove = true;

        // Chạy coroutine thời gian hồi chiêu né tránh
        StartCoroutine(DodgeCooldown());
    }

    /// <summary>
    /// Đây là một coroutine cho phép sử dụng khả năng né tránh trong một khoảng thời gian nhất định sau khi sử dụng.
    /// </summary>
    IEnumerator DodgeCooldown()
    {
        // Sau khi thời gian hồi chiêu né tránh, việc né tránh lại có thể thực hiện được.
        yield return YieldInstructionCache.WaitForSeconds(_dodgeCooldown);
        _canDodge = true;
    }

    #endregion

    #region Rest

    /// <summary>
    /// phương thức thực hiện ngắt tại điểm kiểm tra. Phục hồi máu của nhân vật người chơi và cứu trò chơi. Nếu người chơi chết hoặc tiếp tục trò chơi, trò chơi sẽ bắt đầu từ vị trí đó.
    /// </summary>
    /// <param name="checkPointPos">Tọa độ trạm kiểm soát</param>
    public void RestAtCheckPoint(Vector2 checkPointPos)
    {
        // Không kích hoạt chế độ nghỉ nếu người chơi đang ở trên không hoặc đã Rest
        if (!controller.IsGrounded || _isResting) return;

        // Đặt vị trí xuất hiện của người chơi theo tọa độ của trạm kiểm soát và bối cảnh hiện tại.
        GameManager.instance.playerResurrectionPos = checkPointPos;
        GameManager.instance.resurrectionScene = SceneManager.GetActiveScene().name;

        // Hồi sinh kẻ thù bị giết
        DeadEnemyManager.ClearDeadEnemies();

        // lưu trò chơi
        GameManager.instance.GameSave();

        // Thực thi coroutine còn lại
        StartCoroutine("Resting");
    }

    /// <summary>
    /// xử lý phần còn lại.
    /// </summary>
    IEnumerator Resting()
    {
        float red = _healingEffect.color.r;
        float green = _healingEffect.color.g;
        float blue = _healingEffect.color.b;
        float alpha = 0.5f;

        // nghỉ chạy
        _isResting = true;
        animator.ResetTrigger(GetAnimationHash("AnimationEnd"));
        animator.SetTrigger(GetAnimationHash("Rest"));
        yield return YieldInstructionCache.WaitForSecondsRealtime(0.2f);

        // Phục hồi toàn bộ sức máu sau khi thực hiện hiệu ứng nhấp nháy màn hình.
        _healingEffect.enabled = AccessibilitySettingsManager.screenFlashes;
        _healingEffect.color = new Color(red, green, blue, alpha);
        _damage.HealthRecovery(_damage.maxHealth);
        yield return YieldInstructionCache.WaitForSecondsRealtime(0.1f);

        // Từ từ loại bỏ hiệu ứng nhấp nháy màn hình
        while (alpha > 0f)
        {
            alpha -= 0.025f;
            _healingEffect.color = new Color(red, green, blue, alpha);

            yield return YieldInstructionCache.WaitForSecondsRealtime(0.01f);
        }
        yield return YieldInstructionCache.WaitForSecondsRealtime(0.1f);

        // kết thúc Rest
        animator.SetTrigger(GetAnimationHash("AnimationEnd"));
        _isResting = false;
    }

    #endregion

    #region Damage

    /// <summary>
    /// phương thức gọi lại để thực hiện hạ gục khi nhân vật của người chơi bị kẻ thù tấn công.
    /// </summary>
    /// <param name="knockBack">knockback</param>
    void OnKnockedBack(KnockBack knockBack)
    {
        // 중력을 사용하는 상태로 변경
        controller.UseGravity = true;

        // 플레이어 캐릭터의 좌표를 살짝 위로 올림(버그 방지)
        actorTransform.Translate(new Vector2(0, 0.1f));

        // 넉백량과 방향으로 미끄러지는 움직임을 실행하고 움직일 수 없는 상태로 설정
        controller.SlideMove(knockBack.force, knockBack.direction);
        _canMove = false;

        // 휴식 상태 코루틴을 중단하고 휴식 상태를 false로 설정
        StopCoroutine("Resting");
        _isResting = false;

        // 다음 상태를 모두 false로 설정(공격 상태, 회피 상태, 벽에서 미끄러지는 상태, 점프 상태)
        _isAttacking = _isDodging = _isWallSliding = _isJumped = false;
        // 넉백 중인 상태로 설정하고 게임 패드 진동 실행
        isBeingKnockedBack = true;
        GamepadVibrationManager.instance.GamepadRumbleStart(0.8f, 0.068f);

        if(!isDead)
        {
            // 플레이어 캐릭터가 죽지 않은 상태일 경우 위로 띄워지고 넉백 코루틴 실행
            controller.VelocityY = 24f;
            if(_knockedBackCoroutine != null)
            {
                // 이미 넉백 중인 상태이면 기존 넉백 중단
                StopCoroutine(_knockedBackCoroutine);
                _knockedBackCoroutine = null;
            }
            _knockedBackCoroutine = StartCoroutine(KnockedBackCoroutine());
        }
    }

    /// <summary>
    /// xử lý các chức năng hoạt ảnh và chấm dứt phản đòn khi nhân vật của người chơi bị đẩy lùi.
    /// </summary>
    /// <returns></returns>
    IEnumerator KnockedBackCoroutine()
    {
        // Chạy hoạt hình phản đòn (kb)
        animator.SetTrigger(GetAnimationHash("KnockBack"));

        // Chờ cho đến khi quá trình trượt dừng lại
        while (controller.IsSliding)
        {
            yield return null;
        }

        // Đặt trạng thái có thể di chuyển và làm gián đoạn trạng thái phản đòn
        _canMove = true;
        isBeingKnockedBack = false;

        // Hoạt ảnh phản kháng kết thúc
        animator.SetTrigger(GetAnimationHash("KnockBackEnd"));

        _knockedBackCoroutine = null;
    }

    /// <summary>
    /// coroutine gọi lại chạy khi nhân vật của người chơi chết.
    /// </summary>
    IEnumerator OnDied()
    {
        // chết
        isDead = true;
        // quản lý gửi thông báo người chơi chết
        GameManager.instance.HandlePlayerDeath();
        // Hoạt hình cái chết của nhân vật người chơi đang chạy
        animator.SetTrigger(GetAnimationHash("Die"));
        // Chạy thời gian đạn trong 1 giây
        ScreenEffect.instance.BulletTimeStart(0.3f, 1.0f);
        // Đợi 2,5 giây, thiết lập lại sức chịu đựng và động lực rồi di chuyển đến vị trí hồi sinh.
        yield return YieldInstructionCache.WaitForSecondsRealtime(2.5f);
        GameManager.instance.playerCurrentHealth = _damage.maxHealth;
        GameManager.instance.playerCurrentDrivingForce = 0;
        SceneTransition.instance.LoadScene(GameManager.instance.resurrectionScene);
    }

    #endregion

    #region Animator

    /// <summary>
    /// cập nhật hoạt ảnh của nhân vật người chơi.
    /// </summary>
    void AnimationUpdate()
    {
        animator.SetFloat(GetAnimationHash("MoveX"), _moveX);
        animator.SetFloat(GetAnimationHash("FallSpeed"), controller.VelocityY);

        animator.SetBool(GetAnimationHash("IsGrounded"), controller.IsGrounded);
        animator.SetBool(GetAnimationHash("IsWallSliding"), _isWallSliding);
    }

    #endregion
}