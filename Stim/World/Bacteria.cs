using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Stim.World
{
    public class Bacteria
    {
        public float BodySize { get; set; }
        public Vector2 Location { get; set; }
        public Vector2 BodyMotion { get; set; }
        public float ReadyEnergy { get; set; }
        public float Temperature { get; set; }
        public float AgeHealth { get; set; }
        public Vector2 LightSensed { get; set; }
        public float FoodStore { get; set; }
        public float FoodFilterSize { get; set; }

        /// <summary>
        /// Calculated Burn rate given all current energy use.
        /// </summary>
        public float BurnRate { get; }
        public float BreedState { get; }
        public bool IsBreeding { get; }
        public int HistorySize { get; }
        public bool IsAlive { get; }

        //activity costs BodyMotion + AgeHealth friction + Temperature assist + Growth + Digestion + Breeding + Resting Burn Rate + Cognative costs

        //Internal Dials
        public float GrowthRate { get; set; }
        public float MotionRate { get; set; }
        public float EatingRate { get; set; }
        public float DigestionRate { get; set; }
        public float Metabolism { get; set; }
        public float CognativeRate { get; set; }
        public float TrainRate { get; set; }
        public float DecisionRate { get; set; }
    }
}
