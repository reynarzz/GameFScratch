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
        private static B2WorldId _worldID;
        internal static B2WorldId WorldID => _worldID;

        public override void Initialize()
        {
            //_world = new World(new Box2DX.Collision.AABB() { LowerBound = new Box2DX.Common.Vec2()});

            B2WorldDef worldDef = new B2WorldDef();
            worldDef.gravity = new B2Vec2(0, -9.8f);

            _worldID = B2Worlds.b2CreateWorld(ref worldDef);

            B2BodyDef bodyDef = default;
            
            var bodyId = B2Bodies.b2CreateBody(_worldID, ref bodyDef);
            var rot = B2Bodies.b2Body_GetRotation(bodyId);

            B2ShapeDef shapeDef = default;
            shapeDef.isSensor = true;
            shapeDef.enableSensorEvents = true;
            shapeDef.enableHitEvents = true;
            shapeDef.enableContactEvents = true;

            B2Polygon poly = default;
            poly.vertices = new B2FixedArray8<B2Vec2>();
            poly.vertices[0] = new B2Vec2();

            var shapeId = B2Shapes.b2CreatePolygonShape(bodyId, ref shapeDef, ref poly);
            
        }

        internal override void UpdateLayer()
        {
            
            B2Worlds.b2World_Step(_worldID, 0, 0);
        }

        public override void Close()
        {
        }

        
    }
}
