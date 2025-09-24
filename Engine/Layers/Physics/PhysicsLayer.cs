using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Box2D.NET;
using static Box2D.NET.B2Joints;
using static Box2D.NET.B2Ids;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2MathFunction;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Diagnostics;
using static Box2D.NET.B2Contacts;
using GlmNet;
using Engine.Graphics;

namespace Engine.Layers
{
    internal class PhysicsLayer : LayerBase
    {
        // Samples: https://github.com/ikpil/Box2D.NET/tree/e68c8ff1fb9da8bd87a71159b13010c25eed76f8/src/Box2D.NET.Samples/Samples

        private B2DebugDraw _debugDraw;
        private B2BodyId _bodyTest;
        private B2BodyId _floorTest;

        public override void Initialize()
        {
            _debugDraw = new B2DebugDraw()
            {
                context = null,

                DrawPolygonFcn = Box2DDraw.DrawPolygon,
                DrawSolidPolygonFcn = Box2DDraw.DrawSolidPolygon,
                DrawCircleFcn = Box2DDraw.DrawCircle,
                DrawSolidCircleFcn = Box2DDraw.DrawSolidCircle,
                DrawSolidCapsuleFcn = Box2DDraw.DrawSolidCapsule,
                DrawSegmentFcn = Box2DDraw.DrawSegment,
                DrawTransformFcn = Box2DDraw.DrawTransform,
                DrawPointFcn = Box2DDraw.DrawPoint,
                DrawStringFcn = Box2DDraw.DrawString,

                drawShapes = false,
                drawJoints = false,
                drawJointExtras = false,
                drawBounds = false,
                drawMass = false,
                drawBodyNames = false,
                drawContacts = false,
                drawGraphColors = false,
                drawContactNormals = false,
                drawContactImpulses = false,
                drawContactFeatures = false,
                drawFrictionImpulses = false,
                drawIslands = false,
            };

            //ulong player = 0x00001, enemy1 = 0x00002, enemy2 = 0x00004, floor = 0x00008;

            //B2Filter playerFilter = default;
            //playerFilter.categoryBits = player;
            //playerFilter.maskBits = enemy1 | enemy2 | floor; // which category can collide with

            //B2Filter floorFilter = default;
            //floorFilter.categoryBits = floor;
            //floorFilter.maskBits = enemy1 | enemy2 | player; // which category can collide with
            //floorFilter.maskBits &= ~player; // remove bit
            //floorFilter.maskBits |= player; // add bit

            //B2Shapes.b2Shape_SetFilter(shapeId, playerFilter);
            //B2Shapes.b2Shape_SetFilter(floorShapeId, floorFilter);

            B2QueryFilter castFilter = default;
            castFilter.categoryBits = 0;
            castFilter.maskBits = 0xFF;

            B2Worlds.b2World_CastRay(PhysicWorld.WorldID, new B2Vec2(0, 0), new B2Vec2(2, 4), castFilter, CastResultFunc, this);

            B2ShapeProxy shapeCast = default;
            shapeCast.radius = 1;
            B2Worlds.b2World_CastShape(PhysicWorld.WorldID, ref shapeCast, new B2Vec2(1, 0), castFilter, CastResultFunc, null);

            // B2Geometries.b2RayCastPolygon();
        }

        private float CastResultFunc(B2ShapeId shapeId, B2Vec2 point, B2Vec2 normal, float fraction, object context)
        {


            return fraction; // Stop at the first hit
        }

        private bool _isContactReached = false;


        float accumulator = 0f;
        float fixedTimeStep = 1.0f / 60.0f;

        internal override void UpdateLayer()
        {
            // var deltaTime = 0.0036f; //Time.DeltaTime;
            var deltaTime = Time.DeltaTime;
            accumulator += deltaTime; // time since last frame
            var scripts = SceneManager.ActiveScene.FindAll<ScriptBehavior>();

            while (accumulator >= fixedTimeStep)
            {
                B2Worlds.b2World_Step(PhysicWorld.WorldID, fixedTimeStep, 1);
                accumulator -= fixedTimeStep;

                foreach (var script in scripts)
                {
                    script.OnFixedUpdate();
                }
            }

            // TODO: refactor, this is for fast protoyping
            var rigidBodies = SceneManager.ActiveScene.FindAll<RigidBody2D>();

            foreach (var rigidbody in rigidBodies)
            {
                rigidbody.UpdateBody();
            }

            B2Worlds.b2World_Draw(PhysicWorld.WorldID, _debugDraw);

            // TODO: Interpolate position and rotation only for rendering, create a smooth model matrix.
            float alpha = accumulator / fixedTimeStep;

            ContactProcess();
        }

        private void ContactProcess()
        {
            // Contacts
            B2ContactEvents events = b2World_GetContactEvents(PhysicWorld.WorldID);
            for (int i = 0; i < events.beginCount; ++i)
            {
                B2ContactBeginTouchEvent evt = events.beginEvents[i];

                B2Shapes.b2Shape_GetUserData(evt.shapeIdA);
                // evt.manifold.points[];
                _isContactReached = true;
                Log.Debug("contact");
                //m_contactId = events.beginEvents[i].contactId;
            }

            for (int i = 0; i < events.endCount; ++i)
            {
                //if (B2_ID_EQUALS(m_contactId, events.endEvents[i].contactId))
                //{
                //    m_contactId = b2_nullContactId;
                //    break;
                //}
            }

            // Sensor
            var sensorEvents = B2Worlds.b2World_GetSensorEvents(PhysicWorld.WorldID);
            for (int i = 0; i < sensorEvents.beginCount; ++i)
            {
                var sensorA = sensorEvents.beginEvents[i].sensorShapeId;
                var visitorB = sensorEvents.beginEvents[i].visitorShapeId;

                B2Shapes.b2Shape_GetUserData(sensorA);

            }

            for (int i = 0; i < sensorEvents.endCount; ++i)
            {

            }
        }

        public override void Close()
        {
        }


    }
}
