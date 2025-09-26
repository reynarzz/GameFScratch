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
using static Box2D.NET.B2Diagnostics;
using static Box2D.NET.B2Contacts;
using GlmNet;
using Engine.Graphics;
using Engine.Utils;

namespace Engine.Layers
{
    internal class PhysicsLayer : LayerBase
    {
        // Samples: https://github.com/ikpil/Box2D.NET/tree/e68c8ff1fb9da8bd87a71159b13010c25eed76f8/src/Box2D.NET.Samples/Samples

        private B2DebugDraw _debugDraw;
        private ContactsDispatcher _contactDispatcher;
        private B2BodyId _bodyTest;
        private B2BodyId _floorTest;
        private float accumulator = 0f;
        private float fixedTimeStep = 1.0f / 60.0f;

        public override void Initialize()
        {
            _contactDispatcher = new ContactsDispatcher();
            _debugDraw = new B2DebugDraw()
            {
                context = this,

                DrawPolygonFcn = Box2DDraw.DrawPolygon,
                DrawSolidPolygonFcn = Box2DDraw.DrawSolidPolygon,
                DrawCircleFcn = Box2DDraw.DrawCircle,
                DrawSolidCircleFcn = Box2DDraw.DrawSolidCircle,
                DrawSolidCapsuleFcn = Box2DDraw.DrawSolidCapsule,
                DrawSegmentFcn = Box2DDraw.DrawSegment,
                DrawTransformFcn = Box2DDraw.DrawTransform,
                DrawPointFcn = Box2DDraw.DrawPoint,
                DrawStringFcn = Box2DDraw.DrawString,

                drawShapes = true,
                drawJoints = true,
                drawJointExtras = false,
                drawBounds = true,
                drawMass = true,
                drawBodyNames = false,
                drawContacts = true,
                drawGraphColors = true,
                drawContactNormals = true,
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

            B2Worlds.b2World_SetCustomFilterCallback(PhysicWorld.WorldID, CustomFilter, null);

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

        private bool CustomFilter(B2ShapeId shapeIdA, B2ShapeId shapeIdB, object context)
        {
            B2Shapes.b2Shape_GetUserData(shapeIdA);
            B2Shapes.b2Shape_GetUserData(shapeIdB);

            return false;
        }


        internal override void UpdateLayer()
        {
            accumulator = Math.Min(accumulator + Time.DeltaTime, 0.25f);

            // TODO: refactor, this is for fast protoyping
            var rigidBodies = SceneManager.ActiveScene.FindAll<RigidBody2D>(findDisabled: false);

            foreach (var rigidbody in rigidBodies)
            {
                rigidbody.PreUpdateBody();
            }

            while (accumulator >= fixedTimeStep)
            {
                SceneManager.ActiveScene.FixedUpdate();

                B2Worlds.b2World_Step(PhysicWorld.WorldID, fixedTimeStep, 4);
                accumulator -= fixedTimeStep;

                _contactDispatcher.Update();

            }

            foreach (var rigidbody in rigidBodies)
            {
                if (rigidbody)
                {
                    rigidbody.PostUpdateBody();
                }

                // Example, remove from here
                B2Transform transform = new B2Transform();
                transform.p = new B2Vec2(rigidbody.Transform.WorldPosition.x, rigidbody.Transform.WorldPosition.y);
                transform.q = rigidbody.Transform.WorldRotation.QuatToB2Rot();

                B2Worlds.b2DrawShape(_debugDraw, B2Shapes.b2GetShape(B2Worlds.b2GetWorld(0), rigidbody.GetComponent<Collider2D>().ShapesId[0]), transform, B2HexColor.b2_colorBlack);

            }
            
            B2Worlds.b2World_Draw(PhysicWorld.WorldID, _debugDraw);
           

            // TODO: Interpolate position and rotation only for rendering, create a smooth model matrix.
            float alpha = accumulator / fixedTimeStep;

        }

        

        public override void Close()
        {
        }


    }
}
