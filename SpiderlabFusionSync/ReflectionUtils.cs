using System;
using System.Reflection;

namespace SpiderlabFusionSync
{
    public class ReflectionUtils
    {
        public static void InvokeMethod(object obj, string methodName, object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            method.Invoke(obj, parameters);
        }
        
        public static T InvokeMethod<T>(object obj, string methodName, object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return (T)method.Invoke(obj, parameters);
        }
        
        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return (T)field.GetValue(obj);
        }
    }
}