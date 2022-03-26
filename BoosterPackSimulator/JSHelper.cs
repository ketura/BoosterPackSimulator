using Microsoft.JSInterop;

namespace BoosterPackSimulator
{
    public class JSHelper
    {
        private readonly IJSRuntime JS;

        public JSHelper(IJSRuntime js)
        {
            JS = js;
        }

        //public void SelectElementByID(string id)
        //{
        //    await JsRuntime.InvokeVoidAsync("elementId")
        //}
    }
}
