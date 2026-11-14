using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager I;
    public UIScreen[] screens;
    public string startScreen = "Title";

    Dictionary<string, UIScreen> map = new Dictionary<string, UIScreen>();
    Stack<UIScreen> history = new Stack<UIScreen>();
    UIScreen current;
    bool busy;

    void Awake()
    {
        I = this;
        foreach (var s in screens) { map[s.name] = s; s.gameObject.SetActive(false); }
    }

    void Start() { StartCoroutine(Show(map[startScreen], 1f)); }

    void Update()
    {
        // Android hardware back
        if (Input.GetKeyDown(KeyCode.Escape)) Back();
    }

    public void Open(string name)
    {
        if (busy || !map.ContainsKey(name)) return;
        StartCoroutine(OpenRoutine(map[name]));
    }

    public void Back()
    {
        if (busy || history.Count == 0) return;
        StartCoroutine(BackRoutine());
    }

    // Clears history, used by "Main Menu" from Results/Pause
    public void GoRoot(string name)
    {
        if (busy || !map.ContainsKey(name)) return;
        history.Clear();
        StartCoroutine(RootRoutine(map[name]));
    }

    IEnumerator Show(UIScreen s, float dir)
    {
        busy = true;
        current = s;
        yield return s.In(dir);
        busy = false;
    }

    IEnumerator OpenRoutine(UIScreen next)
    {
        busy = true;
        history.Push(current);
        if (!next.isOverlay) yield return current.Out(1f);
        current = next;
        yield return next.In(1f);
        busy = false;
    }

    IEnumerator BackRoutine()
    {
        busy = true;
        var prev = history.Pop();
        bool wasOverlay = current.isOverlay;
        yield return current.Out(-1f);
        current = prev;
        if (!wasOverlay) yield return prev.In(-1f);
        busy = false;
    }

    IEnumerator RootRoutine(UIScreen root)
    {
        busy = true;
        // hide any overlays/screens still visible
        foreach (var s in screens)
            if (s != root && s.gameObject.activeSelf) yield return s.Out(1f);
        current = root;
        yield return root.In(-1f);
        busy = false;
    }
}