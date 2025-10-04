using Engine;
using Engine.Layers;
using Engine.Utils;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class PlayerTest : ScriptBehavior
    {
        private bool _extraJumpAvailable = false;
        private float _jumpForce = 15;
        private float _walkSpeed = 5.35f;
        private RigidBody2D _rigid;
        private bool _isOnGround = false;
        private float _gravityScale = 3.5f;

        private bool _jumped = false;
        private SpriteRenderer _renderer;
        private Sprite[] _idleSprites;
        private Sprite[] _runSprites;
        private Sprite[] _jumpSprites;
        private Sprite[] _fallSprites;
        private Sprite[] _attackSprites;
        private SpriteAnimation2D _animation;
        private bool _attacking = false;
        private float _attackTime = 0.15f;
        private float _maxFallYVelocity = -20;

        private float _currentAttackTime;

        public override void OnAwake()
        {
            Debug.Info("Awake");
            //new Actor<RotateTest>();
            _rigid = GetComponent<RigidBody2D>();
            _renderer = GetComponent<SpriteRenderer>();

            _rigid.IsContinuos = true;
            _rigid.GravityScale = _gravityScale;
            _rigid.Interpolate = true;

            GetComponent<Collider2D>().Friction = 0;
            //var act = new Actor<SpriteRenderer>();
            //act.Transform.Parent = Transform;
            //act.Transform.LocalPosition = new vec3(-1, 1, 0);

            //var act2 = new Actor<SpriteRenderer>();
            //act2.Transform.Parent = act.Transform;
            //act2.Transform.LocalPosition = new vec3(-1, 1, 0);

            var basePath = "D:\\Projects\\GameScratch\\Game\\Assets\\KingsAndPigsSprites\\01-King Human\\";
            var pTexture = Assets.GetTexture(basePath + "Run (78x58).png");
            var pTexture2 = Assets.GetTexture(basePath + "Idle (78x58).png");
            var jumpTex = Assets.GetTexture(basePath + "Jump (78x58).png");
            var FallTex = Assets.GetTexture(basePath + "Fall (78x58).png");
            var attackTex = Assets.GetTexture(basePath + "Attack (78x58).png");

            pTexture.PixelPerUnit = 16;
            pTexture2.PixelPerUnit = 16;
            jumpTex.PixelPerUnit = 16;
            FallTex.PixelPerUnit = 16;
            attackTex.PixelPerUnit = 16;

            _runSprites = TextureAtlasUtils.SliceSprites(pTexture, 78, 58, new vec2(0.4f, 0.4f));
            _idleSprites = TextureAtlasUtils.SliceSprites(pTexture2, 78, 58, new vec2(0.4f, 0.4f));
            _jumpSprites = TextureAtlasUtils.SliceSprites(jumpTex, 78, 58, new vec2(0.4f, 0.4f));
            _fallSprites = TextureAtlasUtils.SliceSprites(FallTex, 78, 58, new vec2(0.4f, 0.4f));
            _attackSprites = TextureAtlasUtils.SliceSprites(attackTex, 78, 58, new vec2(0.4f, 0.4f));

            _animation = GetComponent<SpriteAnimation2D>();
            _animation.Renderer = _renderer;
            _animation.PushFrames(_idleSprites);
            _animation.FPS = 14;
        }

        public override void OnStart()
        {
            Debug.Info("Start");
            //new Actor<RotateTest>();
        }

        public override void OnEnabled()
        {
        }

        public override void OnDisabled()
        {
        }
        public override void OnUpdate()
        {
            if (!_attacking && Input.GetKeyDown(KeyCode.F))
            {
                _attacking = true;
                _currentAttackTime = _attackTime;
                _animation.Loop = false;
                _animation.PushFrames(_attackSprites);

            }
            if (_currentAttackTime > 0)
            {
                _currentAttackTime -= Time.DeltaTime;

                if (_currentAttackTime <= 0)
                {
                    _animation.Loop = true;
                    _animation.PushFrames(_idleSprites);
                    _attacking = false;
                }
            }

            if (/*(_isOnGround || _extraJumpAvailable) &&*/ Input.GetKeyDown(KeyCode.Space))
            {
                _extraJumpAvailable = false;
                _jumped = true;
                _rigid.GravityScale = _gravityScale;
                _rigid.Velocity = new GlmNet.vec2(_rigid.Velocity.x, 0);
                _rigid.Velocity = new vec2(_rigid.Velocity.x, _jumpForce);
                //_rigid?.AddForce(new GlmNet.vec2(0, _jumpForce), ForceMode2D.Impulse);
                _animation.Play();
                _animation.Loop = true;
                _animation.PushFrames(_jumpSprites);
            }

            if (!_attacking && Input.GetKey(KeyCode.A))
            {
                _rigid.GravityScale = _gravityScale;
                _rigid.Velocity = new GlmNet.vec2(-_walkSpeed, _rigid.Velocity.y);
                Transform.WorldScale = new vec3(-1, 1, 1);
                // _renderer.FlipX = true;

                if (_isOnGround && !_jumped)
                {
                    _animation.Loop = true;
                    _animation.PushFrames(_runSprites);
                }
            }
            else if (!_attacking && Input.GetKey(KeyCode.D))
            {
                _rigid.GravityScale = _gravityScale;
                _rigid.Velocity = new GlmNet.vec2(_walkSpeed, _rigid.Velocity.y);
                Transform.WorldScale = new vec3(1, 1, 1);

                // _renderer.FlipX = false;
                if (_isOnGround && !_jumped)
                {
                    _animation.Loop = true;
                    _animation.PushFrames(_runSprites);
                }

            }
            else
            {
                if (!_attacking && _isOnGround && !_jumped)
                {
                    _animation.Loop = true;
                    _animation.PushFrames(_idleSprites);
                }

                if (!_attacking)
                    _rigid.Velocity = new vec2(0, _rigid.Velocity.y);

            }

            if (!_attacking && !_isOnGround)
            {
                if (_rigid.Velocity.y > 0)
                {
                    _animation.Loop = true;

                    _animation.PushFrames(_jumpSprites);
                }
                else if (_rigid.Velocity.y < 0)
                {
                    _animation.Loop = true;

                    _animation.PushFrames(_fallSprites);
                }

                _animation.Play();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Physics2D.DrawColliders = !Physics2D.DrawColliders;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Window.FullScreen(!Window.IsFullScreen);
                Window.MouseVisible = !Window.IsFullScreen;
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                _rigid.Interpolate = !_rigid.Interpolate;
            }


        }

        public override void OnFixedUpdate()
        {
            TestGround();
            _rigid.Velocity = new vec2(_rigid.Velocity.x, Math.Clamp(_rigid.Velocity.y, _maxFallYVelocity, float.MaxValue));

            // Error: Rays move late not in sync with the velocity
        }

        private void TestGround()
        {
            var length = 1.0f;
            var yOffset = 0;//-0.55f;
            var origin1 = Transform.WorldPosition + new vec3(-0.45f, yOffset, 0);
            var origin2 = Transform.WorldPosition + new vec3(0.45f, yOffset, 0);
            //Debug.DrawRay(Transform.WorldPosition, Transform.Up, Color.Green);

            var hitA = Physics2D.Raycast(origin1, Transform.Down * length, LayerMask.NameToBit("Floor") | LayerMask.NameToBit("Platform"));
            var hitB = Physics2D.Raycast(origin2, Transform.Down * length, LayerMask.NameToBit("Floor") | LayerMask.NameToBit("Platform"));

            var color1 = Color.White;
            var color2 = Color.White;
            if (hitA.isHit || hitB.isHit)
            {
                // Test code
                var pressingKeysToMove = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A);
                if (hitA.isHit)
                {
                    // Debug.Log("RayHit: " + hitA.Collider.Name);
                    color1 = Color.Red;
                    //Debug.DrawRay(origin1 + Transform.Down * length, new vec3(hitA.Normal.x, hitA.Normal.y, 0), Color.Blue);
                    if (!pressingKeysToMove)
                    {
                        //_rigid.GravityScale = 0;
                        _rigid.Velocity = new GlmNet.vec2(0, _rigid.Velocity.y);
                    }
                }


                if (hitB.isHit)
                {
                    //Debug.DrawRay(origin2 + Transform.Down * length, new vec3(hitB.Normal.x, hitB.Normal.y, 0), Color.Blue);
                    color2 = Color.Red;

                    if (!pressingKeysToMove && !_jumped)
                    {
                        //_rigid.GravityScale = 0;
                        _rigid.Velocity = new GlmNet.vec2(0, _rigid.Velocity.y);
                    }
                }

                if (!pressingKeysToMove && !_jumped)
                {
                    _rigid.Velocity = new GlmNet.vec2(0, _rigid.Velocity.y);
                }
                _isOnGround = true;
                _extraJumpAvailable = true;

                if (_rigid.Velocity.y <= 0)
                {
                    _jumped = false;
                }
            }
            else
            {
                _isOnGround = false;
            }

            if (Physics2D.DrawColliders)
            {
                Debug.DrawRay(origin1, Transform.Down * length, color1);
                Debug.DrawRay(origin2, Transform.Down * length, color2);
            }
        }
        public override void OnCollisionEnter2D(Collision2D collision)
        {
            //List<ContactPoint2D> contacts = null;
            //collision.GetContacts(ref contacts);

            //for (int i = 0; i < contacts.Count; i++)
            //{
            //    Debug.Log(contacts[i].Position);
            //}
            Debug.Info($"{Actor.Name} Collision enter with: " + collision.OtherCollider.Name);
            // Transform.Parent = null;
            // Actor.Destroy(collision.Actor);


            //var platform = new Actor<Platform>("Platform");
            //platform.Layer = LayerMask.NameToLayer("Platform");

            //platform.Transform.WorldPosition = new vec3(-10, 5, 0);
        }

        public override void OnCollisionExit2D(Collision2D collision)
        {
            //Debug.Info("Player Collision -exit: " + collision.OtherCollider.Name);
        }

        public override void OnCollisionStay2D(Collision2D collision)
        {
            //Debug.Info("Player Collision stay: " + collision.OtherCollider.Name);
        }
        public override void OnTriggerEnter2D(Collider2D collider)
        {
            Debug.Log("On trigger enter: " + collider.Name);
            //Debug.Log("Destroy");
            //Actor.Destroy(collider.Actor);
        }
    }
}
