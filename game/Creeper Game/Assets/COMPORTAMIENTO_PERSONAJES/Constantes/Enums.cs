﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.COMPORTAMIENTO_PERSONAJES.Constantes
{
    public enum AIStates { Patrol, Alerted, Attack, Feeding, Dead, RunAway, Give, Rest, Steal, Heal }
    public enum AITargets { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio }

   
}
