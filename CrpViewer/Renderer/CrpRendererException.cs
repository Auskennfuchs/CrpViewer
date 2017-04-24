using System;
using System.Runtime.CompilerServices;

namespace CrpViewer.Renderer {
    class CrpRendererException : Exception{
        protected CrpRendererException(string message) : base(message)
        {
        }

        protected CrpRendererException(string message, Exception baseException) : base(message,baseException)
        {
        }

        protected CrpRendererException() : base()
        {
        }

        public static CrpRendererException Create(string message,
            Exception baseException,
            [CallerMemberName] string methodName = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string file = "") {
            file = file.Substring(file.LastIndexOf("\\") + 1);
            return new CrpRendererException(message + "\n" + file + "(" + lineNumber + "):" + methodName + "\n" + baseException.Message, baseException);

        }
        public static CrpRendererException Create(string message,
            [CallerMemberName] string methodName = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string file = "") {
            return CrpRendererException.Create(message, null, methodName, lineNumber, file);
        }
    }
}
