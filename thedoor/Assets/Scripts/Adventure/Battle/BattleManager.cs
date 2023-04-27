using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;

namespace TheDoor.Main {
    public class BattleManager {
        public static PlayerRole PRole { get; private set; }
        public static EnemyRole ERole { get; private set; }
    }
}