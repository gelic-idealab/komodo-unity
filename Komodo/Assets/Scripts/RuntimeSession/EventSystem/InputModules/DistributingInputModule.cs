///Script to use multiple inputmodules
//https://forum.unity.com/threads/multiple-processing-inputmodules.369578/
using UnityEngine.EventSystems;
using System.Reflection;
using System.Collections.Generic;

public class DistributingInputModule : BaseInputModule
{

    private List<BaseInputModule> GetInputModules()
    {
        EventSystem current = EventSystem.current;
        FieldInfo m_SystemInputModules = current.GetType().GetField("m_SystemInputModules",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return m_SystemInputModules.GetValue(current) as List<BaseInputModule>;
    }

    private void SetInputModules(List<BaseInputModule> inputModules)
    {
        EventSystem current = EventSystem.current;
        FieldInfo m_SystemInputModules = current.GetType().GetField("m_SystemInputModules",
            BindingFlags.NonPublic | BindingFlags.Instance);
        m_SystemInputModules.SetValue(current, inputModules);
    }

    public override void UpdateModule()
    {
        MethodInfo changeEventModuleMethod =
            EventSystem.current.GetType().GetMethod("ChangeEventModule",
            BindingFlags.NonPublic | BindingFlags.Instance, null,
            new[] { typeof(BaseInputModule) }, null);
        changeEventModuleMethod.Invoke(EventSystem.current, new object[] { this });
        EventSystem.current.UpdateModules();
        List<BaseInputModule> activeInputModules = GetInputModules();
        activeInputModules.Remove(this);
        activeInputModules.Insert(0, this);
        SetInputModules(activeInputModules);
    }

    public override void Process()
    {
        List<BaseInputModule> activeInputModules = GetInputModules();
        foreach (BaseInputModule module in activeInputModules)
        {
            if (module == this)
                continue;

            module.Process();
        }
    }
    public override string ToString()
    {
        var moduleStringList = new List<string>();
        foreach (var module in GetInputModules())
        {
            if (module == this)
                continue;

            moduleStringList.Add(module.ToString());
        }
        return string.Join("\n\n", moduleStringList);
    }
}

