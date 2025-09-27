using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class LayerMask
    {
        private static ulong[] MaskBits;
        private static Dictionary<string, int> Names;
        private const int ARRAY_SIZE = sizeof(ulong) * 8;
        static LayerMask()
        {
            MaskBits = new ulong[ARRAY_SIZE];
            Names = new Dictionary<string, int>(ARRAY_SIZE);

            for (int i = 0; i < MaskBits.Length; i++)
            {
                MaskBits[i] = ulong.MaxValue;
            }
        }

        public static ulong GetMaskBits(int layer)
        {
            return MaskBits[layer];
        }

        public static int LayerToIndex(ulong layer)
        {
            return BitOperations.TrailingZeroCount(layer);
        }

        public static ulong LayerToBits(int index)
        {
            return 1UL << index;
        }

        public static bool AreValid(int layer, ulong mask)
        {
            return (LayerToBits(layer) & mask) != 0;
        }

        public static void TurnOff(int layerA, int layerB)
        {
            ModifyLayers(layerA, layerB, (mask, bit) => mask & ~bit);
        }

        public static void TurnOn(int layerA, int layerB)
        {
            ModifyLayers(layerA, layerB, (mask, bit) => mask | bit);
        }

        public static void TurnOff(string nameA, string nameB)
        {
            ModifyLayers(nameA, nameB, (mask, bit) => mask & ~bit);
        }

        public static void TurnOn(string nameA, string nameB)
        {
            ModifyLayers(nameA, nameB, (mask, bit) => mask | bit);
        }

        private static void ModifyLayers(string nameA, string nameB, Func<ulong, ulong, ulong> op)
        {
            ModifyLayers(NameToLayer(nameA), NameToLayer(nameB), op);
        }

        private static void ModifyLayers(int layerA, int layerB, Func<ulong, ulong, ulong> op)
        {
            MaskBits[layerA] = op(MaskBits[layerA], LayerToBits(layerB));
            MaskBits[layerB] = op(MaskBits[layerB], LayerToBits(layerA));
        }

        public static bool AreEnabled(int layerA, int layerB)
        {
            return (MaskBits[layerA] & LayerToBits(layerB)) != 0; // Checking just one since both MaskBits are in sync.
        }

        public static void AssignName(int layer, string name)
        {
            if (Names.ContainsKey(name))
            {
                Names[name] = layer;
            }
            else
            {
                Names.Add(name, layer);
            }
        }

        public static IReadOnlyList<KeyValuePair<string, int>> GetAllAssignedNames() 
        {
            var validNames = new List<KeyValuePair<string, int>>();

            foreach (var kvp in Names)
            {
                if(!string.IsNullOrEmpty(kvp.Key))
                {
                    validNames.Add(new KeyValuePair<string, int>(kvp.Key, kvp.Value));
                }
            }

            return validNames;
        }

        public static string LayerToName(int layer)
        {
            foreach (var kvp in Names)
            {
                if (kvp.Value == layer)
                    return kvp.Key;
            }
            return string.Empty;
        }

        public static int NameToLayer(string name)
        {
            if (Names.TryGetValue(name, out var layer))
            {
                return layer;
            }

            return 0;
        }

        public static ulong NameToBit(string name)
        {
            var layer = NameToLayer(name);

            if(layer > 0)
            {
                return LayerToBits(layer);
            }

            return 0;
        }
    }
}