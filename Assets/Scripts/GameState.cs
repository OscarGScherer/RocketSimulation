using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
Classe statica para permitir que alguns components se comuniquem
nesse caso apenas serve para lidar com o fail state
*/
public static class GameState
{
    public static string failReason = "";
    private static bool _failed = false;
    public static bool gameStarted = false;
    public static bool failed
    {
        get => _failed;
        set {
            if(value != _failed && value) onFail.Invoke();
            _failed = value;
        }
    }
    public static UnityEvent onFail = new UnityEvent();
}
