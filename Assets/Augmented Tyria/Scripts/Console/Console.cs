﻿using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class Console : MonoBehaviour, IActionable
{
    public ConsoleMessage MessagePrefab;
    public UserConfigHolder UserConfigHolder;

    public const float DefaultFadeTime = 4;

    public bool Hidden { get; private set; }

    public UserConfig UserConfig { get { return this.UserConfigHolder.UserConfig; } }
    Console IActionable.Console { get { return this; } }

    public string InputGroupName { get { return "Console"; } }
    public Dictionary<string, Action> Actions
    {
        get
        {
            return new Dictionary<string, Action>
            {
                { "ToggleHide", this.ToggleHide },
                { "Clear", this.Clear }
            };
        }
    }

    private IKeyboardMouseEvents globalHook;

    public CanvasGroup CanvasGroup { get; private set; }

    private void Awake()
    {
        this.CanvasGroup = this.GetComponent<CanvasGroup>();

        this.globalHook = Hook.GlobalEvents();
        this.globalHook.KeyDown += this.GlobalHookKeyDown;
    }

    private void OnDestroy()
    {
        this.globalHook.KeyDown -= this.GlobalHookKeyDown;
        this.globalHook.Dispose();
    }

    private void LateUpdate()
    {
        this.CanvasGroup.alpha = this.Hidden || this.transform.childCount == 0 ? 0 : 1;
    }

    public void Message(ConsoleMessageType type, string message, float fadeOutTime = -1)
    {
        if (this.UserConfig.ConsoleFilter > (int)type)
            return;
        ConsoleMessage msg = Instantiate(this.MessagePrefab.gameObject, this.transform).GetComponent<ConsoleMessage>();

        if (fadeOutTime >= 0)
            fadeOutTime += this.transform.GetComponentsInChildren<ConsoleMessage>().Max(cm => cm.FadeOutTimeLeft);

        msg.SetUp(type, message, fadeOutTime);
    }

    public void Info(bool fadeOut, string message, params object[] args)
    {
        this.Message(ConsoleMessageType.Info, string.Format(message, args), fadeOut ? DefaultFadeTime : -1);
    }

    public void Info(string message, params object[] args)
    {
        this.Info(false, message, args);
    }

    public void InfoFade(string message, params object[] args)
    {
        this.Info(true, message, args);
    }

    public void Warning(bool fadeOut, string message, params object[] args)
    {
        this.Message(ConsoleMessageType.Warning, string.Format(message, args), fadeOut ? DefaultFadeTime : -1);
    }

    public void Warning(string message, params object[] args)
    {
        this.Warning(false, message, args);
    }

    public void WarningFade(string message, params object[] args)
    {
        this.Warning(true, message, args);
    }

    public void Error(bool fadeOut, string message, params object[] args)
    {
        this.Message(ConsoleMessageType.Error, string.Format(message, args), fadeOut ? DefaultFadeTime : -1);
    }

    public void Error(string message, params object[] args)
    {
        this.Error(false, message, args);
    }

    public void ErrorFade(string message, params object[] args)
    {
        this.Error(true, message, args);
    }

    public void ToggleHide()
    {
        this.Hidden = !this.Hidden;
    }

    public void Clear()
    {
        if (this.CanvasGroup.alpha == 1)
            foreach (Transform t in this.transform)
                Destroy(t.gameObject);
    }

    private void GlobalHookKeyDown(object sender, KeyEventArgs e)
    {
        if (Camera.main.cullingMask == 0)
            return;

        this.Act(e.KeyCode, e.Control);
    }
}