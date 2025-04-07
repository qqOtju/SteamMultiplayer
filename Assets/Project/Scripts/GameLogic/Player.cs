using System;
using Mirror;
using Project.Scripts.Infrastructure;
using Project.Scripts.Network;
using Steamworks;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace Project.Scripts.GameLogic
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : EntityBase
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _acceleration = 10f;
        [SerializeField] private float _deceleration = 12f;
        [SerializeField] private float _airControl = 0.5f;
        [Header("Jump Settings")]
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private float _coyoteTime = 0.1f;
        [SerializeField] private float _jumpBufferTime = 0.1f;
        [Header("Ground Detection")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private Transform _view;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem _ps;
        [Header("Attack Settings")]
        [SerializeField] private Transform _attackPoint;
        [SerializeField] private LayerMask _attackLayer;
        [SerializeField] private Attack _attackAnimator;
        [Header("Other")]
        [SerializeField] private TMP_Text _nicknameText;

        private const float AttackTime = .6f;
        private const float AttackRadius = 0.9f;
        private const float AttackDamage = 1f;
        private const float DamageTime = 0.5f;
        
        private static readonly int AttackAnimHash = Animator.StringToHash("Attack");
        private static readonly int DamageAnimHash = Animator.StringToHash("Damage");
        private static readonly int DeathAnimHash = Animator.StringToHash("Death");
        private static readonly int SpeedAnimHash = Animator.StringToHash("Speed");
        private static readonly int JumpAnimHash = Animator.StringToHash("Jump");
        
        private readonly Collider2D[] _attackResults = new Collider2D[10];
        
        private Callback<AvatarImageLoaded_t> _avatarImageLoaded;
        private ContactFilter2D _attackFilter = new ();
        private CinemachineCamera _cinemachineCamera;
        private Rigidbody2D _rigidbody;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;
        private float _attackTimer;
        private float _damageTimer;
        private float _xInput;
        private bool _jumpPressed;
        private bool _isGrounded;
        private CSteamID _steamID;
        private bool _attack;
        private bool _damage;
        private bool _death;

        [SyncVar(hook = nameof(OnNameChanged))]
        public string PlayerName;
        [SyncVar(hook = nameof(OnColorChanged))]
        public Color PlayerColor = Color.white;
        [SyncVar]
        public Texture2D PlayerAvatar;
        

        [Inject]
        private void Construct(CinemachineCamera cinemachineCamera)
        {
            _cinemachineCamera = cinemachineCamera;
            Debug.Log($"Cinemachine camera");
        }
        
        [Client]
        private void Awake()
        {
            OnHealthChange += CheckAnimation;
        }

        [Client]
        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _attackFilter.useTriggers = false;
            _attackFilter.SetLayerMask(_attackLayer);
            _attackAnimator.transform.SetParent(null);
        }

        public override void OnStartClient()
        {
            _avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            if(!netIdentity.isOwned) return;
            Debug.Log($"#{netIdentity.connectionToClient} Starting authority for player...");
            GameInstaller.DiContainer.Inject(this);
            var cameraTarget = new CameraTarget();
            cameraTarget.TrackingTarget = _view;
            _cinemachineCamera.Target = cameraTarget;
            Debug.Log($"#{netIdentity.connectionToClient} is owned by client");
        }

        [Client]
        private void Update()
        {
            if(!netIdentity.isOwned) return;
            StunTimer();
            if(_death || _damage) return;
            CheckInput();
            RotateView();
            HandleAttack();
        }

        [Client]
        private void FixedUpdate()
        {
            if(!netIdentity.isOwned) return;
            CheckGround();
            if(_attack || _damage || _death) return;
            Move();
            HandleJump();
        }

        private void OnNameChanged(string oldName, string newName)
        {
            _nicknameText.text = newName;
        }

        private void OnColorChanged(Color oldColor, Color newColor)
        {
            _nicknameText.color = newColor;
        }
        
        [Client]
        private void CheckAnimation(OnHealthChangeArgs obj)
        {
            if(obj.Type == HeathChangeType.Damage)
            {
                _animator.SetBool(DamageAnimHash, true);
                _animator.SetBool(JumpAnimHash, false);
                _damage = true;
                _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
            } else if(obj.Type == HeathChangeType.Death)
            {
                _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                _animator.SetBool(DeathAnimHash, true);
                _animator.SetBool(DamageAnimHash, false);
                _animator.SetBool(JumpAnimHash, false);
                _damage = false;
                _death = true;
            }else if(obj.Type == HeathChangeType.Heal)
            {
                _animator.SetBool(DeathAnimHash, false);
                _animator.SetBool(DamageAnimHash, false);
                _damage = false;
                _death = false;
            }
        }
        
        [Server]
        public void SetupPlayer(CSteamID steamID)
        {
            _steamID = steamID;
            SetInitialHealth(DefaultMaxHealth);
            var playerName = SteamFriends.GetFriendPersonaName(_steamID);
            var playerImage = GetAvatarImage(_steamID);
            var playerColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            PlayerName = playerName;
            PlayerColor = playerColor;
            PlayerAvatar = playerImage;
        }

        private void OnAvatarImageLoaded(AvatarImageLoaded_t param)
        {
            if(param.m_steamID != _steamID) return;
            PlayerAvatar = GetAvatarImage(_steamID);
        }
        
        private Texture2D GetAvatarImage(CSteamID steamId)
        {
            var imageId = SteamFriends.GetLargeFriendAvatar(steamId);
            if (imageId == -1) return null;
            var texture = new Texture2D(0, 0);
            if (!SteamUtils.GetImageSize(imageId, out var width, out var height)) return texture;
            var imageData = new byte[width * height * 4];
            if (SteamUtils.GetImageRGBA(imageId, imageData, (int)(width * height * 4)))
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(imageData);
                texture.Apply();
            }
            return texture;
        }

        [Client]
        private void CheckInput()
        {
            _xInput = Input.GetAxisRaw("Horizontal");
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || 
                Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.JoystickButton14))
                _jumpBufferCounter = _jumpBufferTime;
            else
                _jumpBufferCounter -= Time.deltaTime;
            if(Input.GetKeyDown(KeyCode.B))
                TakeDamageCommand(1);
        }

        [Command]
        private void TakeDamageCommand(float value)
        {
            TakeDamage(value);
        }

        [Client]
        private void RotateView()
        {
            if (_xInput > 0)
                _view.localScale = new Vector3(1, 1, 1);
            else if (_xInput < 0)
                _view.localScale = new Vector3(-1, 1, 1);
        }

        [Client]
        private void StunTimer()
        {
            if (!_damage) return;
            _damageTimer += Time.deltaTime;
            if (_damageTimer >= DamageTime)
            {
                _animator.SetBool(DamageAnimHash, false);
                _damage = false;
                _damageTimer = 0f;
            }
        }

        [Client]
        private void HandleAttack()
        {
            if ((Input.GetKey(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.JoystickButton13)) && _isGrounded && !_damage)
            {
                _attack = true;
                _rigidbody.linearVelocity = Vector2.zero;
                _attackTimer += Time.deltaTime;
                if (_attackTimer >= AttackTime)
                {
                    _attackTimer = 0f;
                    AttackCommand();
                }
            }
            else
            {
                _attackTimer = 0f;
                _attack = false;
            }
            _animator.SetBool(AttackAnimHash, _attack);
        }

        [Command]
        private void AttackCommand()
        {
            var attackPointPosition = _attackPoint.position;
            AttackRpc(attackPointPosition);
            Array.Clear(_attackResults, 0, _attackResults.Length);
            Physics2D.OverlapCircle(attackPointPosition, AttackRadius, _attackFilter, _attackResults);
            foreach (var result in _attackResults)
            {
                if (result == null || result.gameObject == gameObject || !result.CompareTag("Player")) continue;
                var health = result.GetComponent<IHealth>();
                health?.TakeDamage(AttackDamage);
            }
        }
        
        [ClientRpc]
        private void AttackRpc(Vector3 attackPointPosition)
        {
            _attackAnimator.transform.position = attackPointPosition;
            _attackAnimator.PlayAnimation();
        }

        [Client]
        private void CheckGround()
        {
            _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
            /*if(_isGrounded)
                _ps.Play();
            else
                _ps.Stop();*/
            if(!_damage && !_death)
                _animator.SetBool(JumpAnimHash, !_isGrounded);
            if (_isGrounded)
                _coyoteTimeCounter = _coyoteTime;
            else
                _coyoteTimeCounter -= Time.deltaTime;
        }

        [Client]
        private void Move()
        {
            var targetSpeed = _xInput * _moveSpeed;
            var acceleration = _isGrounded ? _acceleration : _acceleration * _airControl;
            _rigidbody.linearVelocity = new Vector2(
                Mathf.Lerp(_rigidbody.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime),
                _rigidbody.linearVelocity.y);
            _animator.SetFloat(SpeedAnimHash, Mathf.Abs(_rigidbody.linearVelocity.x));
        }

        [Client]
        private void HandleJump()
        {
            if (!(_jumpBufferCounter > 0f) || !(_coyoteTimeCounter > 0f)) return;
            //_rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
            _animator.SetBool(JumpAnimHash, true);
            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
}
