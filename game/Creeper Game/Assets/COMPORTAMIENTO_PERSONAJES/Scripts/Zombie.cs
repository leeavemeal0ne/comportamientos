﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace comportamiento_personajes
{

    public abstract class Zombie : MonoBehaviour
    {

        public abstract void startToEat();
        public abstract void TakeDamage(int dmg);
        public abstract bool getIsDead();
        public abstract void notifyPeace();
        public abstract void leavePeace();
    }

}
