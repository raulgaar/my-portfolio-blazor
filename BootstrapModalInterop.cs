using Microsoft.JSInterop;
using System.Threading.Tasks;

public class BootstrapModalInterop
{
    private readonly IJSRuntime _jsRuntime;

    public BootstrapModalInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task Show(string modalId)
    {
        await _jsRuntime.InvokeVoidAsync("BootstrapModalInterop.show", modalId);
    }

    public async Task Hide(string modalId)
    {
        await _jsRuntime.InvokeVoidAsync("BootstrapModalInterop.hide", modalId);
    }
}