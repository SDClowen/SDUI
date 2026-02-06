using SDUI.Controls;
using SDUI.Native.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static SDUI.Native.Windows.Methods;

namespace SDUI;

public class Application
{
    private static readonly List<UIWindowBase> _openForms = new();
    private static UIWindowBase _activeForm;

    public static IReadOnlyList<UIWindowBase> OpenForms => _openForms.AsReadOnly();

    public static UIWindowBase ActiveForm
    {
        get => _activeForm;
        internal set
        {
            if (_activeForm == value) return;
            _activeForm = value;
        }
    }

    internal static void RegisterForm(UIWindowBase form)
    {
        if (form == null || _openForms.Contains(form))
            return;

        _openForms.Add(form);
        _activeForm = form;
    }

    internal static void UnregisterForm(UIWindowBase form)
    {
        if (form == null)
            return;

        _openForms.Remove(form);

        if (_activeForm == form)
            _activeForm = _openForms.LastOrDefault();
    }

    internal static void SetActiveForm(UIWindowBase form)
    {
        if (form == null || !_openForms.Contains(form))
            return;

        _activeForm = form;
    }

    public static void Run(UIWindowBase window)
    {
		try
		{
            if (!window.IsHandleCreated)
                window.CreateHandle();

            window.Show();

            MSG msg;
            while (GetMessage(out msg, IntPtr.Zero, 0, 0) > 0)
            {
				try
				{
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
				catch (Exception e)
				{
                    Debug.WriteLine("Exception in message loop: " + e.ToString());
				}
            }
        }
		catch (Exception ex)
		{
            DefWindowProc(IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
            Debug.WriteLine("Exception in Application.Run: " + ex.ToString());
		}
    }
}
