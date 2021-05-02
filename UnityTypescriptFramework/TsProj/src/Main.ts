﻿import Handler from "./Framework/Utils/Handler";
import {ExHandler} from "./Example/ExHandler";
import UnityTs from "./Framework/UnityTs";

const CS = require("csharp");
let Debug = CS.UnityEngine.Debug;

class Main {
    constructor() {
        UnityTs.init();
        //
        ExHandler.Run();
        UnityTs.Timer.loop(2000, this, () => {
            Debug.Log("timer call back")
        }, [1], true, false);
    }
}

new Main();