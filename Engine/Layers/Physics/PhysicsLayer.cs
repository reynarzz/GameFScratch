using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Box2D.NET;

namespace Engine.Layers
{
    internal class PhysicsLayer : LayerBase
    {
        // Samples: https://github.com/ikpil/Box2D.NET/tree/e68c8ff1fb9da8bd87a71159b13010c25eed76f8/src/Box2D.NET.Samples/Samples
        private static B2WorldId _worldID;
        internal static B2WorldId WorldID => _worldID;


        private B2DebugDraw _debugDraw;
        private B2BodyId _bodyTest;

        public override void Initialize()
        {
            //_world = new World(new Box2DX.Collision.AABB() { LowerBound = new Box2DX.Common.Vec2()});

            _debugDraw = new B2DebugDraw();
            _debugDraw.context = this;

            B2WorldDef worldDef = new B2WorldDef();
            worldDef.gravity = new B2Vec2(0, -9.8f);
            
            _worldID = B2Worlds.b2CreateWorld(ref worldDef);


            B2BodyDef bodyDef = default;
            bodyDef.type = B2BodyType.b2_dynamicBody;
            bodyDef.position = new B2Vec2(0.0f, 0.0f);
            bodyDef.rotation = new B2Rot(2.0f, 1.0f);
            bodyDef.name = "guid value";
            bodyDef.isBullet = false;
            bodyDef.fixedRotation = false;
            bodyDef.isAwake = true;
            bodyDef.gravityScale = 1;
            bodyDef.isEnabled = true;
            
            _bodyTest = B2Bodies.b2CreateBody(_worldID, ref bodyDef);

            B2ShapeDef shapeDef = default;
            shapeDef.isSensor = false;
            shapeDef.enableSensorEvents = true;
            shapeDef.enableHitEvents = true;
            shapeDef.enableContactEvents = true;
            shapeDef.density = 1;
            shapeDef.updateBodyMass = true;
            //B2Polygon poly = default;
            //poly.vertices = new B2FixedArray8<B2Vec2>();
            //poly.vertices[0] = new B2Vec2();

            var boxPoly = B2Geometries.b2MakeBox(0.5f, 0.5f);
            var shapeId = B2Shapes.b2CreatePolygonShape(_bodyTest, ref shapeDef, ref boxPoly);
            // B2Geometries.b2RayCastPolygon();
        }

        // In your game loop:
        float accumulator = 0f;
        float fixedTimeStep = 1.0f / 60.0f;

        internal override void UpdateLayer()
        {
            var deltaTime = 0.16f; // implementation TODO:
            accumulator += deltaTime; // time since last frame
            var scripts = SceneManager.ActiveScene.FindAll<ScriptBehavior>();

            while (accumulator >= fixedTimeStep)
            {
                B2Worlds.b2World_Step(_worldID, fixedTimeStep, 1);
                accumulator -= fixedTimeStep;
               
                foreach (var script in scripts)
                {
                    script.OnFixedUpdate();
                }
            }

            var position = B2Bodies.b2Body_GetPosition(_bodyTest);
            Log.Info($"({position.X}, {position.Y})");

            // TODO: Interpolate position and rotation only for rendering, create a smooth model matrix.
            float alpha = accumulator / fixedTimeStep;

        }

        public override void Close()
        {
        }

        
    }
}
