using System;

namespace OsuQqBot.AttributedFunctions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class FunctionAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string functionName;

        // This is a positional argument
        public FunctionAttribute(string functionName = "")
        {
            this.functionName = functionName;

            // TODO: Implement code here

            //throw new NotImplementedException();
        }

        public string PositionalString
        {
            get { return functionName; }
        }

        // This is a named argument
        public int NamedInt { get; set; }
    }
}
