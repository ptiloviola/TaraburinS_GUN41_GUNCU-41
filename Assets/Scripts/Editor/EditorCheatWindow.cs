
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System;
using System.Reflection;


public class EditorCheatWindow : EditorWindow
{
    private EditorControls _controls;
    [MenuItem("CustomWindows/Editor Cheat Window")]
    public static void ShowWindow()
    {
        GetWindow<EditorCheatWindow>("CheatWindow");
    }

    private void OnEnable()
    {
        _controls = new EditorControls();
        _controls.Cheats.Enable();
        _controls.Cheats.NextTurn.performed += OnNextTurn;
        _controls.Cheats.Kill.performed += OnKill;
    }

    private void OnDisable()
    {
        if (_controls != null)
        {
            _controls.Cheats.NextTurn.performed -= OnNextTurn;
            _controls.Cheats.Kill.performed -= OnKill;
            _controls.Cheats.Disable();
        }
    }

    private void OnNextTurn(InputAction.CallbackContext context)
    {
        if (!EditorApplication.isPlaying) 
        {
            Debug.LogWarning("Cheats only work in PlayMode!");
            return;
        }
        Debug.Log($"cheat #1(NextTurn) used");
        ITurn turn = GetTurn();
        if (turn != null)
        {
            Debug.Log($"sharedData retrieved successfully. Active unit - {turn.Current}");
            turn.Next();
        }
        
    }

    private void OnKill(InputAction.CallbackContext context)
    {
        if (!EditorApplication.isPlaying) 
        {
            Debug.LogWarning("Cheats only work in PlayMode!");
            return;
        }
        Debug.Log($"cheat #2(Kill) used");
        ISharedData sharedData = GetSharedData();
        if (sharedData != null)
        {
            Debug.Log($"sharedData retrieved successfully. Active unit - {sharedData.ActiveUnit}");
            if (sharedData.Status == GameStatus.Move)
            {
                sharedData.Status = GameStatus.Attack;
            }
            
        }
    }

    private ISharedData GetSharedData()
    {
        var battleController = FindObjectOfType<BattleController>();
        if (battleController == null)
        {
            Debug.Log("BattleControl not found on scene");
            return null;
        }

        Type type = battleController.GetType();

        var sharedDataField = type.GetField("_data", BindingFlags.Instance | BindingFlags.NonPublic);
        ISharedData sharedData = (ISharedData)sharedDataField.GetValue(battleController);
        return sharedData;
    }

    private ITurn GetTurn()
    {
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.Log("PlayerController not found on scene");
            return null;
        }
        Type type = playerController.GetType();
        var playerControllerField = type.GetField("_turn", BindingFlags.Instance | BindingFlags.NonPublic);
        ITurn turn = (ITurn)playerControllerField.GetValue(playerController);
        return turn;
    }

}
