using System;
#if FIREBASE_SDK
using Firebase.Crashlytics;
#endif
public class CustomException : Exception
{
    public override string StackTrace { get; }

    CustomException(string message, string stackTrace) : base(message)
    {
        StackTrace = stackTrace;
    }
    public static void Fire(string name, string log)
    {
#if FIREBASE_SDK
        Crashlytics.LogException(new CustomException($"{name}: {log}", name));
#endif
    }
}
