using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class LevelEntity
    {
        public string Identifier { get; set; }
        public Type ScriptType { get; set; }
    }

    public enum AdjacentLevel : uint
    {
        Top,
        Bottom,
        Right,
        Left,

        BottomLeft,
        TopLeft,
        TopRight,
        BottomRight,
    }

    public enum LevelPrewarmOptions
    {
        None = 0,
        PrewarmAdjacents,
        ActiveAdjacents,
    }

    public class PrewarmOptions
    {
        public LevelPrewarmOptions LevelPrewarmOptions { get; set; }
        public AdjacentLevel AdjacentLevels { get; set; }

        /// <summary>
        /// Entities of adjacent levels are going to be enabled.
        /// </summary>
        public bool PrewarmEntities { get; set; }
    }

    public interface IEntityControlData
    {
        public bool EnableOnInitialization { get; set; }
        public Actor Actor { get; set; }
    }

    public interface IEntityData
    {
        public string EntityId { get; set; }
        public string Name { get; set; }
        public int Layer { get; set; }
        public string Tag { get; set; }
    }

    public interface IEntityBuilder
    {
        public void Build(IEntityData data);
    }

    public class EntityBuilder : IEntityBuilder
    {
        public void Build(IEntityData data)
        {
            
        }
    }

    // TODO: Make a streaming level system, only load neighbor levels, 
    public class LevelOptions
    {
        public ldtk.Level Level { get; set; }
        public string[] CollisionIds { get; set; }
        public string[] LevelEntities { get; set; }
    }

    public class LdtkLevel : Component
    {
        public void SetLevel(EntityBuilder entitiesBuilder, LevelOptions options)
        {

            // Do:
            // 
            // TODO: Load all entities, set up colliders.
            // options.PrewarmOptions.
        }
    }
}