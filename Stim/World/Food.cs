using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Stim.World
{
    public class Food
    {
        public Vector2 Location { get; set; }
        public float EatingRate { get; set; }
        public float BodySize { get; set; }
        public float BreedState { get; }
        public bool IsBreeding { get; }
    }
}
            