using UnityEngine;
using System.Reflection;
using System.Linq;
using System;

namespace TrueBingo
{
    public static class Extensions
    {
        private static BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

        public static T GetValue<T>(this object input, string name, params object[] methodParams)
        {
            if (GetMember(input, name, out MemberInfo member))
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:     return (T)(member as FieldInfo)     .GetValue(input);
                    case MemberTypes.Property:  return (T)(member as PropertyInfo)  .GetValue(input, null);
                    case MemberTypes.Method:    return (T)(member as MethodInfo)    .Invoke(input, methodParams);
                }
            }
            return default;
        }

        public static void InvokeMethod(this object input, string name, params object[] methodParams)
        {
            if (GetMember(input, name, out MemberInfo member))
                (member as MethodInfo).Invoke(input, methodParams);
        }

        public static void SetValue<T>(this object input, string name, T value)
        {
            if (GetMember(input, name, out MemberInfo member))
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:     (member as FieldInfo)   .SetValue(input, value); break;
                    case MemberTypes.Property:  (member as PropertyInfo).SetValue(input, value); break;
                }
            }
        }

        public static T GetMember<T>(this object input, string name) where T : MemberInfo
        {
            if (GetMember(input, name, out MemberInfo member))
                return (T)member;

            return default;
        }

        private static bool GetMember(this object input, string name, out MemberInfo member)
        {
            MemberInfo[] members = input.GetType().GetMember(name, flags);

            if (members != null && members.Length > 0)
            {
                member = members.First();
                return true;
            }
            Debug.LogError($"No Member \"{name}\" Could Be Found...");
            member = default;
            return false;
        }
    }
}
